using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("Inventory")]
public class PlayerHasItemInInventory : FsmStateAction
{
	public ItemDefinition m_itemDefinition;

	public FsmEvent m_hasItem;

	public FsmEvent m_doesntHaveItem;

	public override void OnEnter()
	{
		Check();
		Finish();
	}

	private void Check()
	{
		GameObject item = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item;
		if (item != null)
		{
			CharacterInventory component = item.GetComponent<CharacterInventory>();
			if (component != null && component.Inventory.GetItemOfType(m_itemDefinition) != null)
			{
				base.Fsm.Event(m_hasItem);
				return;
			}
		}
		base.Fsm.Event(m_doesntHaveItem);
	}
}
