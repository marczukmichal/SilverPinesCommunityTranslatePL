using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Map/Map Area List")]
public class MapGraphMapAreaList : ScriptableObject
{
	[Serializable]
	private struct RegionSettings
	{
		public LevelRegionSettings m_region;

		public bool m_include;
	}

	public enum InclusionMode
	{
		Whitelist,
		Blacklist
	}

	[SerializeField]
	private InclusionMode m_mode;

	[SerializeField]
	private BaseMapAreaMetadata m_rootNode;

	[SerializeField]
	private List<BaseMapAreaMetadata> m_includedMapAreas;

	[SerializeField]
	private List<RegionSettings> m_regions;

	public InclusionMode Mode => m_mode;

	public BaseMapAreaMetadata rootNode => m_rootNode;

	public List<BaseMapAreaMetadata> IncludedMapAreas => m_includedMapAreas;

	public bool ShouldMapAreaBeIncluded(BaseMapAreaMetadata mapAreaMetadata)
	{
		if (mapAreaMetadata == null)
		{
			return false;
		}
		LevelMetadata mainTrackedLevelMetadata = mapAreaMetadata.GetMainTrackedLevelMetadata();
		if (mainTrackedLevelMetadata != null)
		{
			foreach (RegionSettings region in m_regions)
			{
				if (region.m_region == mainTrackedLevelMetadata.Region)
				{
					return region.m_include;
				}
			}
		}
		if (m_mode == InclusionMode.Whitelist)
		{
			return m_includedMapAreas.Contains(mapAreaMetadata);
		}
		return !m_includedMapAreas.Contains(mapAreaMetadata);
	}
}
