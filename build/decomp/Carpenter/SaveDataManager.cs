using System.Collections;
using Team17;
using Team17.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class SaveDataManager : MonoBehaviour
{
	[SerializeField]
	private NewGameStartConfiguration m_newGameStartConfiguration;

	[SerializeField]
	private Image m_ioIconSprite;

	[SerializeField]
	private SharedSaveData m_sharedSaveData;

	private static float m_lastSaveTime = -1f;

	private bool m_hasSave;

	private SaveProfiles m_saveProfiles;

	private ISaveDataSerializer m_platformSerializer;

	private Texture2D m_thumbnailTexture;

	private int m_tempSaveSlotIndex;

	private bool m_isQuickLoadFlag;

	private bool m_reloadStateFlag;

	private int m_lastLoadedSaveSlot = -1;

	public SharedSaveData SharedData => m_sharedSaveData;

	public bool IsInitialized
	{
		get
		{
			if (m_platformSerializer != null)
			{
				return m_platformSerializer.IsInitialized();
			}
			return false;
		}
	}

	public static SaveDataManager Instance => GlobalReferences.Instance.Anchors.SaveData.SaveDataManagerAnchor.Item;

	public bool ProfilesLoaded => m_saveProfiles != null;

	public bool CurrentProfileHasSaveData => m_hasSave;

	public static void SetLastSaveTimeToNow()
	{
		m_lastSaveTime = Time.unscaledTime;
	}

	public static float GetTimeSinceLastSave()
	{
		return Time.unscaledTime - m_lastSaveTime;
	}

	private void Awake()
	{
		if (Services.TryGet<ISaveDataSerializer>(out var service))
		{
			m_platformSerializer = service;
			return;
		}
		m_platformSerializer = new PCSaveDataSerializer();
		Services.Register<ISaveDataSerializer>(m_platformSerializer);
	}

	private async Awaitable<bool> TryLoadSharedDataAwaitable()
	{
		AwaitableCompletionSource<DataLoadedCallbackEvent<SharedSaveData>> completionSource = new AwaitableCompletionSource<DataLoadedCallbackEvent<SharedSaveData>>();
		m_platformSerializer.LoadSharedSaveData(delegate(DataLoadedCallbackEvent<SharedSaveData> result)
		{
			completionSource.TrySetResult(in result);
		});
		DataLoadedCallbackEvent<SharedSaveData> dataLoadedCallbackEvent = await completionSource.Awaitable;
		switch (dataLoadedCallbackEvent.m_resultCode)
		{
		case ResultCode.Success:
			m_sharedSaveData = dataLoadedCallbackEvent.m_loadedObject;
			break;
		case ResultCode.FileDoesNotExist:
			m_sharedSaveData.SaveProfileIndex = 0;
			break;
		case ResultCode.FileVersionTooHigh:
			if (!(await Services.Get<Team17DialogService>().PromptUserLoadErrorDeleteOrCancelAsync(Services.Get<Team17DialogLocalizationService>().LoadSaveFileTooNewError)))
			{
				return false;
			}
			m_sharedSaveData.SaveProfileIndex = 0;
			break;
		default:
			if (!(await Services.Get<Team17DialogService>().PromptUserLoadErrorDeleteOrCancelAsync()))
			{
				return false;
			}
			m_sharedSaveData.SaveProfileIndex = 0;
			break;
		}
		m_sharedSaveData.SaveProfileIndex = Mathf.Clamp(m_sharedSaveData.SaveProfileIndex, 0, 2);
		return true;
	}

	public void SaveSharedData()
	{
		m_platformSerializer.SaveSharedSaveData(m_sharedSaveData, OnSaveSharedDataCallback);
	}

	private void OnSaveSharedDataCallback(DataSavedCallbackEvent result)
	{
		if (result.m_resultCode != ResultCode.Success)
		{
			Services.Get<Team17DialogService>().PromptUserFailedToSaveAsync();
		}
	}

	private void OnEnable()
	{
		GlobalReferences.Instance.Anchors.SaveData.SaveDataManagerAnchor.Set(this);
		GlobalReferences.Instance.EventChannels.SaveLoad.LoadMostRecentSave.Register(LoadMostRecentSave);
		GlobalReferences.Instance.EventChannels.SaveLoad.GameLoad.Register(LoadGameFromSlot);
		GlobalReferences.Instance.EventChannels.SaveLoad.GameSave.Register(SaveGame);
		GlobalReferences.Instance.EventChannels.SaveLoad.GameSaveInSlot.Register(SaveGameInSlot);
		GlobalReferences.Instance.EventChannels.SaveLoad.OnGameReloadedEvent.Register(GameReloaded);
		GlobalReferences.Instance.EventChannels.SaveLoad.DeleteProfile.Register(DeleteProfile);
		GlobalReferences.Instance.EventChannels.MainMenu.ChapterSelect.Register(ChapterSelectStartGame);
		if (m_ioIconSprite != null)
		{
			Color color = m_ioIconSprite.color;
			color.a = 0f;
			m_ioIconSprite.color = color;
		}
	}

	public async Awaitable<bool> TryLoadInitialFilesForEngagedUserAsync()
	{
		if (!(await TryLoadSharedDataAwaitable()))
		{
			return false;
		}
		if (!(await TryLoadProfilesMetadataAwaitable()))
		{
			return false;
		}
		return true;
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.Anchors.SaveData.SaveDataManagerAnchor.Set(null);
		GlobalReferences.Instance.EventChannels.SaveLoad.LoadMostRecentSave.Unregister(LoadMostRecentSave);
		GlobalReferences.Instance.EventChannels.SaveLoad.GameLoad.Unregister(LoadGameFromSlot);
		GlobalReferences.Instance.EventChannels.SaveLoad.GameSave.Unregister(SaveGame);
		GlobalReferences.Instance.EventChannels.SaveLoad.GameSaveInSlot.Unregister(SaveGameInSlot);
		GlobalReferences.Instance.EventChannels.SaveLoad.OnGameReloadedEvent.Unregister(GameReloaded);
		GlobalReferences.Instance.EventChannels.SaveLoad.DeleteProfile.Unregister(DeleteProfile);
		GlobalReferences.Instance.EventChannels.MainMenu.ChapterSelect.Unregister(ChapterSelectStartGame);
	}

	private void OnDestroy()
	{
		if ((bool)m_thumbnailTexture)
		{
			Object.Destroy(m_thumbnailTexture);
		}
	}

	private async Awaitable<bool> TryLoadProfilesMetadataAwaitable()
	{
		AwaitableCompletionSource<DataLoadedCallbackEvent<SaveProfiles>> completionSource = new AwaitableCompletionSource<DataLoadedCallbackEvent<SaveProfiles>>();
		m_platformSerializer.LoadProfilesMetadata(delegate(DataLoadedCallbackEvent<SaveProfiles> result)
		{
			completionSource.TrySetResult(in result);
		});
		DataLoadedCallbackEvent<SaveProfiles> dataLoadedCallbackEvent = await completionSource.Awaitable;
		switch (dataLoadedCallbackEvent.m_resultCode)
		{
		case ResultCode.Success:
			m_saveProfiles = dataLoadedCallbackEvent.m_loadedObject;
			break;
		case ResultCode.FileVersionTooHigh:
			if (!(await Services.Get<Team17DialogService>().PromptUserLoadErrorDeleteOrCancelAsync(Services.Get<Team17DialogLocalizationService>().LoadSaveFileTooNewError)))
			{
				return false;
			}
			await DeleteAllProfilesAsync();
			break;
		default:
			if (!(await Services.Get<Team17DialogService>().PromptUserLoadErrorDeleteOrCancelAsync()))
			{
				return false;
			}
			await DeleteAllProfilesAsync();
			break;
		}
		m_hasSave = HasSave();
		return true;
	}

	private async Awaitable DeleteAllProfilesAsync()
	{
		for (int i = 0; i < GameUtils.Constants.s_numProfiles; i++)
		{
			bool reloadProfilesOnComplete = i == GameUtils.Constants.s_numProfiles - 1;
			await DeleteProfileAsync(i, reloadProfilesOnComplete);
		}
		m_saveProfiles = CreateDefaultSaveProfiles();
	}

	private SaveProfiles CreateDefaultSaveProfiles()
	{
		SaveProfiles saveProfiles = new SaveProfiles();
		for (int i = 0; i < GameUtils.Constants.s_numProfiles; i++)
		{
			saveProfiles.m_profileMetadata[i] = new SaveProfileMetadata();
		}
		return saveProfiles;
	}

	public void LoadSaveFromStartup()
	{
		m_hasSave = HasSave();
		if (Startup.DoneRegularStartup)
		{
			if (m_hasSave)
			{
				LoadMostRecentSaveData(reloadState: false);
			}
			else
			{
				NewGame();
			}
		}
	}

	public void InvalidSaveDataStartup()
	{
		m_hasSave = false;
		NewGame();
	}

	private bool HasSave()
	{
		return m_saveProfiles.m_profileMetadata[SharedData.SaveProfileIndex].HasAnySaves();
	}

	private bool HasSaveInSlot(int index)
	{
		return m_saveProfiles.m_profileMetadata[SharedData.SaveProfileIndex].HasSaveInSlot(index);
	}

	public void ReloadState(bool isQuickLoad)
	{
		LevelTransitionEventData value = new LevelTransitionEventData
		{
			m_levelMetadata = GlobalReferences.Instance.DataStore.Data.ActiveLevelMetadata,
			m_targetSceneName = GlobalReferences.Instance.DataStore.Data.ActiveScene,
			m_targetSceneAssetReference = ((GlobalReferences.Instance.DataStore.Data.ActiveLevelMetadata != null) ? GlobalReferences.Instance.DataStore.Data.ActiveLevelMetadata.TargetSceneAssetReference : null),
			m_type = LevelTransitionEventType.LoadGame,
			m_playerSpawnPosition = GlobalReferences.Instance.DataStore.Data.PlayerPosition,
			m_playerSetup = ((!isQuickLoad) ? LevelTransitionPlayerSetup.AttachToPhone : LevelTransitionPlayerSetup.Normal)
		};
		GlobalReferences.Instance.EventChannels.LevelTransition.LevelTransition.Raise(value);
	}

	public void Update()
	{
		if (GlobalReferences.Instance.GameState.IsInState(GameState.GameStateFlag.GameActive) && !GlobalReferences.Instance.GameMenuState.IsInMenu(GameMenuState.GameMenu.SystemMenu))
		{
			float timeStep = Mathf.Max(Time.unscaledDeltaTime, Time.deltaTime);
			GlobalReferences.Instance.DataStore.IncreasePlayTime(timeStep);
		}
		m_platformSerializer.Update();
		if (m_ioIconSprite != null)
		{
			bool num = m_platformSerializer.IsWriting() || m_platformSerializer.HasWrittenRecently();
			Color color = m_ioIconSprite.color;
			if (num)
			{
				color.a = 1f;
			}
			else
			{
				color.a = Mathf.MoveTowards(color.a, 0f, Time.unscaledDeltaTime);
			}
			m_ioIconSprite.color = color;
		}
	}

	public void RequestMainSaveDataForProfile(int profileIndex, UnityAction<DataLoadedCallbackEvent<PersistentData>> loadedCallback)
	{
		m_platformSerializer.LoadPersistentData(profileIndex, m_saveProfiles.m_profileMetadata[profileIndex].MostRecentSaveSlotIndex, null, loadedCallback);
	}

	public void RequestSaveDataForActiveProfile(int saveSlot, Texture2D thumbnailOutput, UnityAction<DataLoadedCallbackEvent<PersistentData>> loadedCallback)
	{
		m_platformSerializer.LoadPersistentData(SharedData.SaveProfileIndex, saveSlot, thumbnailOutput, loadedCallback);
	}

	public SaveDataTempLoader GetSaveDataTempLoader()
	{
		return new SaveDataTempLoader(m_platformSerializer, SharedData.SaveProfileIndex, m_saveProfiles.m_profileMetadata[SharedData.SaveProfileIndex].MostRecentSaveSlotIndex);
	}

	private void SaveGame()
	{
		int mostRecentSaveSlotIndex = m_saveProfiles.m_profileMetadata[SharedData.SaveProfileIndex].MostRecentSaveSlotIndex;
		if (mostRecentSaveSlotIndex == -1)
		{
			mostRecentSaveSlotIndex = 0;
		}
		else
		{
			mostRecentSaveSlotIndex++;
			mostRecentSaveSlotIndex %= GameUtils.Constants.s_saveGameSlots;
		}
		SaveGameInSlot(mostRecentSaveSlotIndex);
	}

	private void SaveGameInSlot(int saveSlot)
	{
		GlobalReferences.Instance.EventChannels.SaveLoad.PrepareSave.Raise();
		PersistentData populatedPersistentData = GlobalReferences.Instance.DataStore.GetPopulatedPersistentData();
		m_tempSaveSlotIndex = saveSlot;
		if (m_thumbnailTexture == null)
		{
			m_thumbnailTexture = new Texture2D(GameUtils.Constants.s_saveGameThumbnailWidth, GameUtils.Constants.s_saveGameThumbnailHeight, TextureFormat.RGB24, mipChain: false);
		}
		ReadScreenshotForThumbnailTexture(m_thumbnailTexture);
		foreach (PhotoData photo in populatedPersistentData.UserMapData.m_photos)
		{
			photo.EnsureTextureLoadStarted();
		}
		m_platformSerializer.SavePersistentData(SharedData.SaveProfileIndex, saveSlot, populatedPersistentData, populatedPersistentData.UserMapData.m_photos, m_thumbnailTexture, OnSavedPersistentDataCallback);
		SaveSharedData();
		SetLastSaveTimeToNow();
	}

	private void OnSavedPersistentDataCallback(DataSavedCallbackEvent result)
	{
		Debug.Log("Game saved result " + result.m_resultCode);
		if (result.m_resultCode != ResultCode.Success)
		{
			Services.Get<Team17DialogService>().PromptUserFailedToSaveAsync();
			GlobalReferences.Instance.EventChannels.SaveLoad.SaveCompletedFailed.Raise();
			return;
		}
		m_hasSave = true;
		m_saveProfiles.m_profileMetadata[SharedData.SaveProfileIndex].SetMostRecentSaveSlotIndex(m_tempSaveSlotIndex);
		SaveProfileMetadata(SharedData.SaveProfileIndex);
		GlobalReferences.Instance.EventChannels.SaveLoad.SaveCompletedSuccesfully.Raise();
	}

	private void SaveProfileMetadata(int index)
	{
		m_platformSerializer.SaveProfileMetadata(index, m_saveProfiles.m_profileMetadata[SharedData.SaveProfileIndex], OnSaveProfileCallback);
	}

	private void OnSaveProfileCallback(DataSavedCallbackEvent result)
	{
		Debug.Log("Profile save result " + result.m_resultCode);
		if (result.m_resultCode != ResultCode.Success)
		{
			Services.Get<Team17DialogService>().PromptUserFailedToSaveAsync();
		}
	}

	private void LoadGameFromSlotInProfile(int profileIndex, int saveSlot, bool reloadState, bool isQuickLoad)
	{
		if (HasSaveInSlot(saveSlot))
		{
			m_isQuickLoadFlag = isQuickLoad;
			m_reloadStateFlag = reloadState;
			if (HasSaveInSlot(saveSlot))
			{
				m_lastLoadedSaveSlot = saveSlot;
				m_platformSerializer.LoadPersistentData(profileIndex, saveSlot, null, OnLoadGameCallback);
			}
		}
	}

	private void OnLoadGameCallback(DataLoadedCallbackEvent<PersistentData> result)
	{
		Debug.Log("Load game result " + result.m_resultCode);
		if (result.m_resultCode == ResultCode.Success)
		{
			GlobalReferences.Instance.DataStore.PopulateFromLoadedPersistentData(result.m_loadedObject);
			if (m_reloadStateFlag)
			{
				ReloadState(m_isQuickLoadFlag);
			}
			GlobalReferences.Instance.EventChannels.SaveLoad.OnGameReloadedEvent.Raise();
		}
	}

	private void LoadMostRecentSave()
	{
		LoadMostRecentSaveData(reloadState: true);
	}

	private void LoadMostRecentSaveData(bool reloadState)
	{
		int mostRecentSaveSlotIndex = m_saveProfiles.m_profileMetadata[SharedData.SaveProfileIndex].MostRecentSaveSlotIndex;
		LoadGameFromSlotInProfile(SharedData.SaveProfileIndex, mostRecentSaveSlotIndex, reloadState, isQuickLoad: false);
	}

	private void LoadGameFromSlot(int saveSlot)
	{
		m_lastLoadedSaveSlot = saveSlot;
		LoadGameFromSlotInProfile(SharedData.SaveProfileIndex, saveSlot, reloadState: true, isQuickLoad: false);
		m_saveProfiles.m_profileMetadata[SharedData.SaveProfileIndex].SetMostRecentSaveSlotIndex(saveSlot);
	}

	private void NewGame()
	{
		GlobalReferences.Instance.DataStore.ClearDataStore();
		GlobalReferences.Instance.DataStore.StartNewGame(m_newGameStartConfiguration);
		SetLastSaveTimeToNow();
	}

	private void ChapterSelectStartGame(NewGameStartConfiguration startConfiguration)
	{
		StartCoroutine(StartInChapterCoroutine(startConfiguration));
	}

	private IEnumerator StartInChapterCoroutine(NewGameStartConfiguration startConfiguration)
	{
		GlobalReferences.Instance.DataStore.ClearDataStore();
		GlobalReferences.Instance.DataStore.StartNewGame(startConfiguration);
		GlobalReferences.Instance.EventChannels.LevelTransition.ShowLoadingScreen.Raise(value: true);
		GlobalReferences.Instance.GameMenuState.ClearInMenu(GameMenuState.GameMenu.SystemMenu);
		LevelTransitionEventData value = new LevelTransitionEventData
		{
			m_targetSceneAssetReference = startConfiguration.StartingScene,
			m_type = LevelTransitionEventType.LoadGame,
			m_playerSetup = LevelTransitionPlayerSetup.Normal
		};
		GlobalReferences.Instance.EventChannels.LevelTransition.LevelTransition.Raise(value);
		yield return new WaitForSecondsRealtime(0.1f);
		yield return LevelManager.LoadUtilityScenesAsync();
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		GlobalReferences.Instance.EventChannels.LevelTransition.TriggerStartGameInScene.Raise();
		GlobalReferences.Instance.GameState.SetStateFlag(GameState.GameStateFlag.GameActive);
		GlobalReferences.Instance.EventChannels.LevelTransition.ShowLoadingScreen.Raise(value: false);
	}

	private void GameReloaded()
	{
		StopAllCoroutines();
	}

	private void DeleteProfile(int index)
	{
		m_platformSerializer.ClearDataForProfile(index, delegate
		{
			TryLoadProfilesMetadataAwaitable();
		});
	}

	private Awaitable DeleteProfileAsync(int index, bool reloadProfilesOnComplete = false)
	{
		AwaitableCompletionSource awaitableCompletionSource = new AwaitableCompletionSource();
		m_platformSerializer.ClearDataForProfile(index, delegate
		{
			if (reloadProfilesOnComplete)
			{
				TryLoadProfilesMetadataAwaitable();
			}
			awaitableCompletionSource.TrySetResult();
		});
		return awaitableCompletionSource.Awaitable;
	}

	private void ReadScreenshotForThumbnailTexture(Texture2D texture2D)
	{
		Camera main = Camera.main;
		int width = texture2D.width;
		int height = texture2D.height;
		RenderTexture renderTexture = new RenderTexture(width, height, 24);
		renderTexture.antiAliasing = 1;
		RenderTexture targetTexture = main.targetTexture;
		try
		{
			main.targetTexture = renderTexture;
			UniversalRenderPipelineAsset obj = QualitySettings.renderPipeline as UniversalRenderPipelineAsset;
			float renderScale = obj.renderScale;
			obj.renderScale = 1f;
			main.Render();
			obj.renderScale = renderScale;
			RenderTexture.active = renderTexture;
			texture2D.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
			texture2D.Apply();
		}
		finally
		{
			main.targetTexture = targetTexture;
			RenderTexture.active = null;
			renderTexture.Release();
		}
	}

	public void LoadUserPreferences(UserPreferences userPreferences, UnityAction<DataLoadedCallbackEvent<UserPreferences>> loadedCallback)
	{
		m_platformSerializer.LoadUserPreferences(userPreferences, loadedCallback);
	}

	public void SaveUserPreferences(UserPreferences userPreferences, UnityAction<DataSavedCallbackEvent> callback)
	{
		m_platformSerializer.SaveUserPreferences(userPreferences, callback);
	}

	public Awaitable<DataLoadedCallbackEvent<UserPreferences>> LoadUserPreferencesAwaitable(UserPreferences userPreferences)
	{
		AwaitableCompletionSource<DataLoadedCallbackEvent<UserPreferences>> completionSource = new AwaitableCompletionSource<DataLoadedCallbackEvent<UserPreferences>>();
		LoadUserPreferences(userPreferences, delegate(DataLoadedCallbackEvent<UserPreferences> result)
		{
			completionSource.TrySetResult(in result);
		});
		return completionSource.Awaitable;
	}

	public void LoadPhotoTexture(string photoFileName, PhotoData photoData, UnityAction<DataLoadedCallbackEvent<byte[]>> loadedCallback)
	{
		m_platformSerializer.LoadPhotoTexture(m_sharedSaveData.SaveProfileIndex, m_lastLoadedSaveSlot, photoFileName, photoData, loadedCallback);
	}

	public void DeletePhoto(string photoFileName)
	{
		m_platformSerializer.DeletePhoto(photoFileName);
	}
}
