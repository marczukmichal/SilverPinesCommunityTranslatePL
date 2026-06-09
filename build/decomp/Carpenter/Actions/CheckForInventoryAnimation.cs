using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("Inventory")]
public class CheckForInventoryAnimation : FsmStateAction
{
	public FsmEvent m_isInInventoryEvent;

	public FsmEvent m_notInInventoryEvent;

	public float m_minimumStateTime = 1f;

	private CharacterInventory m_inventory;

	public override void Awake()
	{
		base.Awake();
		if (base.Owner != null)
		{
			m_inventory = base.Owner.GetComponent<CharacterInventory>();
		}
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		Check();
	}

	private void Check()
	{
		bool flag = false;
		if (base.State.StateTime > m_minimumStateTime)
		{
			if (m_inventory.IsShowingItemWheel || m_inventory.IsShowingQuickItemPanel)
			{
				flag = true;
			}
			bool flag2 = GlobalReferences.Instance.GameMenuState.IsInMenu(GameMenuState.GameMenu.InGameMenu);
			if (flag2 && GlobalReferences.Instance.GameMenuState.IsInMenu(GameMenuState.GameMenu.OptionsPage))
			{
				flag2 = false;
			}
			if (flag2 || QuickItemPanel.UsedQuickItemRecently || m_inventory.UsedQuickWeaponSwitchRecently)
			{
				flag = true;
			}
		}
		if (flag)
		{
			base.Fsm.Event(m_isInInventoryEvent);
		}
		else
		{
			base.Fsm.Event(m_notInInventoryEvent);
		}
	}
}
