using System;
using System.Collections;
using System.IO;
using DG.Tweening;
using FMOD.Studio;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

public class Startup : MonoBehaviour
{
	[SerializeField]
	private VideoController m_videoPlayer;

	[SerializeField]
	private VideoMetadata m_introSplashVideo;

	[SerializeField]
	private AssetLoadingManager m_assetLoadingManager;

	[SerializeField]
	private StartupLoadingScreen m_loadingScreen;

	[SerializeField]
	private AssetReference m_defaultStartingScene;

	[SerializeField]
	private PlaytestNotice m_playtestNotice;

	[SerializeField]
	private StartupStep[] m_preServicesSteupSteps;

	[SerializeField]
	private StartupStep[] m_preStartGameSteps;

	[SerializeField]
	private GameObject m_epicEOSPrefab;

	[SerializeField]
	private bool m_showSplashInEditor;

	[SerializeField]
	private CanvasGroup m_logosCanvasGroup;

	[SerializeField]
	private AudioEvent m_doneStartupAudioEvent;

	[SerializeField]
	private GameObject m_engagementScreen;

	private EventInstance m_fmodEventInstance;

	private bool m_videoFinished;

	private float m_startTime;

	private bool m_loaded;

	private static bool s_alreadySeenSplashScreen;

	private static bool s_doneStartup;

	private bool m_shouldUseDebugConsole;

	private bool m_levelLoaded;

	public static bool DoneRegularStartup => s_doneStartup;

	public static void ClearStartupFlag()
	{
		s_doneStartup = false;
	}

