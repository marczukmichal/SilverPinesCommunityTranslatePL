using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Settings/Loot Table Settings")]
public class LootTableSettings : ScriptableObject
{
	[Serializable]
	public struct LootTableEntry
	{
		public Loot m_loot;

		public float m_weight;

		[Header("Adaptive Difficulty Spawning")]
		public GameDifficultyResourceScoreType m_resourceScoreType;

		public GameDifficultyResourceLevel m_resourceLevelRequired;

		public bool CanInclude()
		{
			if (m_resourceScoreType != 0)
			{
				if (GameDifficultyManager.GetCurrentResourceLevel(m_resourceScoreType) == m_resourceLevelRequired)
				{
					return true;
				}
				return false;
			}
			return true;
		}
	}

	[SerializeField]
	private LootTableEntry[] m_lootTable;

	public LootTableEntry[] LootTable => m_lootTable;

	public bool GetLootFromLootTable(ref Loot loot)
	{
		float num = 0f;
		LootTableEntry[] lootTable = m_lootTable;
		for (int i = 0; i < lootTable.Length; i++)
		{
			LootTableEntry lootTableEntry = lootTable[i];
			if (lootTableEntry.CanInclude())
			{
				num += lootTableEntry.m_weight;
			}
		}
		float num2 = 0f;
		float num3 = UnityEngine.Random.Range(0f, num);
		lootTable = m_lootTable;
		for (int i = 0; i < lootTable.Length; i++)
		{
			LootTableEntry lootTableEntry2 = lootTable[i];
			if (!lootTableEntry2.CanInclude())
			{
				continue;
			}
			num2 += lootTableEntry2.m_weight;
			if (num2 > num3)
			{
				loot = lootTableEntry2.m_loot;
				if (loot.m_generateRandomAmount)
				{
					loot.m_amount = loot.m_randomlyGeneratedAmountRange.GetRandom();
				}
				return true;
			}
		}
		return false;
	}
}
