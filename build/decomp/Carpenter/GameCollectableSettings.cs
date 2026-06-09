using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Collectable", menuName = "Collectables/Game Collectable Settings")]
public class GameCollectableSettings : ScriptableObject
{
	[Serializable]
	public class Region
	{
		[SerializeField]
		private LevelRegionSettings m_regionSettings;

		[SerializeField]
		private CollectableDefinition[] m_collectables;

		public LevelRegionSettings RegionSettings => m_regionSettings;

		public CollectableDefinition[] Collectables => m_collectables;
	}

	[SerializeField]
	private List<Region> m_regions;

	[SerializeField]
	private List<int> m_artifactSlotUnlockAmounts;

	public List<Region> Regions => m_regions;

	public int GetNextUnlockThreshold()
	{
		int maxEquippedArtifactsCount = GlobalReferences.Instance.MainInventory.MaxEquippedArtifactsCount;
		return m_artifactSlotUnlockAmounts[maxEquippedArtifactsCount];
	}
}
