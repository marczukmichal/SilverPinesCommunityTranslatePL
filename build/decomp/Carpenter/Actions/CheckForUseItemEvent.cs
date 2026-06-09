using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("Inventory")]
public class CheckForUseItemEvent : FsmStateAction
{
	public enum ItemUseMode
	{
		All,
		SecondaryOnly,
		DropOnly
	}

	private CharacterInventory m_inventory;

	private CharacterAiming m_aiming;

	public ItemUseMode m_mode;

	public bool m_useEvent;

	public FsmEvent m_onItemQueuedEvent;

	public bool m_saveAimState;

	public override void Awake()
	{
		base.Awake();
		if (base.Owner != null)
		{
			m_inventory = base.Owner.GetComponent<CharacterInventory>();
			m_aiming = base.Owner.GetComponent<CharacterAiming>();
		}
	}

	public override void OnEnter()
	{
		Check();
		switch (m_mode)
		{
		case ItemUseMode.All:
			m_inventory.ComplexInventoryActionsAllowed = CharacterInventory.ComplexInventoryActionsMode.All;
			break;
		case ItemUseMode.SecondaryOnly:
			m_inventory.ComplexInventoryActionsAllowed = CharacterInventory.ComplexInventoryActionsMode.SecondaryOnly;
			break;
		}
	}

	public override void OnUpdate()
	{
		Check();
	}

	public override void OnExit()
	{
		m_inventory.ComplexInventoryActionsAllowed = CharacterInventory.ComplexInventoryActionsMode.None;
	}

	private void Check()
	{
		if (m_inventory.QueuedUseItemInstance != null && m_mode != ItemUseMode.DropOnly)
		{
			if (m_inventory.QueuedUseItemInstance is SecondaryWeaponItemInstance usedSecondaryItem)
			{
				if (m_aiming != null)
				{
					m_aiming.UseAimForSecondary = m_saveAimState;
				}
				m_inventory.UsedSecondaryItem = usedSecondaryItem;
				base.Fsm.Event(m_inventory.QueuedUseItemInstance.ItemDefinition.UseFSMEvent);
				Finish();
			}
			else if (m_mode != ItemUseMode.SecondaryOnly)
			{
				if (m_useEvent)
				{
					base.Fsm.Event(m_onItemQueuedEvent);
				}
				else
				{
					base.Fsm.Event("UseItem/Start");
					base.Fsm.Event(m_inventory.QueuedUseItemInstance.ItemDefinition.UseFSMEvent);
				}
				Finish();
			}
		}
		if (m_inventory.QueuedDropItemInstance != null && m_mode != ItemUseMode.SecondaryOnly)
		{
			base.Fsm.Event("UseItem/Drop");
			Finish();
		}
	}
}
