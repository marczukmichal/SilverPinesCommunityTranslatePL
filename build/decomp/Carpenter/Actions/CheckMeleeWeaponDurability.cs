using System;
using HutongGames.PlayMaker;
using UnityEngine.Events;

namespace Actions;

[ActionCategory("Combat")]
public class CheckMeleeWeaponDurability : FsmStateAction
{
	[Tooltip("Event to trigger when wesapon reaches 0 durability")]
	public FsmEvent m_onZeroDurability;

	private CharacterInventory m_inventory;

	private MeleeWeaponItemInstance m_weapon;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_inventory = base.Owner.GetComponent<CharacterInventory>();
		}
	}

	public override void OnEnter()
	{
		ItemInstance equippedMeleeItem = m_inventory.Inventory.EquippedMeleeItem;
		m_weapon = equippedMeleeItem as MeleeWeaponItemInstance;
		if (m_weapon != null)
		{
			MeleeWeaponItemInstance weapon = m_weapon;
			weapon.OnWeaponBroken = (UnityAction)Delegate.Combine(weapon.OnWeaponBroken, new UnityAction(OnWeaponBroken));
		}
	}

	public override void OnExit()
	{
		if (m_weapon != null)
		{
			MeleeWeaponItemInstance weapon = m_weapon;
			weapon.OnWeaponBroken = (UnityAction)Delegate.Remove(weapon.OnWeaponBroken, new UnityAction(OnWeaponBroken));
			m_weapon = null;
		}
	}

	private void OnWeaponBroken()
	{
		base.Fsm.Event(m_onZeroDurability);
	}
}
