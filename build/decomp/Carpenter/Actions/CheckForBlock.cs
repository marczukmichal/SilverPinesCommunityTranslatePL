using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.Character)]
public class CheckForBlock : FsmStateAction
{
	protected BaseCharacterInput m_input;

	public bool m_fireBlockEvent;

	public FsmEvent m_notBlockEvent;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_input = base.Owner.GetCharacterInputComponent();
		}
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		OnBlock(m_input.IsSecondaryFiring);
	}

	private void OnBlock(bool isBlocking)
	{
		bool flag = false;
		ItemInstance equippedMeleeItem = GlobalReferences.Instance.MainInventory.EquippedMeleeItem;
		if (isBlocking && equippedMeleeItem != null && equippedMeleeItem is MeleeWeaponItemInstance)
		{
			MeleeWeaponItemDefinition meleeWeaponItemDefinition = equippedMeleeItem.ItemDefinition as MeleeWeaponItemDefinition;
			if (meleeWeaponItemDefinition != null && !string.IsNullOrEmpty(meleeWeaponItemDefinition.MoveSet.FSMEventOnBlock))
			{
				if (m_fireBlockEvent)
				{
					base.Fsm.Event(meleeWeaponItemDefinition.MoveSet.FSMEventOnBlock);
				}
				flag = true;
			}
		}
		if (!flag)
		{
			base.Fsm.Event(m_notBlockEvent);
		}
	}
}
