using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[Serializable]
public class PersistentData
{
	public enum PersistentDataSaveVersion
	{
		Unknown,
		InitialVersion,
		AddedDynamicHints,
		MapAreaVisitedDataRework
	}

	private static readonly PersistentDataSaveVersion s_currentVersion = PersistentDataSaveVersion.MapAreaVisitedDataRework;

	[SerializeField]
	private PersistentDataSaveVersion m_saveVersion;

	[SerializeField]
	private long m_timeStamp;

	[SerializeField]
	private float m_playtime;

	[SerializeField]
	private int m_saveCount;

	[SerializeField]
	private DifficultySettingEnum m_currentDifficulty;

	[SerializeField]
	private DifficultySettingEnum m_lowestDifficulty;

	[SerializeField]
	private List<PersistentDataObject> m_dataEntries = new List<PersistentDataObject>();

	[SerializeField]
	private List<PersistentDataDynamicallySpawnedObject> m_dynamicallySpawnedObjects = new List<PersistentDataDynamicallySpawnedObject>();

	[SerializeField]
	private List<PersistentDataFloat> m_floatVariables = new List<PersistentDataFloat>();

	[SerializeField]
	private List<PersistentDataInt> m_intVariables = new List<PersistentDataInt>();

	[SerializeField]
	private List<PersistentDataBool> m_boolVariables = new List<PersistentDataBool>();

	[SerializeField]
	private Vector3 m_playerPosition;

	[SerializeField]
	private AssetReference m_activeLevelMetadataAssetReference;

	private LevelMetadata m_activeLevelMetadata;

	[SerializeField]
	private string m_activeScene;

	[SerializeField]
	private PersistentDataMainInventory m_characterInventory;

	[SerializeField]
	private PersistentDataInventory m_stashInventory;

	[SerializeField]
	private PersistentDataLoreInventory m_loreInventory;

	[SerializeField]
	private PersistentDataCollectableInventory m_collectableInventory;

	[SerializeField]
	private PersistentDataUserMapData m_userMapData;

	[SerializeField]
	private PersistentDataDynamicHints m_dynamicHintsData;

	[SerializeField]
	private PersistentMapStateData m_mapStateData;

	[SerializeField]
	private TimeOfDay.TimePeriod m_timeOfDay;

	[SerializeField]
	private AssetReference m_activeWeatherSettingsAssetReference;

	private WeatherSettings m_activeWeatherSettings;

	[SerializeField]
	private List<string> m_knownPhoneNumbers = new List<string>();

	[SerializeField]
	private List<string> m_calledNumberIDs = new List<string>();

	[SerializeField]
	private List<string> m_previouslyDonePhoneEvents = new List<string>();

	[SerializeField]
	private string m_mostRecentPhoneEvent;

	[SerializeField]
	private List<StatusEffectInstance> m_statusEffects = new List<StatusEffectInstance>();

	[SerializeField]
	private List<EnemyMigrationManager.MigratedEnemyStatus> m_migratedEnemies;

	[SerializeField]
	private MapObjectiveState m_mapObjectiveState;

	[SerializeField]
	private List<string> m_seenComicIDs;

	[SerializeField]
	private int m_randomCodeSeed;

	[SerializeField]
	public PersistentDataStats m_stats;

	public PersistentDataSaveVersion SaveVerison => m_saveVersion;

	public bool IsCurrentVersion => m_saveVersion == s_currentVersion;

	public long TimeStamp => m_timeStamp;

	public float PlayTime => m_playtime;

	public int SaveCount
	{
		get
		{
			return m_saveCount;
		}
		set
		{
			m_saveCount = value;
		}
	}

	public DifficultySettingEnum CurrentDifficulty => m_currentDifficulty;

	public DifficultySettingEnum LowestDifficulty => m_lowestDifficulty;

	public Vector3 PlayerPosition
	{
		get
		{
			return m_playerPosition;
		}
		set
		{
			m_playerPosition = value;
		}
	}

	public LevelMetadata ActiveLevelMetadata
	{
		get
		{
			if (m_activeLevelMetadata == null)
			{
				m_activeLevelMetadata = AddressablesContentManager.Instance.GetScriptableObjectAsset(m_activeLevelMetadataAssetReference) as LevelMetadata;
			}
			return m_activeLevelMetadata;
		}
		set
		{
			m_activeLevelMetadata = value;
			m_activeLevelMetadataAssetReference = m_activeLevelMetadata.AssetReference;
		}
	}

