using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Steamworks;
using UnityEngine;
using UnityEngine.Events;

public class PCSaveDataSerializer : ISaveDataSerializer
{
	private int m_testDelay;

	private List<Task> m_readingTasks = new List<Task>();

	private List<Task> m_writingTasks = new List<Task>();

	private float m_lastWriteTime;

	private static string GetBaseSaveFilePath()
	{
		return string.Concat(Application.persistentDataPath + "/Demo", "/", SteamUser.GetSteamID().ToString());
	}

	private static string ProfileFolderLocation(int profile)
	{
		return GetBaseSaveFilePath() + "/Profile_" + profile;
	}

	private static string SaveLocation(int profile, int index)
	{
		return ProfileFolderLocation(profile) + "/save_game_" + index + ".json";
	}

	private static string SaveThumbnailLocation(int profile, int index)
	{
		return ProfileFolderLocation(profile) + "/save_game_" + index + ".png";
	}

	private static string ProfileMetadataLocation(int profile)
	{
		return ProfileFolderLocation(profile) + "/profile_metadata.json";
	}

	private static string PhotoFolderLocation(int profile)
	{
		return ProfileFolderLocation(profile) + "/Photos";
	}

	private static string GetSharedSaveDataLocation()
	{
		return GetBaseSaveFilePath() + "/shared.json";
	}

	private string PreferencesLocation()
	{
		return Application.persistentDataPath + "/user_preferences.json";
	}

