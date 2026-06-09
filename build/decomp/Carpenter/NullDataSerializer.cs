using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class NullDataSerializer : ISaveDataSerializer
{
	private int m_testDelay = 100;

	private List<Task> m_readingTasks = new List<Task>();

	private List<Task> m_writingTasks = new List<Task>();

	private float m_lastWriteTime;

	private async Task LoadData<T>(UnityAction<DataLoadedCallbackEvent<T>> callback) where T : new()
	{
		await Task.Delay(m_testDelay);
		callback(new DataLoadedCallbackEvent<T>
		{
			m_resultCode = ResultCode.FileDoesNotExist
		});
	}

	private async Task LoadDataIntoObject<T>(T existingObject, UnityAction<DataLoadedCallbackEvent<T>> callback) where T : new()
	{
		await Task.Delay(m_testDelay);
		callback(new DataLoadedCallbackEvent<T>
		{
			m_resultCode = ResultCode.FileDoesNotExist
		});
	}

	private async Task SaveData(UnityAction<DataSavedCallbackEvent> callback)
	{
		await Task.Delay(m_testDelay);
		m_lastWriteTime = Time.time;
		callback(new DataSavedCallbackEvent
		{
			m_resultCode = ResultCode.Success
		});
	}

	public void LoadSharedSaveData(UnityAction<DataLoadedCallbackEvent<SharedSaveData>> loadedCallback)
	{
		Task item = LoadData(loadedCallback);
		m_readingTasks.Add(item);
	}

	public void SaveSharedSaveData(SharedSaveData sharedSavedData, UnityAction<DataSavedCallbackEvent> savedCallback)
	{
		Task item = SaveData(savedCallback);
		m_writingTasks.Add(item);
	}

	private async Task LoadProfileMetadataTask(UnityAction<DataLoadedCallbackEvent<SaveProfiles>> loadedCallback)
	{
		await Task.Delay(m_testDelay);
		SaveProfiles saveProfiles = new SaveProfiles();
		for (int i = 0; i < GameUtils.Constants.s_numProfiles; i++)
		{
			saveProfiles.m_profileMetadata[i] = new SaveProfileMetadata();
		}
		loadedCallback(new DataLoadedCallbackEvent<SaveProfiles>
		{
			m_loadedObject = saveProfiles,
			m_resultCode = ResultCode.Success
		});
	}

	public void LoadProfilesMetadata(UnityAction<DataLoadedCallbackEvent<SaveProfiles>> loadedCallback)
	{
		Task item = LoadProfileMetadataTask(loadedCallback);
		m_readingTasks.Add(item);
	}

	public void SaveProfileMetadata(int profileIndex, SaveProfileMetadata profileMetadata, UnityAction<DataSavedCallbackEvent> savedCallback)
	{
		Task item = SaveData(savedCallback);
		m_writingTasks.Add(item);
	}

	public void SavePersistentData(int profileIndex, int saveSlot, PersistentData persistentData, List<PhotoData> mPhotos, Texture2D thumbnailTexture, UnityAction<DataSavedCallbackEvent> savedCallback)
	{
		Task item = SaveData(savedCallback);
		m_writingTasks.Add(item);
	}

	public void LoadPersistentData(int profileIndex, int saveSlot, Texture2D thumbnailOutput, UnityAction<DataLoadedCallbackEvent<PersistentData>> loadedCallback)
	{
		Task item = LoadData(loadedCallback);
		m_readingTasks.Add(item);
	}

	public void ClearDataForProfile(int profileIndex, Action onComplete = null)
	{
		onComplete?.Invoke();
	}

	private void RemoveCompletedTasks(List<Task> tasks)
	{
		for (int num = tasks.Count - 1; num >= 0; num--)
		{
			Task task = tasks[num];
			if (task.IsCompleted || task.IsCompletedSuccessfully || task.IsCanceled || task.IsFaulted)
			{
				tasks.RemoveAt(num);
			}
		}
	}

	public void Update()
	{
		RemoveCompletedTasks(m_writingTasks);
		RemoveCompletedTasks(m_readingTasks);
	}

	public bool IsWriting()
	{
		return m_writingTasks.Count > 0;
	}

	public bool HasWrittenRecently()
	{
		if (m_lastWriteTime <= 0f)
		{
			return false;
		}
		return Time.time - m_lastWriteTime < 1f;
	}

	public bool IsInitialized()
	{
		return true;
	}

	public bool IsBusy()
	{
		if (m_writingTasks.Count <= 0)
		{
			return m_readingTasks.Count > 0;
		}
		return true;
	}

	public void LoadUserPreferences(UserPreferences userPreferences, UnityAction<DataLoadedCallbackEvent<UserPreferences>> loadedCallback)
	{
		Task item = LoadDataIntoObject(userPreferences, loadedCallback);
		m_readingTasks.Add(item);
	}

	public void SaveUserPreferences(UserPreferences userPreferences, UnityAction<DataSavedCallbackEvent> savedCallback)
	{
		Task item = SaveData(savedCallback);
		m_writingTasks.Add(item);
	}

	private async Task LoadPhotoTextureAsync(Texture2D texture, UnityAction<DataLoadedCallbackEvent<byte[]>> loadedCallback)
	{
		await Task.Delay(m_testDelay);
		loadedCallback(new DataLoadedCallbackEvent<byte[]>
		{
			m_resultCode = ResultCode.FileDoesNotExist
		});
	}

	public void LoadPhotoTexture(int profileIndex, int saveSlot, string photoFileName, PhotoData photoData, UnityAction<DataLoadedCallbackEvent<byte[]>> loadedCallback)
	{
		Task item = LoadPhotoTextureAsync(photoData.Texture, loadedCallback);
		m_readingTasks.Add(item);
	}

	private async Task SavePhotoTextureAsync(string photoFileName, Texture2D texture, UnityAction<DataSavedCallbackEvent> savedCallback)
	{
		await Task.Delay(m_testDelay);
		savedCallback(new DataSavedCallbackEvent
		{
			m_resultCode = ResultCode.Success
		});
	}

	public void DeletePhoto(string photoFileName)
	{
	}

	public void LoadDataThumbnail(int profileIndex, int saveSlot, Texture2D texture, UnityAction<DataLoadedCallbackEvent<byte[]>> loadedCallback)
	{
		Task item = LoadPhotoTextureAsync(texture, loadedCallback);
		m_readingTasks.Add(item);
	}

	public void SaveDataThumbnail(int profileIndex, int saveSlot, Texture2D texture, UnityAction<DataSavedCallbackEvent> savedCallback)
	{
		Task item = SavePhotoTextureAsync("", texture, savedCallback);
		m_writingTasks.Add(item);
	}
}
