using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[Serializable]
public class PersistentDataMainInventory : PersistentDataInventory
{
	[SerializeField]
	public List<AssetReferenceT<ItemDefinition>> m_shortcutItemDefinitions;

	[SerializeField]
	public List<AssetReferenceT<ItemDefinition>> m_seenItemTypes;

	[SerializeReference]
	public ItemInstance m_equippedRangedItem;

	[SerializeReference]
	public ItemInstance m_equippedMeleeItem;

	[SerializeReference]
	public List<ArtifactItemInstance> m_artifactItems;

	[SerializeReference]
	public List<ItemInstance> m_equippedArtifacts;

	[SerializeField]
	public int m_maxEquippedArtifactsCount;

	[SerializeReference]
	public List<ItemInstance> m_inventoryUpgrades;

	[SerializeReference]
	public List<ItemInstance> m_specialItems;

	[SerializeField]
	public int m_money;

	[SerializeField]
	public bool m_pickedUpMoney;
}
