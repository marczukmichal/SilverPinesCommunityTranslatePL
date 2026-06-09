using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "MapView", menuName = "Settings/Map View Metadata")]
public class MapViewMetadata : ScriptableObject
{
	[SerializeField]
	private AssetReference m_mapViewScene;

	[SerializeField]
	private LocalizedString m_areaName;

	[SerializeField]
	private int m_uniqueID;

	[SerializeField]
	private LevelRegionSettings[] m_associatedRegions;

	[SerializeField]
	private LevelMetadata[] m_associatedLevels;

	public AssetReference MapViewScene => m_mapViewScene;

	public string AreaName
	{
		get
		{
			if (m_areaName.IsEmpty)
			{
				return base.name;
			}
			return m_areaName.GetLocalizedString();
		}
	}

	public int UniqueID => m_uniqueID;

	public bool EncompassesLevel(LevelMetadata level)
	{
		if (level.Region != null)
		{
			LevelRegionSettings[] associatedRegions = m_associatedRegions;
			for (int i = 0; i < associatedRegions.Length; i++)
			{
				if (associatedRegions[i] == level.Region)
				{
					return true;
				}
			}
		}
		LevelMetadata[] associatedLevels = m_associatedLevels;
		for (int i = 0; i < associatedLevels.Length; i++)
		{
			if (associatedLevels[i] == level)
			{
				return true;
			}
		}
		return false;
	}

	public bool EncompassesRegion(LevelRegionSettings targetRegion)
	{
		LevelRegionSettings[] associatedRegions = m_associatedRegions;
		for (int i = 0; i < associatedRegions.Length; i++)
		{
			if (associatedRegions[i] == targetRegion)
			{
				return true;
			}
		}
		return false;
	}
}