	public string ActiveScene
	{
		get
		{
			return m_activeScene;
		}
		set
		{
			m_activeScene = value;
		}
	}

	public PersistentDataMainInventory CharacterInventory
	{
		get
		{
			return m_characterInventory;
		}
		set
		{
			m_characterInventory = value;
		}
	}

	public PersistentDataInventory StashInventory
	{
		get
		{
			return m_stashInventory;
		}
		set
		{
			m_stashInventory = value;
		}
	}

	public PersistentDataLoreInventory LoreInventory
	{
		get
		{
			return m_loreInventory;
		}
		set
		{
			m_loreInventory = value;
		}
	}

	public PersistentDataCollectableInventory CollectableInventory
	{
		get
		{
			return m_collectableInventory;
		}
		set
		{
			m_collectableInventory = value;
		}
	}

	public PersistentDataUserMapData UserMapData
	{
		get
		{
			return m_userMapData;
		}
		set
		{
			m_userMapData = value;
		}
	}

	public PersistentDataDynamicHints DynamicHintsData
	{
		get
		{
			return m_dynamicHintsData;
		}
		set
		{
			m_dynamicHintsData = value;
		}
	}

	public PersistentMapStateData MapStateData
	{
		get
		{
			return m_mapStateData;
		}
		set
		{
			m_mapStateData = value;
		}
	}

	public TimeOfDay.TimePeriod TimeOfDay
	{
		get
		{
			return m_timeOfDay;
		}
		set
		{
			m_timeOfDay = value;
		}
	}

	public WeatherSettings ActiveWeatherSettings
	{
		get
		{
			if (m_activeWeatherSettings == null)
			{
				m_activeWeatherSettings = AddressablesContentManager.Instance.GetScriptableObjectAsset(m_activeWeatherSettingsAssetReference) as WeatherSettings;
			}
			return m_activeWeatherSettings;
		}
		set
		{
			m_activeWeatherSettings = value;
			m_activeWeatherSettingsAssetReference = ((value != null) ? value.AssetReference : null);
		}
	}

	public List<string> KnownPhoneNumbers
	{
		get
		{
			return m_knownPhoneNumbers;
		}
		set
		{
			m_knownPhoneNumbers = value;
		}
	}

	public List<string> CalledNumberIDs
	{
		get
		{
			return m_calledNumberIDs;
		}
		set
		{
			m_calledNumberIDs = value;
		}
	}

	public List<string> PreviouslyDonePhoneEvents
	{
		get
		{
			return m_previouslyDonePhoneEvents;
		}
		set
		{
			m_previouslyDonePhoneEvents = value;
		}
	}

	public string MostRecentPhoneEvent
	{
		get
		{
			return m_mostRecentPhoneEvent;
		}
		set
		{
			m_mostRecentPhoneEvent = value;
		}
	}

	public List<StatusEffectInstance> StatusEffects
	{
		get
		{
			return m_statusEffects;
		}
		set
		{
			m_statusEffects = value;
		}
	}

	public List<EnemyMigrationManager.MigratedEnemyStatus> MigratedEnemies
	{
		get
		{
			return m_migratedEnemies;
		}
		set
		{
			m_migratedEnemies = value;
		}
	}

	public MapObjectiveState ObjectiveState
	{
		get
		{
			return m_mapObjectiveState;
		}
		set
		{
			m_mapObjectiveState = value;
		}
	}

	public List<string> SeenComicIDs
	{
		get
		{
			return m_seenComicIDs;
		}
		set
		{
			m_seenComicIDs = value;
		}
	}

	public int RandomCodeSeed
	{
		get
		{
			return m_randomCodeSeed;
		}
		set
		{
			m_randomCodeSeed = value;
		}
	}

	public void IncreasePlayTime(float timePassed)
	{
		m_playtime += timePassed;
	}

	public void SetTimeStampToNow()
	{
		m_timeStamp = DateTime.UtcNow.Ticks;
	}

	public void IncrementSaveCount()
	{
		m_saveCount++;
	}

	public void SetStartingDifficulty(DifficultySettingEnum difficulty)
	{
		m_currentDifficulty = (m_lowestDifficulty = difficulty);
	}

