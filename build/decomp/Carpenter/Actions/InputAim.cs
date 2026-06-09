using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.Input)]
public class InputAim : FsmStateAction
{
	[Tooltip("Event to trigger on aim start")]
	public FsmEvent m_onAimEvent;

	[Tooltip("Event to trigger on aim end")]
	public FsmEvent m_onNotAimEvent;

	public ItemDefinition m_flashlightItemDefinition;

	public bool m_checkIfStuckInAim;

	public bool m_disableThrowables;

	private BaseCharacterInput m_characterInput;

	private CharacterAiming m_characterAiming;

	private CharacterInventory m_characterInventory;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_characterInput = base.Owner.GetCharacterInputComponent();
			m_characterAiming = base.Owner.GetComponent<CharacterAiming>();
			m_characterInventory = base.Owner.GetComponent<CharacterInventory>();
		}
	}

	public override void OnEnter()
	{
		Check();
	}

	public override void OnUpdate()
	{
		Check();
	}

	private bool IsStuckInAim()
	{
		if (m_checkIfStuckInAim)
		{
			return m_characterAiming.MovementBlocked;
		}
		return false;
	}

	private void Check()
	{
		bool flag = m_characterInput.IsAiming;
		if (!flag && IsStuckInAim())
		{
			flag = true;
		}
		if (flag)
		{
			ItemInstance rangedWeaponForAim = m_characterInventory.Inventory.GetRangedWeaponForAim();
			if (rangedWeaponForAim == null)
			{
				flag = false;
			}
			else if (m_disableThrowables && rangedWeaponForAim is SecondaryWeaponItemInstance)
			{
				flag = false;
			}
		}
		if (m_onAimEvent != null && flag)
		{
			base.Fsm.Event(m_onAimEvent);
		}
		if (m_onNotAimEvent != null && !flag)
		{
			base.Fsm.Event(m_onNotAimEvent);
		}
	}
}
