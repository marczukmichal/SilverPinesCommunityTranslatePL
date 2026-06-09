using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MapDetailGenerationSettings
{
	[Serializable]
	public class GenerateMapDetailSettings
	{
		public MapDetailType m_enumType;

		public GameObject m_prefabToSpawn;
	}

	[SerializeField]
	private List<GenerateMapDetailSettings> m_mapDetailSettings;

	public GenerateMapDetailSettings GetMapDetailSettings(MapDetailType type)
	{
		foreach (GenerateMapDetailSettings mapDetailSetting in m_mapDetailSettings)
		{
			if (mapDetailSetting.m_enumType == type)
			{
				return mapDetailSetting;
			}
		}
		return null;
	}
}
