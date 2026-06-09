using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Items/Inventory Upgrade Item Definition")]
public class InventoryUpgradeItemDefinition : ItemDefinition
{
	[SerializeField]
	private int m_addedSlots = 1;

	public int AddedSlots => m_addedSlots;
}
