using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UserMapData", menuName = "Misc/User Map Data")]
public class UserMapData : ScriptableObject
{
	[SerializeField]
	private List<PhotoData> m_photos;

	[Header("Event Channels")]
	[SerializeField]
	private RenderTextureGameEventChannel m_onPhotoTakenEvent;

	[SerializeField]
	private List<MapUserMarkerData> m_markers;

	[SerializeField]
	private TakingPhotoStateData m_photoStateData;

	public List<PhotoData> Photos => m_photos;

	public IReadOnlyCollection<MapUserMarkerData> Markers => m_markers.AsReadOnly();

	public int GetMarkerCount()
	{
		return m_markers.Count;
	}

	public int GetAvailableMarkerCount()
	{
		return GameUtils.Constants.s_maxMarkers - GetMarkerCount();
	}

	private void Awake()
	{
		m_photos = new List<PhotoData>();
		m_markers = new List<MapUserMarkerData>();
	}

	private void OnEnable()
	{
		m_onPhotoTakenEvent.Register(OnPhotoTaken);
	}

	private void OnDisable()
	{
		m_onPhotoTakenEvent.Unregister(OnPhotoTaken);
	}

	private void OnPhotoTaken(RenderTexture photoTexture)
	{
		PhotoData item = PhotoData.TakeNewPhoto(photoTexture, m_photoStateData);
		m_photos.Add(item);
	}

	public void Clear()
	{
		m_photos.Clear();
		m_markers.Clear();
	}

	private void OnDestroy()
	{
		foreach (PhotoData photo in m_photos)
		{
			photo.UnloadTexture();
		}
	}

	public MapUserMarkerData AddNewUserMarker(Vector3 worldPosition, int mapID, int floorID)
	{
		MapUserMarkerData mapUserMarkerData = new MapUserMarkerData(worldPosition, mapID, floorID);
		m_markers.Add(mapUserMarkerData);
		return mapUserMarkerData;
	}

	public void DeleteUserMarker(MapUserMarkerData marker)
	{
		m_markers.Remove(marker);
	}

	public void DeletePhoto(PhotoData photo)
	{
		photo.UnloadTexture();
		photo.DeleteFileOnDisk();
		m_photos.Remove(photo);
	}

	public int GetPhotoCount()
	{
		return m_photos.Count;
	}

	public int GetAvailablePhotoCount()
	{
		return GameUtils.Constants.s_maxPhotos - GetPhotoCount();
	}

	public int GetMapViewMetadataPhotoCount(MapViewMetadata mapViewMetadata)
	{
		int num = 0;
		foreach (PhotoData photo in m_photos)
		{
			if (photo.MapAreaMetadata == null)
			{
				Debug.LogWarning("Photo is missing a map area metadata reference! - Ignoring for GetMapViewMetadataPhotoCount");
			}
			else if (mapViewMetadata.EncompassesLevel(photo.MapAreaMetadata.GetMainTrackedLevelMetadata()))
			{
				num++;
			}
		}
		return num;
	}

	public PersistentDataUserMapData GetPersistentData()
	{
		return new PersistentDataUserMapData
		{
			m_photos = new List<PhotoData>(m_photos),
			m_markers = new List<MapUserMarkerData>(m_markers)
		};
	}

	public void ReadFromPersistentData(PersistentDataUserMapData data)
	{
		m_photos = new List<PhotoData>(data.m_photos);
		m_markers = new List<MapUserMarkerData>(data.m_markers);
	}
}
