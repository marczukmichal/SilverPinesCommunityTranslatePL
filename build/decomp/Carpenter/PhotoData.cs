using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;

[Serializable]
public class PhotoData
{
	[SerializeField]
	private string m_fileName;

	[SerializeField]
	private Vector2 m_worldPosition;

	[SerializeField]
	private AssetReference m_mapAreaAssetReference;

	private BaseMapAreaMetadata m_mapAreaMetadata;

	private Texture2D m_texture;

	private byte[] m_serializedTextureData;

	public UnityAction m_onTextureLoadedCallback;

	public string FileName => m_fileName;

	public Vector3 WorldPosition => m_worldPosition;

	public BaseMapAreaMetadata MapAreaMetadata
	{
		get
		{
			if (m_mapAreaMetadata == null && m_mapAreaAssetReference != null && m_mapAreaAssetReference.HasAsset())
			{
				m_mapAreaMetadata = AddressablesContentManager.Instance.GetScriptableObjectAsset(m_mapAreaAssetReference) as BaseMapAreaMetadata;
			}
			return m_mapAreaMetadata;
		}
	}

	public byte[] SerializedTextureData => m_serializedTextureData;

	public Texture2D Texture
	{
		get
		{
			EnsureTextureLoadStarted();
			return m_texture;
		}
	}

	public bool IsLoaded => m_texture;

	public PhotoData(string fileName, Vector2 worldPosition, BaseMapAreaMetadata mapAreaMetadata, Texture2D cachedTexture)
	{
		m_fileName = fileName;
		m_worldPosition = worldPosition;
		if (mapAreaMetadata != null)
		{
			m_mapAreaAssetReference = mapAreaMetadata.AssetReference;
			m_mapAreaMetadata = mapAreaMetadata;
		}
		else
		{
			m_mapAreaMetadata = null;
		}
		m_texture = cachedTexture;
	}

	public void EnsureTextureLoadStarted()
	{
		if (!IsLoaded)
		{
			m_texture = new Texture2D(2, 2);
			LoadTexture();
		}
	}

	public void LoadTexture()
	{
		SaveDataManager.Instance.LoadPhotoTexture(m_fileName, this, LoadedCallback);
	}

	private void LoadedCallback(DataLoadedCallbackEvent<byte[]> loadedEvent)
	{
		if (loadedEvent.m_resultCode == ResultCode.Success)
		{
			m_serializedTextureData = loadedEvent.m_loadedObject;
			m_onTextureLoadedCallback?.Invoke();
		}
	}

	public void UnloadTexture()
	{
		if (m_texture != null)
		{
			UnityEngine.Object.DestroyImmediate(m_texture);
			m_texture = null;
		}
	}

	public void DeleteFileOnDisk()
	{
		SaveDataManager.Instance.DeletePhoto(m_fileName);
	}

	private static Texture2D ConvertRenderTextureToTexture(RenderTexture renderTexture, int x, int y, int width, int height)
	{
		bool linear = !renderTexture.sRGB;
		Texture2D texture2D = new Texture2D(width, height, TextureFormat.RGB24, mipChain: false, linear);
		RenderTexture.active = renderTexture;
		texture2D.ReadPixels(new Rect(x, y, width, height), 0, 0);
		RenderTexture.active = null;
		texture2D.Apply();
		return texture2D;
	}

	public static PhotoData TakeNewPhoto(RenderTexture photoTexture, TakingPhotoStateData photoStateData)
	{
		string fileName = "Photo-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".png";
		int num = Mathf.RoundToInt(photoStateData.m_size * (float)Screen.height);
		int x = (int)(photoStateData.m_offset.x + (float)Screen.width * 0.5f - (float)num * 0.5f);
		int y = (int)(photoStateData.m_offset.y + (float)Screen.height * 0.5f - (float)num * 0.5f);
		Texture2D cachedTexture = ConvertRenderTextureToTexture(photoTexture, x, y, num, num);
		Vector2 worldPosition = Vector2.zero;
		LevelMetadata item = GlobalReferences.Instance.Anchors.Generic.ActiveLevelMetadata.Item;
		GameObject item2 = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item;
		if (item2 != null)
		{
			worldPosition = item2.transform.position;
		}
		PhotoData photoData = new PhotoData(fileName, worldPosition, item.GetActivePlayerMapAreaMetadata(), cachedTexture);
		GlobalReferences.Instance.EventChannels.Photos.NewPhotoAdded.Raise(photoData);
		return photoData;
	}

	private void OnSavePhotoCallback(DataSavedCallbackEvent callback)
	{
	}

	public void SetSerializedTextureData(byte[] data)
	{
		m_serializedTextureData = data;
	}
}
