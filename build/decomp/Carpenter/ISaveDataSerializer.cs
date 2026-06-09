using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

public interface ISaveDataSerializer
{
	void LoadSharedSaveData(UnityAction<DataLoadedCallbackEvent<SharedSaveData>> loadedCallback);

	void SaveSharedSaveData(SharedSaveData sharedSavedData, UnityAction<DataSavedCallbackEvent> savedCallback);

	void LoadProfilesMetadata(UnityAction<DataLoadedCallbackEvent<SaveProfiles>> loadedCallback);

	void SaveProfileMetadata(int profileIndex, SaveProfileMetadata profileMetadata, UnityAction<DataSavedCallbackEvent> savedCallback);

	void LoadPersistentData(int profileIndex, int saveSlot, [CanBeNull] Texture2D thumbnailOutput, UnityAction<DataLoadedCallbackEvent<PersistentData>> loadedCallback);

	void SavePersistentData(int profileIndex, int saveSlot, PersistentData persistentData, List<PhotoData> photos, Texture2D thumbnailTexture, UnityAction<DataSavedCallbackEvent> savedCallback);

	void ClearDataForProfile(int profileIndex, Action onComplete = null);

	void LoadUserPreferences(UserPreferences userPreferences, UnityAction<DataLoadedCallbackEvent<UserPreferences>> loadedCallback);

	void SaveUserPreferences(UserPreferences userPreferences, UnityAction<DataSavedCallbackEvent> savedCallback);

	void LoadPhotoTexture(int profileIndex, int saveSlot, string photoFileName, PhotoData photoData, UnityAction<DataLoadedCallbackEvent<byte[]>> loadedCallback);

	void DeletePhoto(string photoFileName);

	void Update();

	bool IsWriting();

	bool HasWrittenRecently();

	bool IsInitialized();

	bool IsBusy();
}
