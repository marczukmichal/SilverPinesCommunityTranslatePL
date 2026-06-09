using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization;
using UnityEngine.Serialization;

public class LevelMetadata : AddressableScriptableObject<LevelMetadata>
{
	public enum MetadataLevelType
	{
		Normal,
		Cutscene,
		BoatTravel
	}

	public enum LevelSpacialType
	{
		Exterior,
		Interior,
		KeepPrevious
	}

	public enum MapAreaGenerationMode
	{
		Path,
		Unmapped
	}

	[SerializeField]
	private string m_sceneName;

	[SerializeField]
	private AssetReference m_targetSceneAssetReference;

	[SerializeField]
	private LocalizedString m_displayNameReferenceString;

	[SerializeField]
	private LevelRegionSettings m_region;

	[SerializeField]
	private MusicSettings m_musicSettings;

	[Tooltip("Sets spawn setup for different level types")]
	[SerializeField]
	private MetadataLevelType m_levelType;

	[FormerlySerializedAs("m_levelAmbience")]
	[Header("Interior / Exterior Special Type")]
	[SerializeField]
	private LevelSpacialType m_levelSpacialType;

	[Header("Audio / Ambience")]
	[SerializeField]
	private LevelAmbienceSettings m_ambienceSettings;

	[SerializeField]
	private LevelReverbSettings m_reverbSettings;

	[SerializeField]
	private int m_mapFloor;

	[SerializeField]
	private MapAreaGenerationMode m_mapAreaGenerationMode;

	[SerializeField]
	private List<BaseMapAreaMetadata> m_mapAreaMetadatas;

	[SerializeField]
	private bool m_useTemporaryInventory;

	public string SceneFileName => m_sceneName;

	public AssetReference TargetSceneAssetReference => m_targetSceneAssetReference;

	public LocalizedString DisplayNameLocalizedString => m_displayNameReferenceString;

	public string DisplayName
	{
		get
		{
			if (m_displayNameReferenceString == null || m_displayNameReferenceString.IsEmpty)
			{
				return base.name;
			}
			return m_displayNameReferenceString.GetLocalizedString();
		}
	}

	public LevelRegionSettings Region => m_region;

	public MusicSettings MusicSettings => m_musicSettings;

	public MetadataLevelType LevelType => m_levelType;

	public bool IsCutscene => m_levelType == MetadataLevelType.Cutscene;

	public LevelSpacialType SpacialType => m_levelSpacialType;

	public LevelAmbienceSettings AmbienceSettings => m_ambienceSettings;

	public LevelReverbSettings ReverbSettings => m_reverbSettings;

	public int MapFloor => m_mapFloor;

	public MapAreaGenerationMode MapAreaAutoGenerationMode => m_mapAreaGenerationMode;

	public List<BaseMapAreaMetadata> MapAreaMetadatas => m_mapAreaMetadatas;

	public bool UseTemporaryInventory => m_useTemporaryInventory;

	public BaseMapAreaMetadata GetPrimaryMapAreaMetadata()
	{
		if (m_mapAreaMetadatas != null && m_mapAreaMetadatas.Count > 0)
		{
			return m_mapAreaMetadatas[0];
		}
		return null;
	}

	public BaseMapAreaMetadata GetActivePlayerMapAreaMetadata()
	{
		if (m_mapAreaMetadatas != null)
		{
			foreach (BaseMapAreaMetadata mapAreaMetadata in m_mapAreaMetadatas)
			{
				if (!(mapAreaMetadata == null) && mapAreaMetadata.IsPlayerInArea())
				{
					return mapAreaMetadata;
				}
			}
			float num = float.MaxValue;
			BaseMapAreaMetadata result = null;
			{
				foreach (BaseMapAreaMetadata mapAreaMetadata2 in m_mapAreaMetadatas)
				{
					if (!(mapAreaMetadata2 == null))
					{
						float distanceFromPlayer = mapAreaMetadata2.GetDistanceFromPlayer();
						if (distanceFromPlayer < num)
						{
							num = distanceFromPlayer;
							result = mapAreaMetadata2;
						}
					}
				}
				return result;
			}
		}
		return GetPrimaryMapAreaMetadata();
	}

	public BaseMapAreaMetadata GetMapAreaWithMatchingConnection(int connectionID)
	{
		if (m_mapAreaMetadatas == null || m_mapAreaMetadatas.Count <= 1)
		{
			return GetPrimaryMapAreaMetadata();
		}
		foreach (BaseMapAreaMetadata mapAreaMetadata in m_mapAreaMetadatas)
		{
			if (mapAreaMetadata == null)
			{
				continue;
			}
			foreach (BaseMapAreaMetadata.MapAreaConnection connection in mapAreaMetadata.Connections)
			{
				if (connection.m_connection.ConnectionID == connectionID)
				{
					return mapAreaMetadata;
				}
			}
		}
		return null;
	}
}
