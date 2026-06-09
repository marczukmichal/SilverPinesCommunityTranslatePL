using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class MapDynamicData : ScriptableObject
{
	[Serializable]
	public class MapAreaDynamicData
	{
		public bool m_hasVisited;

		public float m_minXNormalizePosition;

		public float m_maxXNormalizePosition;

		[NonSerialized]
		public bool m_isDirty;

		public MapAreaDynamicData()
		{
			m_minXNormalizePosition = float.MaxValue;
			m_maxXNormalizePosition = float.MinValue;
		}
	}

	public struct CompletionState
	{
		public bool m_completed;

		public float m_completionPercentage;
	}

	private HashSet<string> m_contributionsIds = new HashSet<string>();

	private Dictionary<string, MapAreaDynamicData> m_mapAreaVisitedData = new Dictionary<string, MapAreaDynamicData>();

	private Dictionary<int, MapConnectionStatus> m_connectionStates = new Dictionary<int, MapConnectionStatus>();

	private List<MapRevealPickup> m_mapRevealPickups = new List<MapRevealPickup>();

	private MapRevealPickup m_mapRevealPickupToAnimate;

	private Dictionary<string, MapIconState> m_mapIconStates = new Dictionary<string, MapIconState>();

	public MapRevealPickup MapRevealPickupToAnimate => m_mapRevealPickupToAnimate;

	public void ClearAnimateMapRevealPickup()
	{
		m_mapRevealPickupToAnimate = null;
	}

	public MapAreaDynamicData GetMapAreaDynamicData(BaseMapAreaMetadata metadata)
	{
		if (!m_mapAreaVisitedData.TryGetValue(metadata.name, out var value))
		{
			value = new MapAreaDynamicData();
			m_mapAreaVisitedData.Add(metadata.name, value);
		}
		return value;
	}

	public bool HasEnteredArea(BaseMapAreaMetadata metadata)
	{
		if (metadata == null)
		{
			return false;
		}
		if (m_mapAreaVisitedData.TryGetValue(metadata.name, out var _))
		{
			return true;
		}
		return false;
	}

	private List<MapAreaVistedDataPersistent> GetPersistentVisitedData()
	{
		List<MapAreaVistedDataPersistent> list = new List<MapAreaVistedDataPersistent>();
		foreach (KeyValuePair<string, MapAreaDynamicData> mapAreaVisitedDatum in m_mapAreaVisitedData)
		{
			list.Add(new MapAreaVistedDataPersistent
			{
				m_mapAreaID = mapAreaVisitedDatum.Key,
				m_data = mapAreaVisitedDatum.Value
			});
		}
		return list;
	}

	private void ReadVisitedDataFromPersistentData(List<MapAreaVistedDataPersistent> data)
	{
		m_mapAreaVisitedData = new Dictionary<string, MapAreaDynamicData>();
		foreach (MapAreaVistedDataPersistent datum in data)
		{
			m_mapAreaVisitedData.Add(datum.m_mapAreaID, datum.m_data);
		}
	}

	public void AddCompletedItemID(string identifier)
	{
		if (!m_contributionsIds.Contains(identifier))
		{
			m_contributionsIds.Add(identifier);
		}
	}

	public bool ContainsId(string identifier)
	{
		return m_contributionsIds.Contains(identifier);
	}

	public void Clear()
	{
		m_contributionsIds.Clear();
		m_mapIconStates.Clear();
		m_mapAreaVisitedData.Clear();
		m_connectionStates.Clear();
		m_mapRevealPickups.Clear();
	}

	public MapIconState GetMapIconStateBitMask(string guid)
	{
		if (m_mapIconStates.ContainsKey(guid))
		{
			return m_mapIconStates[guid];
		}
		return MapIconState.None;
	}

	public void SetMapIconStateBit(string guid, MapIconState bit)
	{
		if (m_mapIconStates.ContainsKey(guid))
		{
			m_mapIconStates[guid] |= bit;
		}
		else
		{
			m_mapIconStates.Add(guid, bit);
		}
	}

	public void ClearMapIconStateBit(string guid, MapIconState bit)
	{
		if (m_mapIconStates.ContainsKey(guid))
		{
			m_mapIconStates[guid] &= (MapIconState)(short)(~(int)bit);
		}
	}

	private List<MapIconStateData> GetPersistentIconStateData()
	{
		List<MapIconStateData> list = new List<MapIconStateData>();
		foreach (KeyValuePair<string, MapIconState> mapIconState in m_mapIconStates)
		{
			list.Add(new MapIconStateData
			{
				m_mapIconGUID = mapIconState.Key,
				m_mapIconState = mapIconState.Value
			});
		}
		return list;
	}

	private void ReadIconStateDataFromPersistentData(List<MapIconStateData> data)
	{
		m_mapIconStates = new Dictionary<string, MapIconState>();
		foreach (MapIconStateData datum in data)
		{
			m_mapIconStates.Add(datum.m_mapIconGUID, datum.m_mapIconState);
		}
	}

	public void SetConnectionStatus(int connectionID, MapConnectionStatus status)
	{
		if (m_connectionStates.ContainsKey(connectionID))
		{
			m_connectionStates[connectionID] = status;
		}
		else
		{
			m_connectionStates.Add(connectionID, status);
		}
	}

	public MapConnectionStatus GetConnectionStatus(int connectionID)
	{
		if (m_connectionStates.ContainsKey(connectionID))
		{
			return m_connectionStates[connectionID];
		}
		return MapConnectionStatus.Unknown;
	}

	private List<MapConnectionDataPersistent> GetPersistentConnectionData()
	{
		List<MapConnectionDataPersistent> list = new List<MapConnectionDataPersistent>();
		foreach (KeyValuePair<int, MapConnectionStatus> connectionState in m_connectionStates)
		{
			list.Add(new MapConnectionDataPersistent
			{
				m_connectionID = connectionState.Key,
				m_status = connectionState.Value
			});
		}
		return list;
	}

	private void ReadConnectionDataFromPersistentData(List<MapConnectionDataPersistent> data)
	{
		m_connectionStates = new Dictionary<int, MapConnectionStatus>();
		foreach (MapConnectionDataPersistent datum in data)
		{
			m_connectionStates.Add(datum.m_connectionID, datum.m_status);
		}
	}

	public bool IsKnown(BaseMapAreaMetadata mapAreaMetadata, bool ignoreMapRevealPickups = false)
	{
		if (HasEnteredArea(mapAreaMetadata))
		{
			return true;
		}
		if (!ignoreMapRevealPickups)
		{
			foreach (MapRevealPickup mapRevealPickup in m_mapRevealPickups)
			{
				if (mapRevealPickup.IsKnown(mapAreaMetadata))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void AddMapRevealPickup(MapRevealPickup pickup)
	{
		if (!m_mapRevealPickups.Contains(pickup))
		{
			m_mapRevealPickups.Add(pickup);
			m_mapRevealPickupToAnimate = pickup;
			GlobalReferences.Instance.Achievements.FindAllMaps.UnlockIfValueReached(m_mapRevealPickups.Count);
		}
	}

	public bool HasPickedUpMapRevealPickup(MapRevealPickup pickup)
	{
		return m_mapRevealPickups.Contains(pickup);
	}

	public PersistentMapStateData GetPersistentData()
	{
		List<AssetReference> list = new List<AssetReference>();
		foreach (MapRevealPickup mapRevealPickup in m_mapRevealPickups)
		{
			list.Add(mapRevealPickup.AssetReference);
		}
		return new PersistentMapStateData
		{
			m_mapIconStateData = GetPersistentIconStateData(),
			m_visitedData = GetPersistentVisitedData(),
			m_connectionData = GetPersistentConnectionData(),
			m_contributionIDs = new List<string>(m_contributionsIds),
			m_mapRevealPickups = list
		};
	}

	public void ReadFromPersistentData(PersistentMapStateData data)
	{
		ReadIconStateDataFromPersistentData(data.m_mapIconStateData);
		ReadVisitedDataFromPersistentData(data.m_visitedData);
		ReadConnectionDataFromPersistentData(data.m_connectionData);
		m_contributionsIds = new HashSet<string>(data.m_contributionIDs);
		m_mapRevealPickups = new List<MapRevealPickup>();
		foreach (AssetReference mapRevealPickup in data.m_mapRevealPickups)
		{
			m_mapRevealPickups.Add(AddressablesContentManager.Instance.GetScriptableObjectAsset(mapRevealPickup) as MapRevealPickup);
		}
	}
}
