using System;
using UnityEngine;

[Serializable]
public class ItemCombinationRule
{
	public enum CombineType
	{
		ProduceNewItem,
		RepairMelee,
		ChargePoweredItem,
		ChangeItemDefinition,
		RefillItemUses,
		ProduceArtifact
	}

	public enum CombineSourceResult
	{
		Consume,
		Reusable,
		ReplaceWithItem
	}

	[SerializeField]
	private CombineType m_combineType;

	[SerializeField]
	private ItemDefinition m_combineWith;

	[SerializeField]
	private ItemDefinition m_producedItem;

	[SerializeField]
	private int m_producedAmount;

	[Tooltip("If true then this item is not consumed during the combination process")]
	[SerializeField]
	private CombineSourceResult m_combineSourceresult;

	[SerializeField]
	private ItemDefinition m_replaceSourceWithItemDefinition;

	public CombineType Type => m_combineType;

	public ItemDefinition CombineWith => m_combineWith;

	public ItemDefinition ProducedItem => m_producedItem;

	public int ProducedAmount => m_producedAmount;

	public CombineSourceResult SourceResult => m_combineSourceresult;

	public ItemDefinition ReplaceSourceWithItemDefinition => m_replaceSourceWithItemDefinition;
}