	private async Task LoadData<T>(string path, UnityAction<DataLoadedCallbackEvent<T>> callback) where T : new()
	{
		await Task.Delay(m_testDelay);
		try
		{
			if (File.Exists(path))
			{
				T loadedData = new T();
				JsonUtility.FromJsonOverwrite(await File.ReadAllTextAsync(path), loadedData);
				callback(new DataLoadedCallbackEvent<T>
				{
					m_loadedObject = loadedData,
					m_resultCode = ResultCode.Success
				});
			}
			else
			{
				callback(new DataLoadedCallbackEvent<T>
				{
					m_resultCode = ResultCode.FileDoesNotExist
				});
			}
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.ToString());
			callback(new DataLoadedCallbackEvent<T>
			{
				m_resultCode = ResultCode.ExceptionThrown
			});
		}
	}

	private async Task LoadDataIntoObject<T>(string path, T existingObject, UnityAction<DataLoadedCallbackEvent<T>> callback) where T : new()
	{
		await Task.Delay(m_testDelay);
		try
		{
			if (File.Exists(path))
			{
				JsonUtility.FromJsonOverwrite(await File.ReadAllTextAsync(path), existingObject);
				callback(new DataLoadedCallbackEvent<T>
				{
					m_loadedObject = existingObject,
					m_resultCode = ResultCode.Success
				});
			}
			else
			{
				callback(new DataLoadedCallbackEvent<T>
				{
					m_resultCode = ResultCode.FileDoesNotExist
				});
			}
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.ToString());
			callback(new DataLoadedCallbackEvent<T>
			{
				m_resultCode = ResultCode.ExceptionThrown
			});
		}
	}

	private async Task SaveData<T>(string path, T saveData, UnityAction<DataSavedCallbackEvent> callback) where T : new()
	{
		await Task.Delay(m_testDelay);
		ResultCode resultCode = await SaveData(path, saveData);
		callback(new DataSavedCallbackEvent
		{
			m_resultCode = resultCode
		});
	}

	private async Task<ResultCode> SaveData<T>(string path, T saveData) where T : new()
	{
		await Task.Delay(m_testDelay);
		m_lastWriteTime = Time.unscaledTime;
		try
		{
			Directory.CreateDirectory(Path.GetDirectoryName(path));
			string contents = JsonUtility.ToJson(saveData, prettyPrint: true);
			await File.WriteAllTextAsync(path, contents);
			return ResultCode.Success;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.ToString());
			return ResultCode.ExceptionThrown;
		}
	}

	public void LoadSharedSaveData(UnityAction<DataLoadedCallbackEvent<SharedSaveData>> loadedCallback)
	{
		string sharedSaveDataLocation = GetSharedSaveDataLocation();
		Task item = LoadData(sharedSaveDataLocation, loadedCallback);
		m_readingTasks.Add(item);
	}

	public void SaveSharedSaveData(SharedSaveData sharedSavedData, UnityAction<DataSavedCallbackEvent> savedCallback)
	{
		string sharedSaveDataLocation = GetSharedSaveDataLocation();
		Task item = SaveData(sharedSaveDataLocation, sharedSavedData, savedCallback);
		m_writingTasks.Add(item);
	}

	private async Task LoadProfileMetadataTask(UnityAction<DataLoadedCallbackEvent<SaveProfiles>> loadedCallback)
	{
		await Task.Delay(m_testDelay);
		try
		{
			SaveProfiles profiles = new SaveProfiles();
			for (int i = 0; i < GameUtils.Constants.s_numProfiles; i++)
			{
				string path = ProfileMetadataLocation(i);
				profiles.m_profileMetadata[i] = new SaveProfileMetadata();
				if (!File.Exists(path))
				{
					continue;
				}
				JsonUtility.FromJsonOverwrite(await File.ReadAllTextAsync(path), profiles.m_profileMetadata[i]);
				for (int j = 0; j < GameUtils.Constants.s_saveGameSlots; j++)
				{
					if (File.Exists(SaveLocation(i, j)))
					{
						profiles.m_profileMetadata[i].SetValidSaveSlot(j);
					}
				}
				if (File.Exists(SaveLocation(i, GameUtils.Constants.s_quickSaveSlot)))
				{
					profiles.m_profileMetadata[i].SetValidSaveSlot(GameUtils.Constants.s_quickSaveSlot);
				}
			}
			loadedCallback(new DataLoadedCallbackEvent<SaveProfiles>
			{
				m_loadedObject = profiles,
				m_resultCode = ResultCode.Success
			});
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			loadedCallback(new DataLoadedCallbackEvent<SaveProfiles>
			{
				m_loadedObject = null,
				m_resultCode = ResultCode.ExceptionThrown
			});
		}
	}

	public void LoadProfilesMetadata(UnityAction<DataLoadedCallbackEvent<SaveProfiles>> loadedCallback)
	{
		Task item = LoadProfileMetadataTask(loadedCallback);
		m_readingTasks.Add(item);
	}

	public void SaveProfileMetadata(int profileIndex, SaveProfileMetadata profileMetadata, UnityAction<DataSavedCallbackEvent> savedCallback)
	{
		string path = ProfileMetadataLocation(profileIndex);
		Task item = SaveData(path, profileMetadata, savedCallback);
		m_writingTasks.Add(item);
	}

	public void SavePersistentData(int profileIndex, int saveSlot, PersistentData persistentData, List<PhotoData> photos, Texture2D thumbnailTexture, UnityAction<DataSavedCallbackEvent> savedCallback)
	{
		Task item = SavePersistantDataInternalAsync(profileIndex, saveSlot, persistentData, photos, thumbnailTexture, savedCallback);
		m_writingTasks.Add(item);
	}

	private async Task<ResultCode> SavePersistantDataInternalAsync(int profileIndex, int saveSlot, PersistentData persistentData, List<PhotoData> photos, Texture2D thumbnailTexture, UnityAction<DataSavedCallbackEvent> savedCallback)
	{
		Task task = SavePhotoTexturesAsync(photos);
		string filePath = SaveThumbnailLocation(profileIndex, saveSlot);
		Task<ResultCode> task2 = SaveThumbnailTextureAsync(filePath, thumbnailTexture);
		string path = SaveLocation(profileIndex, saveSlot);
		Task<ResultCode> persistantSaveTask = SaveData(path, persistentData);
		await Task.WhenAll(task, task2, persistantSaveTask);
		ResultCode result = persistantSaveTask.Result;
		savedCallback(new DataSavedCallbackEvent
		{
			m_resultCode = result
		});
		return result;
	}

	public void LoadPersistentData(int profileIndex, int saveSlot, Texture2D thumbnailOutput, UnityAction<DataLoadedCallbackEvent<PersistentData>> loadedCallback)
	{
		if (thumbnailOutput == null)
		{
			string path = SaveLocation(profileIndex, saveSlot);
			Task item = LoadData(path, loadedCallback);
			m_readingTasks.Add(item);
			return;
		}
		LoadDataThumbnail(profileIndex, saveSlot, thumbnailOutput, delegate
		{
			string path2 = SaveLocation(profileIndex, saveSlot);
			Task item2 = LoadData(path2, loadedCallback);
			m_readingTasks.Add(item2);
		});
	}

	public void ClearDataForProfile(int profileIndex, Action onComplete = null)
	{
		string path = ProfileFolderLocation(profileIndex);
		if (Directory.Exists(path))
		{
			Debug.Log("Deleting profile: " + profileIndex);
			Directory.Delete(path, recursive: true);
		}
		else
		{
			Debug.LogError("Tried to delete profile for " + profileIndex);
		}
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
		return Time.unscaledTime - m_lastWriteTime < 1f;
	}

	public bool IsInitialized()
	{
		return true;
	}

	public bool IsBusy()
	{
		if (m_readingTasks.Count <= 0)
		{
			return m_writingTasks.Count > 0;
		}
		return true;
	}

	public void LoadUserPreferences(UserPreferences userPreferences, UnityAction<DataLoadedCallbackEvent<UserPreferences>> loadedCallback)
	{
		string path = PreferencesLocation();
		Task item = LoadDataIntoObject(path, userPreferences, loadedCallback);
		m_readingTasks.Add(item);
	}

	public void SaveUserPreferences(UserPreferences userPreferences, UnityAction<DataSavedCallbackEvent> savedCallback)
	{
		string path = PreferencesLocation();
		Task item = SaveData(path, userPreferences, savedCallback);
		m_writingTasks.Add(item);
	}

	private string GetPhotoFilePath(string photoFileName)
	{
		return PhotoFolderLocation(SaveDataManager.Instance.SharedData.SaveProfileIndex) + "/" + photoFileName;
	}

	private async Task LoadPhotoTextureAsync(string filePath, Texture2D texture, UnityAction<DataLoadedCallbackEvent<byte[]>> loadedCallback)
	{
		try
		{
			if (File.Exists(filePath))
			{
				byte[] array = await File.ReadAllBytesAsync(filePath);
				texture.LoadImage(array);
				loadedCallback(new DataLoadedCallbackEvent<byte[]>
				{
					m_loadedObject = array,
					m_resultCode = ResultCode.Success
				});
			}
			else
			{
				loadedCallback(new DataLoadedCallbackEvent<byte[]>
				{
					m_resultCode = ResultCode.FileDoesNotExist
				});
			}
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.ToString());
			loadedCallback(new DataLoadedCallbackEvent<byte[]>
			{
				m_resultCode = ResultCode.ExceptionThrown
			});
		}
	}

	public void LoadPhotoTexture(int profileIndex, int saveSlot, string photoFileName, PhotoData photoData, UnityAction<DataLoadedCallbackEvent<byte[]>> loadedCallback)
	{
		string photoFilePath = GetPhotoFilePath(photoFileName);
		Task item = LoadPhotoTextureAsync(photoFilePath, photoData.Texture, loadedCallback);
		m_readingTasks.Add(item);
	}

	private async Task SavePhotoTextureAsync(PhotoData photoData, [CanBeNull] UnityAction<DataSavedCallbackEvent> savedCallback)
	{
		string path = PhotoFolderLocation(SaveDataManager.Instance.SharedData.SaveProfileIndex);
		string photoFilePath = GetPhotoFilePath(photoData.FileName);
		try
		{
			if (!File.Exists(photoFilePath))
			{
				Directory.CreateDirectory(path);
				Debug.Log("New photo image captured and saved to: " + photoFilePath);
				byte[] array = photoData.SerializedTextureData;
				if (array == null)
				{
					Texture2D texture = photoData.Texture;
					Texture2D texture2D = new Texture2D(texture.width, texture.height, texture.format, mipChain: false);
					Graphics.CopyTexture(texture, texture2D);
					Color[] pixels = texture2D.GetPixels();
					if (QualitySettings.activeColorSpace == ColorSpace.Linear)
					{
						Color[] array2 = new Color[pixels.Length];
						for (int i = 0; i < array2.Length; i++)
						{
							array2[i] = pixels[i].gamma;
						}
						texture2D.SetPixels(array2);
					}
					array = texture2D.EncodeToPNG();
					UnityEngine.Object.DestroyImmediate(texture2D);
				}
				await File.WriteAllBytesAsync(photoFilePath, array);
				savedCallback?.Invoke(new DataSavedCallbackEvent
				{
					m_resultCode = ResultCode.Success
				});
			}
			else
			{
				savedCallback?.Invoke(new DataSavedCallbackEvent
				{
					m_resultCode = ResultCode.FileAlreadyExists
				});
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			savedCallback?.Invoke(new DataSavedCallbackEvent
			{
				m_resultCode = ResultCode.ExceptionThrown
			});
		}
	}

	private Task SavePhotoTexturesAsync(List<PhotoData> photos)
	{
		Task[] array = new Task[photos.Count];
		for (int i = 0; i < photos.Count; i++)
		{
			PhotoData photoData = photos[i];
			Task task = (array[i] = SavePhotoTextureAsync(photoData, null));
		}
		return Task.WhenAll(array);
	}

	public void DeletePhoto(string photoFileName)
	{
		string photoFilePath = GetPhotoFilePath(photoFileName);
		if (File.Exists(photoFilePath))
		{
			File.Delete(photoFilePath);
			Debug.Log("Deleted photo from disk: " + photoFilePath);
		}
	}

	private async Task<ResultCode> SaveThumbnailTextureAsync(string filePath, Texture2D texture)
	{
		try
		{
			Directory.CreateDirectory(Path.GetDirectoryName(filePath));
			Debug.Log("Save data thumbnail to location" + filePath);
			byte[] pngData = texture.EncodeToPNG();
			await Task.Run(delegate
			{
				File.WriteAllBytes(filePath, pngData);
			});
			return ResultCode.Success;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.ToString());
			return ResultCode.ExceptionThrown;
		}
	}

	private void LoadDataThumbnail(int profileIndex, int saveSlot, Texture2D texture, UnityAction<DataLoadedCallbackEvent<byte[]>> loadedCallback)
	{
		string filePath = SaveThumbnailLocation(profileIndex, saveSlot);
		Task item = LoadPhotoTextureAsync(filePath, texture, loadedCallback);
		m_readingTasks.Add(item);
	}
}