	private void CommandLineArguments(bool isInitialStartup)
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		foreach (string text in commandLineArgs)
		{
			if (text.Equals("-clearsave") || text.Equals("-clearsaves"))
			{
				if (isInitialStartup)
				{
					Debug.Log("Clear save command line argument provided.");
					ClearSaveData();
				}
			}
			else
			{
				text.Equals("-console");
			}
		}
	}

	private void ClearSaveData()
	{
		string persistentDataPath = Application.persistentDataPath;
		Debug.Log("Clearing persistent data at: " + persistentDataPath);
		if (Directory.Exists(persistentDataPath))
		{
			string[] files = Directory.GetFiles(persistentDataPath);
			foreach (string text in files)
			{
				try
				{
					if (!text.Contains("Player.log") && !text.Contains("Player-prev.log"))
					{
						File.Delete(text);
					}
				}
				catch (Exception ex)
				{
					Debug.LogError("Failed to remove file from persistent data: " + ex.Message);
				}
			}
			files = Directory.GetDirectories(persistentDataPath);
			foreach (string path in files)
			{
				try
				{
					Directory.Delete(path, recursive: true);
				}
				catch (Exception ex2)
				{
					Debug.LogError("Failed to delete directory: " + ex2.Message);
				}
			}
			Debug.Log("Persistent data cleared.");
		}
		else
		{
			Debug.LogWarning("Persistent data path does not exist: " + persistentDataPath);
		}
	}

	private IEnumerator Start()
	{
		Debug.Log($"Startup. '{DateTime.UtcNow}']");
		bool isInitialStartup = !s_doneStartup;
		s_doneStartup = true;
		m_shouldUseDebugConsole = Application.isEditor;
		CommandLineArguments(isInitialStartup);
		m_logosCanvasGroup.alpha = 0f;
		m_engagementScreen.gameObject.SetActive(value: false);
		yield return new WaitForSeconds(0.1f);
		if (m_showSplashInEditor)
		{
			s_alreadySeenSplashScreen = false;
		}
		bool doFullFlow = !s_alreadySeenSplashScreen;
		if (doFullFlow)
		{
			m_videoPlayer.gameObject.SetActive(value: true);
			m_videoPlayer.StartVideoClip(m_introSplashVideo);
		}
		m_startTime = Time.unscaledTime;
		VideoController videoPlayer = m_videoPlayer;
		videoPlayer.OnVideoFinished = (UnityAction)Delegate.Combine(videoPlayer.OnVideoFinished, new UnityAction(VideoFinished));
		if (doFullFlow)
		{
			yield return new WaitUntil(() => m_videoFinished);
		}
		else
		{
			m_videoFinished = true;
		}
		m_loadingScreen.gameObject.SetActive(value: true);
		m_engagementScreen.gameObject.SetActive(value: true);
		if (doFullFlow)
		{
			yield return m_logosCanvasGroup.DOFade(1f, 1f).WaitForCompletion();
			yield return new WaitForSeconds(1f);
		}
		m_loadingScreen.ShowText("Initialise Steam");
		new GameObject("SteamManager").AddComponent<SteamManager>();
		yield return new WaitForEndOfFrame();
		yield return new WaitUntil(() => SteamManager.Initialized);
		if (m_playtestNotice != null)
		{
			UnityEngine.Object.Destroy(m_playtestNotice.gameObject);
		}
		m_loadingScreen.ShowText("Initialise Addressables");
		AsyncOperationHandle asyncOperationHandle = Addressables.InitializeAsync();
		yield return asyncOperationHandle;
		m_loadingScreen.ShowText("Running Pre-Services Steps");
		StartupStep[] preServicesSteupSteps = m_preServicesSteupSteps;
		foreach (StartupStep startupStep in preServicesSteupSteps)
		{
			yield return startupStep.Execute(m_loadingScreen);
		}
		m_loadingScreen.ShowText("Loading Global References");
		yield return GlobalReferences.LoadAsync();
		m_loadingScreen.ShowText("Loading User Preferences Manager");
		AsyncOperationHandle loadHandle = Addressables.LoadSceneAsync("UtilityScenes/UserPreferencesManager", LoadSceneMode.Additive);
		yield return new WaitUntil(() => loadHandle.IsDone);
		m_loadingScreen.ShowText("Loading Persistent Managers");
		loadHandle = Addressables.LoadSceneAsync("UtilityScenes/PersistentManagers", LoadSceneMode.Additive);
		yield return new WaitUntil(() => loadHandle.IsDone);
		yield return new WaitForEndOfFrame();
		GlobalReferences.Instance.EventChannels.Audio.TriggerMainMenuMusic.Raise();
		if (m_shouldUseDebugConsole)
		{
			m_loadingScreen.ShowText("Loading Debug Console");
			loadHandle = Addressables.LoadSceneAsync("UtilityScenes/DebugConsole", LoadSceneMode.Additive);
			yield return new WaitUntil(() => loadHandle.IsDone);
		}
		m_assetLoadingManager.LoadAssets();
		yield return new WaitUntil(() => m_assetLoadingManager.IsDoneLoading);
		m_loadingScreen.Hide();
		if (doFullFlow)
		{
			m_logosCanvasGroup.DOFade(0f, 1f).SetDelay(4f);
		}
		if (s_alreadySeenSplashScreen)
		{
			StartSetupAfterLogo();
		}
		else
		{
			GameInputManager.GameInputActions.Game.Proceed.performed += ProgressInputPressed;
			GameInputManager.GameInputActions.UI.Cancel.performed += ProgressInputPressed;
			GameInputManager.GameInputActions.UI.Submit.performed += ProgressInputPressed;
		}
		m_loaded = true;
		CheckForExit();
	}

	private void VideoFinished()
	{
		StartSetupAfterLogo();
	}

	private void ProgressInputPressed(InputAction.CallbackContext callback)
	{
		if (!(Time.unscaledTime - m_startTime < 0.5f) && callback.performed)
		{
			StartSetupAfterLogo();
		}
	}

	private void StartSetupAfterLogo()
	{
		if (!m_videoFinished)
		{
			m_videoFinished = true;
		}
	}

	private void CheckForExit()
	{
		if (m_videoFinished && m_loaded)
		{
			s_alreadySeenSplashScreen = true;
			StartCoroutine(StartGame());
		}
	}

	private void OnDisable()
	{
		if (GameInputManager.GameInputActions != null)
		{
			GameInputManager.GameInputActions.Game.Proceed.performed -= ProgressInputPressed;
			GameInputManager.GameInputActions.UI.Cancel.performed -= ProgressInputPressed;
			GameInputManager.GameInputActions.UI.Submit.performed -= ProgressInputPressed;
		}
		if (m_fmodEventInstance.isValid())
		{
			m_fmodEventInstance.stop(STOP_MODE.ALLOWFADEOUT);
		}
	}

	private void OnLevelLoaded()
	{
		m_levelLoaded = true;
	}

	private void PopulateInitialBackgroundLevelEvent(LevelTransitionEventData eventData, PersistentData data)
	{
		try
		{
			if (data != null)
			{
				eventData.m_targetSceneAssetReference = data.ActiveLevelMetadata.TargetSceneAssetReference;
				eventData.m_playerSpawnPosition = data.PlayerPosition;
				eventData.m_playerSetup = LevelTransitionPlayerSetup.AttachToPhone;
			}
			else
			{
				eventData.m_targetSceneAssetReference = m_defaultStartingScene;
				eventData.m_playerSetup = LevelTransitionPlayerSetup.GameIntroDiner;
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception thrown when in PopulateInitialBackgroundLevelEvent");
			Debug.LogError(ex.ToString());
			eventData.m_targetSceneAssetReference = m_defaultStartingScene;
			eventData.m_playerSetup = LevelTransitionPlayerSetup.GameIntroDiner;
		}
	}

	private IEnumerator StartGame()
	{
		Debug.Log("Build number: " + BuildInfo.NumberString);
		Debug.Log("Build date: " + BuildInfo.DateString);
		Debug.Log("Resolution is: " + Screen.currentResolution.ToString());
		Debug.Log("Current time UTC is: " + DateTime.UtcNow);
		if (m_shouldUseDebugConsole)
		{
			DebugConsole.SetTargetAssembly(typeof(Startup).Assembly);
		}
		Debug.Log("Running as steam build");
		GameObject obj = new GameObject("SteamManager");
		UnityEngine.Object.DontDestroyOnLoad(obj);
		obj.AddComponent<SteamManager>();
		m_levelLoaded = false;
		m_loadingScreen.ShowText("Running Pre-Start-Game Steps");
		StartupStep[] preStartGameSteps = m_preStartGameSteps;
		foreach (StartupStep startupStep in preStartGameSteps)
		{
			yield return startupStep.Execute(m_loadingScreen);
		}
		if ((bool)m_doneStartupAudioEvent)
		{
			m_doneStartupAudioEvent.Play2D();
		}
		m_loadingScreen.ShowText("Loading Profiles");
		SaveDataManager saveGameManager = SaveDataManager.Instance;
		yield return new WaitUntil(() => saveGameManager.ProfilesLoaded);
		LevelTransitionEventData levelEventData = new LevelTransitionEventData
		{
			m_type = LevelTransitionEventType.MenuBackground
		};
		m_loadingScreen.ShowText("Loading Current Save Data");
		SaveDataTempLoader tempLoader = saveGameManager.GetSaveDataTempLoader();
		tempLoader.LoadData();
		yield return new WaitUntil(() => tempLoader.IsDone);
		if (tempLoader.ResultCode == ResultCode.ExceptionThrown)
		{
			Debug.LogError("Failed to load existing save game!");
			saveGameManager.InvalidSaveDataStartup();
		}
		else
		{
			saveGameManager.LoadSaveFromStartup();
		}
		PopulateInitialBackgroundLevelEvent(levelEventData, tempLoader.ResultPersistentData);
		GlobalReferences.Instance.EventChannels.LevelTransition.LevelTransition.Raise(levelEventData);
		GlobalReferences.Instance.EventChannels.LevelTransition.LevelTransitionCompleted.Register(OnLevelLoaded);
		m_loadingScreen.ShowText("Loading Active Game Scene");
		yield return new WaitForEndOfFrame();
		yield return new WaitUntil(() => m_levelLoaded);
		GlobalReferences.Instance.EventChannels.LevelTransition.LevelTransitionCompleted.Unregister(OnLevelLoaded);
		GlobalReferences.Instance.GameMenuState.SetInMenu(GameMenuState.GameMenu.SystemMenu);
		AsyncOperationHandle loadHandle = Addressables.LoadSceneAsync("MenuScenes/SystemMenu", LoadSceneMode.Additive);
		yield return new WaitUntil(() => loadHandle.IsDone);
		GlobalReferences.Instance.EventChannels.Audio.TriggerMainMenuMusicTransition.Raise();
		SceneManager.UnloadSceneAsync("Assets/Scenes/Utility/Startup.unity");
	}
}
