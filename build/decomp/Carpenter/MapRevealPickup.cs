using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapReveal_", menuName = "Map/Map Reveal Pickup")]
public class MapRevealPickup : AddressableScriptableObject<MapRevealPickup>
{
	[SerializeField]
	private List<BaseMapAreaMetadata> m_mapAreaMetadatas;

	[SerializeField]
	private List<BaseMapAreaMetadata> m_overrideDontRevealAreas;

	[SerializeField]
	private List<LevelRegionSettings> m_levelRegions;

	public bool IsKnown(BaseMapAreaMetadata mapAreaMetadata)
	{
		if (mapAreaMetadata == null)
		{
			return false;
		}
		if (m_overrideDontRevealAreas.Contains(mapAreaMetadata))
		{
			return false;
		}
		LevelMetadata mainTrackedLevelMetadata = mapAreaMetadata.GetMainTrackedLevelMetadata();
		if (mainTrackedLevelMetadata != null)
		{
			LevelRegionSettings region = mainTrackedLevelMetadata.Region;
			if (region != null && m_levelRegions.Contains(region))
			{
				return true;
			}
		}
		if (m_mapAreaMetadatas.Contains(mapAreaMetadata))
		{
			return true;
		}
		return false;
	}
}
