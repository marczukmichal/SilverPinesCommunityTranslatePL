using System;
using System.Collections.Generic;
using UnityEngine;

public class GameLevelMetadataInfo : ScriptableObject
{
	[Serializable]
	public class RegionInfo
	{
		public LevelRegionSettings m_region;

		public List<LevelMetadata> m_levels;
	}

	[SerializeField]
	public List<RegionInfo> m_regions = new List<RegionInfo>();

	public void Add(LevelMetadata level)
	{
		if (m_regions == null)
		{
			m_regions = new List<RegionInfo>();
		}
		RegionInfo regionInfo = null;
		foreach (RegionInfo region in m_regions)
		{
			if (region.m_region == level.Region)
			{
				regionInfo = region;
				break;
			}
		}
		if (regionInfo == null)
		{
			regionInfo = new RegionInfo
			{
				m_region = level.Region,
				m_levels = new List<LevelMetadata>()
			};
			m_regions.Add(regionInfo);
		}
		if (!regionInfo.m_levels.Contains(level))
		{
			regionInfo.m_levels.Add(level);
		}
	}
}
