using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("Inventory")]
public class CheckInteractedItemType : FsmStateAction
{
	public ItemPickupAnchor m_pickupItemInteractableAnchor;

	public FsmEvent m_itemPickupEvent;

	public FsmEvent m_meleeWeaponPickupEvent;

	public FsmEvent m_noneEvent;

	public override void OnEnter()
	{
		Check();
		Finish();
	}

	private void Check()
	{
		if (m_pickupItemInteractableAnchor.Item == null)
		{
			base.Fsm.Event(m_noneEvent);
		}
		else if (m_pickupItemInteractableAnchor.Item != null)
		{
			if (m_pickupItemInteractableAnchor.Item.ItemDefinition is MeleeWeaponItemDefinition)
			{
				base.Fsm.Event(m_meleeWeaponPickupEvent);
			}
			else
			{
				base.Fsm.Event(m_itemPickupEvent);
			}
		}
	}
}
