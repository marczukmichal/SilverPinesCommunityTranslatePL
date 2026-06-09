using System;
using HutongGames.PlayMaker;
using UnityEngine.Events;

namespace Actions;

[ActionCategory(ActionCategory.Character)]
public class CheckForSecondaryWeaponFire : FsmStateAction
{
	protected BaseCharacterInput m_input;

	protected CharacterInventory m_characterInventory;

	public FsmEvent m_onThrowEvent;

	private bool m_hasAttemptedFire;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_input = base.Owner.GetCharacterInputComponent();
			m_characterInventory = base.Owner.GetComponent<CharacterInventory>();
		}
	}

	public override void OnEnter()
	{
		BaseCharacterInput input = m_input;
		input.OnSecondaryFireAction = (UnityAction<bool>)Delegate.Combine(input.OnSecondaryFireAction, new UnityAction<bool>(AttemptedFire));
	}

	public override void OnExit()
	{
		BaseCharacterInput input = m_input;
		input.OnSecondaryFireAction = (UnityAction<bool>)Delegate.Remove(input.OnSecondaryFireAction, new UnityAction<bool>(AttemptedFire));
	}

	private void AttemptedFire(bool input)
	{
		m_hasAttemptedFire = input;
	}

	private bool CanUseSecondary()
	{
		if (m_characterInventory.Inventory.EquippedSecondaryItem != null)
		{
			return true;
		}
		return false;
	}

	public override void OnUpdate()
	{
		if (m_hasAttemptedFire)
		{
			m_hasAttemptedFire = false;
			if (CanUseSecondary() && (m_characterInventory.Inventory.EquippedSecondaryItem.ItemDefinition as SecondaryWeaponItemDefinition).WeaponMode == SecondaryWeaponItemDefinition.SecondaryWeaponMode.Throwable)
			{
				base.Fsm.Event(m_onThrowEvent);
			}
		}
	}
}
