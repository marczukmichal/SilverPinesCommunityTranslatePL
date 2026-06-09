using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Profiling;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
	[SerializeField]
	private AssetReference m_playerAssetReference;

	[SerializeField]
	private AssetReference m_boatPlayerAssetReference;

	[SerializeField]
	private MapDynamicData m_mapCompletionData;

	[SerializeField]
	private float m_minimumTransitionTime = 2f;

	private Scene m_activeScene;

	private SceneInstance m_activeSceneInstance;

	private static LevelTransitionEventData m_activeTransition;

	private AsyncOperationHandle<GameObject> m_playerPrefabHandle;

	private AsyncOperationHandle<GameObject> m_boatPlayerPrefabHandle;

	private static bool m_levelTranstionActive;

	private static readonly string[] s_utilityScenes = new string[9] { "UtilityScenes/UserPreferencesManager", "UtilityScenes/PersistentManagers", "UtilityScenes/GameManagers", "UtilityScenes/GameUI", "MenuScenes/InGameMenu", "MenuScenes/SystemMenu", "UtilityScenes/ComicViewer", "MenuScenes/GameOverMenu", "MenuScenes/SaveGameMenu" };

	public static LevelTransitionEventData ActiveTransition => m_activeTransition;

	public static bool LevelTransitionActive => m_levelTranstionActive;

	[UsedImplicitly]
	public bool IsLevelTransitionActive => m_levelTranstionActive;

	[UsedImplicitly]
	public bool IsInActiveGameState => GlobalReferences.Instance.GameState.IsInState(GameState.GameStateFlag.GameActive);

	[UsedImplicitly]
	public bool IsColdStartupOngoing
	{
		get
		{
			ColdStartup coldStartup = UnityEngine.Object.FindFirstObjectByType<ColdStartup>();
			if (coldStartup != null)
			{
				return coldStartup.IsColdStartupOngoing;
			}
			return false;
		}
	}

	private int PreLoadUnloadThreshold => 0;

	private int AfterLoadUnloadThreshold => 2500;

	private void OnEnable()
	{
		m_levelTranstionActive = false;
		m_activeTransition = null;
		GlobalReferences.Instance.EventChannels.LevelTransition.LevelTransition.Register(OnLevelTransitionRequest);
		GlobalReferences.Instance.EventChannels.Generic.ColdStartupSetupScene.Register(ColdStartupSceneSetup);
		GlobalReferences.Instance.EventChannels.LevelTransition.TriggerStartGameInScene.Register(OnTriggerStartGameInScene);
	}

	private void OnDisable()
	{
		m_activeTransition = null;
		GlobalReferences.Instance.EventChannels.LevelTransition.LevelTransition.Unregister(OnLevelTransitionRequest);
		GlobalReferences.Instance.EventChannels.Generic.ColdStartupSetupScene.Unregister(ColdStartupSceneSetup);
		GlobalReferences.Instance.EventChannels.LevelTransition.TriggerStartGameInScene.Unregister(OnTriggerStartGameInScene);
	}

	private void OnDestroy()
	{
		if (m_playerPrefabHandle.IsValid())
		{
			Addressables.ReleaseInstance(m_playerPrefabHandle);
		}
		if (m_boatPlayerPrefabHandle.IsValid())
		{
			Addressables.ReleaseInstance(m_boatPlayerPrefabHandle);
		}
	}

	private void OnLevelTransitionRequest(LevelTransitionEventData transitionEvent)
	{
		if (m_activeTransition != null)
		{
			Debug.LogWarning("Ignored scene transition because a scene transition is already in progress! Currently transitioning to: " + m_activeTransition.m_targetSceneName + "- tried to transition to: " + transitionEvent.m_targetSceneName);
			return;
		}
		m_activeTransition = transitionEvent;
		StartCoroutine(LevelTransitionCoroutine(m_activeTransition));
	}

	private IEnumerator UnloadAndGC()
	{
		long startTicks = DateTime.UtcNow.Ticks;
		AsyncOperation unloadUnusedAsset = Resources.UnloadUnusedAssets();
		yield return new WaitUntil(() => unloadUnusedAsset.isDone);
		GC.Collect();
		Debug.Log("LevelTransition: Resource unloading took " + new TimeSpan(DateTime.UtcNow.Ticks - startTicks).TotalMilliseconds + " milliseconds");
	}

	private IEnumerator LevelTransitionCoroutine(LevelTransitionEventData levelTransition)
	{
		Debug.Log("Transitioning to scene: " + levelTransition.m_targetSceneName);
		GlobalReferences.Instance.EventChannels.LevelTransition.LevelTransitionStarted.Raise();
		m_levelTranstionActive = true;
		PlayerInputOverride value = PlayerInputOverride.None;
		if (levelTransition.m_leaveDirection == LevelTransition.LeaveDirection.ToRight)
		{
			value = ((!levelTransition.m_isSprinting) ? PlayerInputOverride.WalkRight : PlayerInputOverride.SprintRight);
		}
		else if (levelTransition.m_leaveDirection == LevelTransition.LeaveDirection.ToLeft)
		{
			value = (levelTransition.m_isSprinting ? PlayerInputOverride.SprintLeft : PlayerInputOverride.WalkLeft);
		}
		switch (levelTransition.m_fadeType)
		{
		case LevelTransitionFadeType.Normal:
			GlobalReferences.Instance.EventChannels.Generic.FadeScreen.Raise(value: true);
			break;
		case LevelTransitionFadeType.White:
			GlobalReferences.Instance.EventChannels.Generic.ScreenFadeOfType.Raise(FadeType.White);
			break;
		case LevelTransitionFadeType.Red:
			GlobalReferences.Instance.EventChannels.Generic.ScreenFadeOfType.Raise(FadeType.RedDoor);
			break;
		}
		GlobalReferences.Instance.EventChannels.Generic.PlayerInputOverride.Raise(value);
		if (levelTransition.m_leaveDirection == LevelTransition.LeaveDirection.DisableCharacter)
		{
			GameObject item = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item;
			if (item != null)
			{
				UnityEngine.Object.Destroy(item);
			}
		}
		if (levelTransition.m_leavingTransitionAudioEvent != null)
		{
			levelTransition.m_leavingTransitionAudioEvent.Play(GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item.transform.position);
		}
		if (levelTransition.m_fadeType == LevelTransitionFadeType.Ignore)
		{
			yield return new WaitForSecondsRealtime(0.25f);
		}
		else
		{
			yield return new WaitForSecondsRealtime(FadeOverlay.BaseFadeTime);
		}
		long sceneLoadStartTicks = DateTime.UtcNow.Ticks;
		float beforeLoadTime = Time.unscaledTime;
		bool shouldLoadScene = true;
		if (m_activeScene.IsValid() && levelTransition.m_type == LevelTransitionEventType.Transition && levelTransition.m_targetSceneName != null && levelTransition.m_targetSceneName.Equals(m_activeScene.path))
		{
			shouldLoadScene = false;
		}
		long startTicks3;
		if (shouldLoadScene)
		{
			startTicks3 = DateTime.UtcNow.Ticks;
			if (m_activeSceneInstance.Scene.IsValid())
			{
				AsyncOperationHandle<SceneInstance> unloadScene2 = Addressables.UnloadSceneAsync(m_activeSceneInstance);
				yield return new WaitUntil(() => unloadScene2.IsDone);
			}
			else if (m_activeScene.IsValid())
			{
				AsyncOperation unloadScene = SceneManager.UnloadSceneAsync(m_activeScene);
				yield return new WaitUntil(() => unloadScene.isDone);
			}
			Debug.Log("LevelTransition: Current scene took " + new TimeSpan(DateTime.UtcNow.Ticks - startTicks3).TotalMilliseconds + " milliseconds to unload");
		}
		GlobalReferences.Instance.EventChannels.LevelTransition.LevelUnloaded.Raise();
		EventInstance transitionAudioEventInstance = default(EventInstance);
		if (!levelTransition.m_transitionAudioEvent.IsNull)
		{
			transitionAudioEventInstance = RuntimeManager.CreateInstance(levelTransition.m_transitionAudioEvent);
			transitionAudioEventInstance.start();
		}
		long memoryUsageMB2 = Profiler.GetTotalAllocatedMemoryLong() / 1048576;
		Debug.Log("Memory usage before unload: " + memoryUsageMB2 + " MB");
		if (memoryUsageMB2 > PreLoadUnloadThreshold)
		{
			yield return UnloadAndGC();
			long num = Profiler.GetTotalAllocatedMemoryLong() / 1048576;
			Debug.Log("Memory freed: " + (memoryUsageMB2 - num) + "MB");
		}
		float totalLoadTime = 0f;
		startTicks3 = DateTime.UtcNow.Ticks;
		AsyncOperationHandle<IList<IResourceLocation>> sceneResourceLocation = Addressables.LoadResourceLocationsAsync((levelTransition.m_targetSceneAssetReference != null) ? ((object)levelTransition.m_targetSceneAssetReference) : ((object)levelTransition.m_targetSceneName));
		yield return sceneResourceLocation;
		IList<IResourceLocation> result = sceneResourceLocation.Result;
		Debug.Log("LevelTransition: LoadResourceLocationsAsync took " + new TimeSpan(DateTime.UtcNow.Ticks - startTicks3).TotalMilliseconds + " milliseconds");
		if (result.Count == 0)
		{
			Debug.LogError("Scene not found! Not included in AssetBundles? " + levelTransition.m_targetSceneName);
			yield return null;
		}
		string sceneResourcePath = result[0].InternalId;
		if (levelTransition.m_levelMetadata != null)
		{
			GlobalReferences.Instance.Anchors.Generic.ActiveLevelMetadata.Set(levelTransition.m_levelMetadata);
		}
		if (shouldLoadScene)
		{
			startTicks3 = DateTime.UtcNow.Ticks;
			if (levelTransition.m_type != LevelTransitionEventType.MenuBackground)
			{
				GlobalReferences.Instance.EventChannels.LevelTransition.ShowLoadingScreen.Raise(value: true);
			}
			_ = DateTime.UtcNow.Ticks;
			AsyncOperationHandle<SceneInstance> loadHandle;
			if (levelTransition.m_targetSceneAssetReference != null)
			{
				loadHandle = Addressables.LoadSceneAsync(levelTransition.m_targetSceneAssetReference, LoadSceneMode.Additive, activateOnLoad: false);
			}
			else
			{
				loadHandle = Addressables.LoadSceneAsync(levelTransition.m_targetSceneName, LoadSceneMode.Additive, activateOnLoad: false);
			}
			Debug.Log("LevelTransition: Scene LoadSceneAsync time is " + new TimeSpan(DateTime.UtcNow.Ticks - startTicks3).TotalMilliseconds + " milliseconds");
			yield return new WaitUntil(() => loadHandle.IsDone);
			_ = DateTime.UtcNow.Ticks;
			yield return loadHandle.Result.ActivateAsync();
			Debug.Log("LevelTransition: Scene activation time is " + new TimeSpan(DateTime.UtcNow.Ticks - startTicks3).TotalMilliseconds + " milliseconds");
			m_activeSceneInstance = loadHandle.Result;
			float unscaledTime = Time.unscaledTime;
			totalLoadTime = unscaledTime - beforeLoadTime;
			long ticks = DateTime.UtcNow.Ticks - startTicks3;
			TimeSpan timeSpan = new TimeSpan(ticks);
			Debug.Log("LevelTransition: Scene " + levelTransition.m_targetSceneName + " took " + timeSpan.TotalMilliseconds + "milliseconds to load");
			GlobalReferences.Instance.EventChannels.LevelTransition.ShowLoadingScreen.Raise(value: false);
		}
		float num2 = m_minimumTransitionTime - totalLoadTime;
		if (num2 > 0f)
		{
			Debug.Log("Scene loading time expanded to meet minimum transition time by " + num2 + " seconds");
			yield return new WaitForSecondsRealtime(num2);
		}
		Debug.Log("LevelTransition: Total scene loading time is " + new TimeSpan(DateTime.UtcNow.Ticks - sceneLoadStartTicks).TotalMilliseconds + " milliseconds");
		if (transitionAudioEventInstance.isValid())
		{
			while (true)
			{
				transitionAudioEventInstance.getPlaybackState(out var state);
				if (state != 0)
				{
					break;
				}
				yield return new WaitForSeconds(0.05f);
			}
		}
		LevelTransition.ArrivalDirection arrivalDirection = LevelTransition.ArrivalDirection.None;
		LevelTransition.ArrivalState arrivalState = LevelTransition.ArrivalState.Normal;
		AudioEvent arriveAudioEvent = null;
		Vector3 levelTransitionPosition = Vector3.zero;
		LevelTransition arriveLevelTransition = null;
		PersistentDataIdentifier[] array = UnityEngine.Object.FindObjectsByType<PersistentDataIdentifier>(FindObjectsSortMode.None);
		for (int i = 0; i < array.Length; i++)
		{
			array[i].UpdateFromDatastore();
		}
		if (!string.IsNullOrEmpty(levelTransition.m_targetEntryPoint))
		{
			GameObject gameObject = GameObject.Find(levelTransition.m_targetEntryPoint);
			if (gameObject == null)
			{
				gameObject = GameObject.Find("Old_" + levelTransition.m_targetEntryPoint);
			}
			if (gameObject != null)
			{
				ManholeLevelTransitionTarget component = gameObject.GetComponent<ManholeLevelTransitionTarget>();
				arriveLevelTransition = ((!(component != null)) ? gameObject.GetComponent<LevelTransition>() : component.LevelTransition);
				if (arriveLevelTransition != null)
				{
					arrivalDirection = arriveLevelTransition.TransitionArrivalDirection;
					arrivalState = arriveLevelTransition.TransitionArrivalState;
					arriveAudioEvent = arriveLevelTransition.ArriveAudioEvent;
					levelTransitionPosition = arriveLevelTransition.transform.position;
				}
			}
		}
		if (!m_playerPrefabHandle.IsValid())
		{
			m_playerPrefabHandle = Addressables.LoadAssetAsync<GameObject>(m_playerAssetReference);
		}
		yield return new WaitUntil(() => m_playerPrefabHandle.IsDone);
		SetupLoadedScene(SceneManager.GetSceneByPath(sceneResourcePath), !shouldLoadScene, levelTransition, arrivalDirection, arrivalState);
		PlayerInputOverride arrivalInputOverride = PlayerInputOverride.None;
		switch (arrivalDirection)
		{
		case LevelTransition.ArrivalDirection.MoveRight:
			arrivalInputOverride = ((!levelTransition.m_isSprinting) ? PlayerInputOverride.WalkRight : PlayerInputOverride.SprintRight);
			break;
		case LevelTransition.ArrivalDirection.MoveLeft:
			arrivalInputOverride = (levelTransition.m_isSprinting ? PlayerInputOverride.SprintLeft : PlayerInputOverride.WalkLeft);
			break;
		case LevelTransition.ArrivalDirection.LadderUp:
			arrivalInputOverride = PlayerInputOverride.Up;
			break;
		case LevelTransition.ArrivalDirection.LadderDown:
			arrivalInputOverride = PlayerInputOverride.Down;
			break;
		}
		if (arrivalInputOverride != 0 && arrivalInputOverride != PlayerInputOverride.Static)
		{
			GlobalReferences.Instance.EventChannels.Generic.PlayerInputOverride.Raise(arrivalInputOverride);
		}
		if (arriveLevelTransition != null)
		{
			arriveLevelTransition.DoArriveEvent();
		}
		memoryUsageMB2 = Profiler.GetTotalAllocatedMemoryLong() / 1048576;
		Debug.Log("Memory usage after load: " + memoryUsageMB2 + " MB");
		if (memoryUsageMB2 > AfterLoadUnloadThreshold)
		{
			yield return UnloadAndGC();
			long num3 = Profiler.GetTotalAllocatedMemoryLong() / 1048576;
			Debug.Log("Memory freed: " + (memoryUsageMB2 - num3) + " MB");
		}
		m_activeTransition = null;
		yield return new WaitForSecondsRealtime(0.1f);
		if (levelTransition.m_fadeType != LevelTransitionFadeType.Ignore)
		{
			GlobalReferences.Instance.EventChannels.Generic.FadeScreen.Raise(value: false);
		}
		if (arriveAudioEvent != null)
		{
			StartCoroutine(DelayedAudioEvent(arriveAudioEvent, levelTransitionPosition, 0.25f));
		}
		if (arrivalInputOverride != 0 && arrivalInputOverride != PlayerInputOverride.Static)
		{
			float time = 0.6f;
			if (levelTransition.m_isSprinting)
			{
				time = 0.5f;
			}
			if (arriveLevelTransition != null && arriveLevelTransition.ArriveForceMoveTimeOverride >= 0f)
			{
				time = arriveLevelTransition.ArriveForceMoveTimeOverride;
			}
			else if (arrivalInputOverride == PlayerInputOverride.Up || arrivalInputOverride == PlayerInputOverride.Down)
			{
				time = 1f;
			}
			yield return new WaitForSecondsRealtime(time);
			GlobalReferences.Instance.EventChannels.Generic.PlayerInputOverride.Raise(PlayerInputOverride.None);
		}
		GlobalReferences.Instance.EventChannels.LevelTransition.NearbyLevelTransitionObject.Raise(null);
		GlobalReferences.Instance.EventChannels.LevelTransition.EnableLevelTransitionTriggers.Raise();
		yield return new WaitForEndOfFrame();
		if (levelTransition.m_type == LevelTransitionEventType.DebugColdStartup)
		{
			ColdStartup coldStartup = UnityEngine.Object.FindAnyObjectByType<ColdStartup>();
			if (coldStartup != null)
			{
				coldStartup.PerformColdGameStartup();
			}
		}
		GlobalReferences.Instance.EventChannels.LevelTransition.LevelTransitionCompleted.Raise();
		if (levelTransition.m_type == LevelTransitionEventType.LoadGame)
		{
			GlobalReferences.Instance.EventChannels.LevelTransition.TriggerStartGameInScene.Raise();
		}
		m_levelTranstionActive = false;
	}

	private IEnumerator DelayedAudioEvent(AudioEvent audioEvent, Vector3 position, float delay)
	{
		yield return new WaitForSecondsRealtime(delay);
		audioEvent.Play(position);
	}

	private void ColdStartupSceneSetup()
	{
		SetupLoadedScene(SceneManager.GetActiveScene(), moveExistingPlayer: false);
		GlobalReferences.Instance.EventChannels.LevelTransition.EnableLevelTransitionTriggers.Raise();
	}

	private void SetupLoadedScene(Scene scene, bool moveExistingPlayer, LevelTransitionEventData transition = null, LevelTransition.ArrivalDirection arrivalDirection = LevelTransition.ArrivalDirection.None, LevelTransition.ArrivalState arrivalState = LevelTransition.ArrivalState.Normal)
	{
		SceneManager.SetActiveScene(scene);
		m_activeScene = scene;
		bool flag = true;
		bool value = false;
		LevelMetadata.MetadataLevelType levelType = LevelMetadata.MetadataLevelType.Normal;
		LevelMetadataReference levelMetadataReference = UnityEngine.Object.FindAnyObjectByType<LevelMetadataReference>();
		if (levelMetadataReference != null && levelMetadataReference.gameObject.scene == scene)
		{
			if (levelMetadataReference.Metadata.LevelType == LevelMetadata.MetadataLevelType.Cutscene)
			{
				flag = false;
				value = true;
			}
			levelType = levelMetadataReference.Metadata.LevelType;
			GlobalReferences.Instance.EventChannels.LevelTransition.SetupLevel.Raise(levelMetadataReference.Metadata);
		}
		if (transition != null && transition.m_type == LevelTransitionEventType.DebugColdStartup)
		{
			flag = false;
		}
		GlobalReferences.Instance.EventChannels.Generic.SetCutsceneIsPlaying.Raise(value);
		GlobalReferences.Instance.Anchors.FieldOfView.FieldOfViewLevelSettingsAnchor.Set(null);
		GlobalReferences.Instance.TimeOfDay.ApplyQueuedChange();
		if (flag)
		{
			SpawnPlayerInScene(moveExistingPlayer, transition, arrivalDirection, arrivalState, levelType);
		}
	}

	private void SpawnPlayerInScene(bool moveExistingPlayer, LevelTransitionEventData transition, LevelTransition.ArrivalDirection arrivalDirection, LevelTransition.ArrivalState arrivalState, LevelMetadata.MetadataLevelType levelType)
	{
		Vector3 vector = Vector3.zero;
		LevelTransitionDoor levelTransitionDoor = null;
		LevelTransition levelTransition = null;
		LedgeGrabLevelTransition ledgeGrabLevelTransition = null;
		if (transition != null)
		{
			switch (transition.m_type)
			{
			case LevelTransitionEventType.Transition:
			{
				GameObject gameObject = GameObject.Find(transition.m_targetEntryPoint);
				if (gameObject != null)
				{
					ManholeLevelTransitionTarget component = gameObject.GetComponent<ManholeLevelTransitionTarget>();
					if (component != null)
					{
						gameObject = component.DoLevelTransition().gameObject;
					}
					levelTransitionDoor = gameObject.GetComponent<LevelTransitionDoor>();
					levelTransition = gameObject.GetComponent<LevelTransition>();
					ledgeGrabLevelTransition = gameObject.GetComponent<LedgeGrabLevelTransition>();
					vector = ((!(ledgeGrabLevelTransition != null)) ? GetPositionForLevelTransition(gameObject, arrivalDirection, levelTransitionDoor) : ledgeGrabLevelTransition.EntryPosition);
				}
				break;
			}
			case LevelTransitionEventType.LoadGame:
				vector = transition.m_playerSpawnPosition;
				break;
			case LevelTransitionEventType.MenuBackground:
				if (transition.m_playerSetup != LevelTransitionPlayerSetup.GameIntroDiner)
				{
					vector = transition.m_playerSpawnPosition;
				}
				break;
			}
		}
		CharacterDirection.Facing currentDirection = CharacterDirection.Facing.None;
		if (vector == Vector3.zero)
		{
			GameObject gameObject2 = GameObject.FindWithTag(Tags.SpawnPoint);
			if (gameObject2 != null)
			{
				vector = gameObject2.transform.position;
				SpawnPointSettings component2 = gameObject2.GetComponent<SpawnPointSettings>();
				if (component2 != null)
				{
					currentDirection = component2.StartFacingDirection;
				}
			}
		}
		vector = RayCastForFloor(vector, arrivalState);
		switch (arrivalState)
		{
		case LevelTransition.ArrivalState.LadderAttach:
			switch (arrivalDirection)
			{
			case LevelTransition.ArrivalDirection.LadderDown:
				vector.y -= 1f;
				break;
			case LevelTransition.ArrivalDirection.LadderUp:
				vector.y -= 2f;
				break;
			}
			break;
		case LevelTransition.ArrivalState.Ledge:
			if (ledgeGrabLevelTransition != null)
			{
				vector.y += ledgeGrabLevelTransition.SpawnDistanceFromGround;
			}
			break;
		}
		if (moveExistingPlayer && GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item != null)
		{
			UnityEngine.Object.Destroy(GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item);
			GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Set(null);
		}
		if (levelType == LevelMetadata.MetadataLevelType.BoatTravel)
		{
			if (!m_boatPlayerPrefabHandle.IsValid())
			{
				m_boatPlayerPrefabHandle = Addressables.LoadAssetAsync<GameObject>(m_boatPlayerAssetReference);
				m_boatPlayerPrefabHandle.WaitForCompletion();
			}
			GameObject gameObject3 = UnityEngine.Object.Instantiate(m_boatPlayerPrefabHandle.Result, vector, Quaternion.identity);
			gameObject3.name = "Player - Boat";
			GlobalReferences.Instance.EventChannels.Player.PlayerSpawned.Raise(gameObject3);
			return;
		}
		if (!m_playerPrefabHandle.IsValid())
		{
			m_playerPrefabHandle = Addressables.LoadAssetAsync<GameObject>(m_playerAssetReference);
			m_playerPrefabHandle.WaitForCompletion();
		}
		GameObject gameObject4 = UnityEngine.Object.Instantiate(m_playerPrefabHandle.Result, vector, Quaternion.identity);
		gameObject4.name = "Player";
		CharacterDirection component3 = gameObject4.GetComponent<CharacterDirection>();
		if (transition != null)
		{
			PlayMakerFSM component4 = gameObject4.GetComponent<PlayMakerFSM>();
			if (transition.m_playerSetup != 0)
			{
				switch (transition.m_playerSetup)
				{
				case LevelTransitionPlayerSetup.GameIntroDiner:
					currentDirection = CharacterDirection.Facing.Left;
					if (component4 != null)
					{
						component4.SendEvent("Cutscene/DinerAwake");
					}
					break;
				case LevelTransitionPlayerSetup.AttachToPhone:
					currentDirection = CharacterDirection.Facing.Right;
					if (component4 != null)
					{
						component4.SendEvent("Cutscene/UsePhone");
					}
					break;
				}
			}
			else
			{
				if (component3 != null)
				{
					if (arrivalState == LevelTransition.ArrivalState.Ledge && ledgeGrabLevelTransition != null)
					{
						currentDirection = ledgeGrabLevelTransition.EnterDirection;
					}
					else
					{
						switch (arrivalDirection)
						{
						case LevelTransition.ArrivalDirection.MoveLeft:
						case LevelTransition.ArrivalDirection.AwayFromCameraToLeft:
						case LevelTransition.ArrivalDirection.TowardsCameraToLeft:
						case LevelTransition.ArrivalDirection.TowardsCameraFromDoorToLeft:
							currentDirection = CharacterDirection.Facing.Left;
							break;
						case LevelTransition.ArrivalDirection.MoveRight:
						case LevelTransition.ArrivalDirection.AwayFromCameraToRight:
						case LevelTransition.ArrivalDirection.TowardsCameraToRight:
						case LevelTransition.ArrivalDirection.TowardsCameraFromDoorToRight:
							currentDirection = CharacterDirection.Facing.Right;
							break;
						}
					}
				}
				if (levelTransition != null && (arrivalDirection == LevelTransition.ArrivalDirection.TowardsCameraToLeft || arrivalDirection == LevelTransition.ArrivalDirection.TowardsCameraToRight || arrivalDirection == LevelTransition.ArrivalDirection.TowardsCameraFromDoorToLeft || arrivalDirection == LevelTransition.ArrivalDirection.TowardsCameraFromDoorToRight))
				{
					StartCoroutine(DelayedFixPlayerPosition(gameObject4, levelTransition));
				}
				if (component4 != null)
				{
					switch (arrivalDirection)
					{
					case LevelTransition.ArrivalDirection.AwayFromCameraToLeft:
					case LevelTransition.ArrivalDirection.AwayFromCameraToRight:
						component4.SendEvent("Transition/EnterAwayFromCamera");
						break;
					case LevelTransition.ArrivalDirection.TowardsCameraToLeft:
					case LevelTransition.ArrivalDirection.TowardsCameraToRight:
						component4.SendEvent("Transition/EnterTowardsCamera");
						break;
					case LevelTransition.ArrivalDirection.TowardsCameraFromDoorToLeft:
					case LevelTransition.ArrivalDirection.TowardsCameraFromDoorToRight:
						component4.SendEvent("Transition/EnterTowardsCameraDoor");
						break;
					default:
						if (levelTransitionDoor != null && (arrivalDirection == LevelTransition.ArrivalDirection.MoveLeft || arrivalDirection == LevelTransition.ArrivalDirection.MoveRight))
						{
							levelTransitionDoor.TempDisableCollision(0.75f);
							if (transition.m_isSprinting)
							{
								component4.SendEvent("Transition/EnterFromSideDoorSprinting");
							}
							else
							{
								component4.SendEvent("Transition/EnterFromSideDoor");
							}
						}
						break;
					}
					switch (arrivalState)
					{
					case LevelTransition.ArrivalState.LadderAttach:
					{
						CharacterTraversalUtils component5 = gameObject4.GetComponent<CharacterTraversalUtils>();
						component5.AttachToLadder(levelTransition.Ladder);
						component4.SendEvent("Ladder/ForceOntoLadderTransition");
						component5.AttachToLadder(levelTransition.Ladder);
						break;
					}
					case LevelTransition.ArrivalState.Ledge:
						component4.SendEvent("Ledge/LevelTransitionEnterFall");
						break;
					}
				}
			}
		}
		component3.CurrentDirection = currentDirection;
		GlobalReferences.Instance.EventChannels.Player.PlayerSpawned.Raise(gameObject4);
	}

	public static void LoadUtilityScenes()
	{
		string[] array = s_utilityScenes;
		foreach (string key in array)
		{
			AsyncOperationHandle<IList<IResourceLocation>> asyncOperationHandle = Addressables.LoadResourceLocationsAsync(key);
			asyncOperationHandle.WaitForCompletion();
			if (!SceneManager.GetSceneByPath(asyncOperationHandle.Result[0].InternalId).isLoaded)
			{
				((AsyncOperationHandle)Addressables.LoadSceneAsync(key, LoadSceneMode.Additive)).WaitForCompletion();
			}
		}
	}

	public static void LoadDebugConsole()
	{
		DebugConsole.SetTargetAssembly(typeof(LevelManager).Assembly);
		AsyncOperationHandle<IList<IResourceLocation>> asyncOperationHandle = Addressables.LoadResourceLocationsAsync("UtilityScenes/DebugConsole");
		asyncOperationHandle.WaitForCompletion();
		if (!SceneManager.GetSceneByPath(asyncOperationHandle.Result[0].InternalId).isLoaded)
		{
			((AsyncOperationHandle)Addressables.LoadSceneAsync("UtilityScenes/DebugConsole", LoadSceneMode.Additive)).WaitForCompletion();
		}
	}

	public static IEnumerator LoadUtilityScenesAsync()
	{
		string[] array = s_utilityScenes;
		foreach (string scene in array)
		{
			AsyncOperationHandle<IList<IResourceLocation>> loc = Addressables.LoadResourceLocationsAsync(scene);
			yield return loc;
			if (!SceneManager.GetSceneByPath(loc.Result[0].InternalId).isLoaded)
			{
				long startTicks = DateTime.UtcNow.Ticks;
				AsyncOperationHandle loadHandle = Addressables.LoadSceneAsync(scene, LoadSceneMode.Additive);
				yield return new WaitUntil(() => loadHandle.IsDone);
				long ticks = DateTime.UtcNow.Ticks - startTicks;
				TimeSpan timeSpan = new TimeSpan(ticks);
				Debug.Log("Scene " + scene + " took " + timeSpan.TotalMilliseconds + "milliseconds to load");
				yield return new WaitForEndOfFrame();
			}
		}
	}

	private static IEnumerator DelayedFixPlayerPosition(GameObject player, LevelTransition transition)
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		for (float timer = 0f; timer < 0.5f; timer += Time.deltaTime)
		{
			Vector3 position = player.transform.position;
			BaseInteractable component = transition.GetComponent<BaseInteractable>();
			if (component != null)
			{
				position.x = component.GetWalkToInteractPosition().x;
			}
			player.transform.position = position;
			yield return new WaitForEndOfFrame();
		}
	}

	private void OnTriggerStartGameInScene()
	{
		PlayMakerFSM[] array = UnityEngine.Object.FindObjectsByType<PlayMakerFSM>(FindObjectsSortMode.None);
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SendEvent("GameStart");
		}
	}

	public static Vector3 RayCastForFloor(Vector3 position, LevelTransition.ArrivalState arrivalState)
	{
		Vector3 vector = position;
		vector.y += 0.1f;
		RaycastHit2D raycastHit2D = Physics2D.Raycast(vector, Vector2.down, 10f, GameLayers.CharacterNavigationMask);
		if ((bool)raycastHit2D)
		{
			if (arrivalState == LevelTransition.ArrivalState.Normal || arrivalState == LevelTransition.ArrivalState.Crouching)
			{
				position = raycastHit2D.point;
			}
			position.z = raycastHit2D.transform.position.z;
		}
		return position;
	}

	public static Vector3 GetPositionForLevelTransition(GameObject entryPointGameObject, LevelTransition.ArrivalDirection arrivalDirection, LevelTransitionDoor transitionDoor)
	{
		Vector3 result = entryPointGameObject.transform.position;
		BaseInteractable component = entryPointGameObject.GetComponent<BaseInteractable>();
		Renderer component2 = entryPointGameObject.GetComponent<Renderer>();
		if (component != null)
		{
			result = component.GetWalkToInteractPosition(ignorePerpsectiveAdjustment: true);
		}
		else if (component2 != null)
		{
			result = component2.bounds.center;
		}
		if (transitionDoor != null)
		{
			switch (arrivalDirection)
			{
			case LevelTransition.ArrivalDirection.MoveLeft:
				result.x += 1f;
				break;
			case LevelTransition.ArrivalDirection.MoveRight:
				result.x -= 1f;
				break;
			}
		}
		return result;
	}

	public void ForceBackToMainMenu()
	{
		GlobalReferences.Instance.GameState.ClearStateFlag(GameState.GameStateFlag.GameActive);
		SceneManager.LoadScene("Assets/Scenes/Utility/Startup.unity");
	}
}
