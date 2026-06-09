using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Settings/Game Completion Settings")]
public class GameCompletionPercentSettings : ScriptableObject
{
	[Serializable]
	public struct ProgressionVariableCompletion
	{
		public ProgressionVariable m_variable;

		public float m_weight;
	}

	[Serializable]
	public struct KeyItemsCompletion
	{
		public ItemDefinition m_item;

		public float m_weight;
	}

	[SerializeField]
	private float m_mapWeight = 100f;

	[SerializeField]
	private GameLevelMetadataInfo m_mapLevels;

	[SerializeField]
	private ProgressionVariableCompletion[] m_progressionVariables;

	[SerializeField]
	private KeyItemsCompletion[] m_keyItems;

	[SerializeField]
	private float m_artifactsWeight = 100f;

	[SerializeField]
	private ArtifactsGameSettings m_artifactGameSettings;

	public float CalculatePercentage(bool logResults)
	{
		if (logResults)
		{
			Debug.Log("CalculatePercentage for GameCompletionPercentSettings");
		}
		PlayerMainInventory mainInventory = GlobalReferences.Instance.MainInventory;
		bool flag = true;
		float num = 0f;
		float num2 = m_mapWeight + m_artifactsWeight;
		int num3 = 0;
		foreach (GameLevelMetadataInfo.RegionInfo region in m_mapLevels.m_regions)
		{
			foreach (LevelMetadata level in region.m_levels)
			{
				foreach (BaseMapAreaMetadata mapAreaMetadata in level.MapAreaMetadatas)
				{
					if (!(mapAreaMetadata == null))
					{
						num3++;
					}
				}
			}
		}
		if (logResults)
		{
			Debug.Log("Map Area Count: " + num3);
		}
		ProgressionVariableCompletion[] progressionVariables = m_progressionVariables;
		for (int i = 0; i < progressionVariables.Length; i++)
		{
			ProgressionVariableCompletion progressionVariableCompletion = progressionVariables[i];
			if (!(progressionVariableCompletion.m_variable == null))
			{
				num2 += progressionVariableCompletion.m_weight;
			}
		}
		KeyItemsCompletion[] keyItems = m_keyItems;
		for (int i = 0; i < keyItems.Length; i++)
		{
			KeyItemsCompletion keyItemsCompletion = keyItems[i];
			if (!(keyItemsCompletion.m_item == null))
			{
				num2 += keyItemsCompletion.m_weight;
			}
		}
		if (logResults)
		{
			Debug.Log("Total Weight: " + num2);
		}
		float num4 = m_mapWeight / (float)num3 / num2;
		foreach (GameLevelMetadataInfo.RegionInfo region2 in m_mapLevels.m_regions)
		{
			foreach (LevelMetadata level2 in region2.m_levels)
			{
				foreach (BaseMapAreaMetadata mapAreaMetadata2 in level2.MapAreaMetadatas)
				{
					if (!(mapAreaMetadata2 == null))
					{
						if (mapAreaMetadata2.HasPlayerVisitedArea())
						{
							num += num4;
						}
						else
						{
							flag = false;
						}
					}
				}
			}
		}
		progressionVariables = m_progressionVariables;
		for (int i = 0; i < progressionVariables.Length; i++)
		{
			ProgressionVariableCompletion progressionVariableCompletion2 = progressionVariables[i];
			if (!(progressionVariableCompletion2.m_variable == null))
			{
				if (progressionVariableCompletion2.m_variable.Value)
				{
					num += progressionVariableCompletion2.m_weight / num2;
				}
				else
				{
					flag = false;
				}
			}
		}
		keyItems = m_keyItems;
		for (int i = 0; i < keyItems.Length; i++)
		{
			KeyItemsCompletion keyItemsCompletion2 = keyItems[i];
			if (!(keyItemsCompletion2.m_item == null))
			{
				if (mainInventory.HasSeenItemType(keyItemsCompletion2.m_item))
				{
					num += keyItemsCompletion2.m_weight / num2;
				}
				else
				{
					flag = false;
				}
			}
		}
		float num5 = m_artifactsWeight / (float)m_artifactGameSettings.Artifacts.Length / num2;
		ArtifactItemDefinition[] artifacts = m_artifactGameSettings.Artifacts;
		foreach (ArtifactItemDefinition definition in artifacts)
		{
			if (mainInventory.GetArtifactOfType(definition) != null)
			{
				num += num5;
			}
			else
			{
				flag = false;
			}
		}
		if (flag)
		{
			if (logResults)
			{
				Debug.Log("Done everything, set completion amount to 100");
			}
			num = 1f;
		}
		if (logResults)
		{
			Debug.Log("Done Everything: " + flag + " - Calculated Result: " + num + " | " + GetCompletionPercentString(num));
		}
		return num;
	}

	public static string GetCompletionPercentString(float percent)
	{
		if (percent >= 1f)
		{
			return "100%";
		}
		return Mathf.FloorToInt(percent * 100f) + "%";
	}
}
