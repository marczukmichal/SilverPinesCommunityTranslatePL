using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("Inventory")]
public class AddItemToInventory : FsmStateAction
{
	public Inventory m_inventory;

	public ItemDefinition m_itemType;

	public override void OnEnter()
	{
		m_inventory.AddItemToInventory(m_itemType, m_itemType.MaxStackSize, new Vector2Int(-1, -1), rotated: false, autoEquip: true, out var _);
		Finish();
	}
}
