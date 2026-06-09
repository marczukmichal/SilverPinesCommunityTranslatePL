using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("Inventory")]
public class PickupItem : FsmStateAction
{
	private CharacterInventory m_inventory;

	private CharacterEquipment m_melee;

	public ItemPickupAnchor m_pickupItemInteractableAnchor;

	public FsmEvent m_doneEvent;

	public override void Awake()
	{
		base.Awake();
		if (!(base.Owner == null))
		{
			m_inventory = base.Owner.GetComponent<CharacterInventory>();
			m_melee = base.Owner.GetComponent<CharacterEquipment>();
		}
	}

	public override void OnEnter()
	{
		Pickup();
		base.Fsm.Event(m_doneEvent);
		Finish();
	}

	private void Pickup()
	{
		if (m_pickupItemInteractableAnchor.Item != null)
		{
			if (m_inventory.Inventory.EquippedMeleeItem != null)
			{
				(m_inventory.Inventory.EquippedMeleeItem as MeleeWeaponItemInstance).IsBroken();
			}
			m_inventory.Inventory.PickupItem(m_pickupItemInteractableAnchor.Item);
			m_pickupItemInteractableAnchor.Set(null);
		}
	}
}
