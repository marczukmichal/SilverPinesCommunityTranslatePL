using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

[Serializable]
public struct Loot
{
	public LootType m_lootItemType;

	[AssetReferenceUILabelRestriction(new string[] { "Items" })]
	public AssetReference m_itemDefinition;

	[Header("Static Amount")]
	public int m_amount;

	[Header("Random Amount")]
	public bool m_generateRandomAmount;

	public Vector2Int m_randomlyGeneratedAmountRange;
}
