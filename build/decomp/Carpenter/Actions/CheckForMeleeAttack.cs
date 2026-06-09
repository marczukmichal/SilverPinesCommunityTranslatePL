using System;
using HutongGames.PlayMaker;
using UnityEngine.Events;

namespace Actions;

[ActionCategory(ActionCategory.Character)]
public class CheckForMeleeAttack : FsmStateAction
{
	public enum MoveSetEvent
	{
		Normal,
		Crouched,
		Sprint
	}

	protected BaseCharacterInput m_input;

	private CharacterInventory m_inventory;

	public MoveSetEvent m_moveSetEvent;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_input = base.Owner.GetCharacterInputComponent();
			m_inventory = base.Owner.GetComponent<CharacterInventory>();
		}
	}

	public override void OnEnter()
	{
		if (m_input.IsFiring)
		{
			OnAttack(isAttack: true);
		}
		BaseCharacterInput input = m_input;
		input.OnFireAction = (UnityAction<bool>)Delegate.Combine(input.OnFireAction, new UnityAction<bool>(OnAttack));
	}

	public override void OnExit()
	{
		BaseCharacterInput input = m_input;
		input.OnFireAction = (UnityAction<bool>)Delegate.Remove(input.OnFireAction, new UnityAction<bool>(OnAttack));
	}

	protected virtual void OnAttack(bool isAttack)
	{
		if (!isAttack)
		{
			return;
		}
		ItemInstance meleeWeaponForAttack = m_inventory.Inventory.GetMeleeWeaponForAttack();
		if (meleeWeaponForAttack == null || !(meleeWeaponForAttack is MeleeWeaponItemInstance))
		{
			return;
		}
		MeleeWeaponItemDefinition meleeWeaponItemDefinition = meleeWeaponForAttack.ItemDefinition as MeleeWeaponItemDefinition;
		if (meleeWeaponItemDefinition != null)
		{
			switch (m_moveSetEvent)
			{
			case MoveSetEvent.Normal:
				base.Fsm.Event(meleeWeaponItemDefinition.MoveSet.FSMEventOnAttack);
				break;
			case MoveSetEvent.Crouched:
				base.Fsm.Event(meleeWeaponItemDefinition.MoveSet.FSMEventOnAttackCrouched);
				break;
			case MoveSetEvent.Sprint:
				base.Fsm.Event(meleeWeaponItemDefinition.MoveSet.FSMEventOnAttackSprinting);
				break;
			}
		}
	}
}