	public void ChangeDifficulty(DifficultySettingEnum newDifficulty)
	{
		if (m_currentDifficulty != newDifficulty)
		{
			m_currentDifficulty = newDifficulty;
			if (newDifficulty == DifficultySettingEnum.Story && m_lowestDifficulty != DifficultySettingEnum.Story)
			{
				m_lowestDifficulty = newDifficulty;
			}
		}
	}

	public PersistentDataObject GetDataEntry(string guid)
	{
		for (int i = 0; i < m_dataEntries.Count; i++)
		{
			if (m_dataEntries[i].GUID.Equals(guid))
			{
				return m_dataEntries[i];
			}
		}
		PersistentDataObject persistentDataObject = new PersistentDataObject(guid);
		m_dataEntries.Add(persistentDataObject);
		return persistentDataObject;
	}

	public PersistentDataFloat GetFloatVariableEntry(string guid)
	{
		for (int i = 0; i < m_floatVariables.Count; i++)
		{
			if (m_floatVariables[i].GUID.Equals(guid))
			{
				return m_floatVariables[i];
			}
		}
		PersistentDataFloat persistentDataFloat = new PersistentDataFloat(guid);
		m_floatVariables.Add(persistentDataFloat);
		return persistentDataFloat;
	}

	public PersistentDataInt GetIntVariableEntry(string guid)
	{
		for (int i = 0; i < m_intVariables.Count; i++)
		{
			if (m_intVariables[i].GUID.Equals(guid))
			{
				return m_intVariables[i];
			}
		}
		PersistentDataInt persistentDataInt = new PersistentDataInt(guid);
		m_intVariables.Add(persistentDataInt);
		return persistentDataInt;
	}

	public PersistentDataBool GetBoolVariableEntry(string guid)
	{
		for (int i = 0; i < m_boolVariables.Count; i++)
		{
			if (m_boolVariables[i].GUID.Equals(guid))
			{
				return m_boolVariables[i];
			}
		}
		PersistentDataBool persistentDataBool = new PersistentDataBool(guid);
		m_boolVariables.Add(persistentDataBool);
		return persistentDataBool;
	}

	public void AddDynamicallySpawnedObject(PersistentDataDynamicallySpawnedObject dynamicallySpawnedObject)
	{
		m_dynamicallySpawnedObjects.Add(dynamicallySpawnedObject);
	}

	public void RemoveDynamicallySpawnedObject(PersistentDataDynamicallySpawnedObject dynamicallySpawnedObject)
	{
		m_dynamicallySpawnedObjects.Remove(dynamicallySpawnedObject);
	}

	public List<PersistentDataDynamicallySpawnedObject> GetDynamicallySpawnedObjects(LevelMetadata level)
	{
		List<PersistentDataDynamicallySpawnedObject> list = new List<PersistentDataDynamicallySpawnedObject>();
		if (level != null)
		{
			foreach (PersistentDataDynamicallySpawnedObject dynamicallySpawnedObject in m_dynamicallySpawnedObjects)
			{
				if (dynamicallySpawnedObject.MatchesLevel(level))
				{
					list.Add(dynamicallySpawnedObject);
				}
			}
			return list;
		}
		return list;
	}

	public void Clear()
	{
		m_dataEntries.Clear();
		m_dynamicallySpawnedObjects.Clear();
		m_floatVariables.Clear();
		m_intVariables.Clear();
		m_boolVariables.Clear();
		m_mostRecentPhoneEvent = null;
		m_knownPhoneNumbers.Clear();
		m_previouslyDonePhoneEvents.Clear();
		m_migratedEnemies.Clear();
		m_seenComicIDs.Clear();
		m_playerPosition = Vector3.zero;
		m_activeScene = "";
		m_activeLevelMetadata = null;
		m_saveCount = 0;
		m_playtime = 0f;
		m_randomCodeSeed = UnityEngine.Random.Range(0, 999999);
		m_stats.Clear();
	}

	public void UpdateSaveVersion()
	{
		m_saveVersion = s_currentVersion;
	}

	public void PatchData()
	{
		if (m_saveVersion == PersistentDataSaveVersion.Unknown)
		{
			m_saveVersion = PersistentDataSaveVersion.InitialVersion;
			Debug.Log("Test patch!");
		}
	}
}
