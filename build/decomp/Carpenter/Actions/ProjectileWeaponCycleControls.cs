using System;
using HutongGames.PlayMaker;
using UnityEngine;
using UnityEngine.Events;

namespace Actions;

[ActionCategory("Combat")]
public class ProjectileWeaponCycleControls : FsmStateAction
{
	public FsmEvent m_fsmEvent;

	public FsmEvent m_idleTimeoutEvent;

	public float m_idleTimeoutTime = 0.5f;

	public bool m_allowInstantSwap;

	private CharacterInventory m_inventory;

	private float m_timer;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_inventory = base.Owner.GetComponent<CharacterInventory>();
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
		if (m_inventory.HasQueuedWeaponSwap())
		{
			base.Fsm.Event(m_fsmEvent);
		}
		m_inventory.InstantQuickWeaponSwitch = m_allowInstantSwap;
		CharacterInventory inventory = m_inventory;
		inventory.OnPerformedQuickWeaponSwitch = (UnityAction)Delegate.Combine(inventory.OnPerformedQuickWeaponSwitch, new UnityAction(OnWeaponChanged));
		m_timer = 0f;
		if (m_idleTimeoutEvent == null)
		{
			Finish();
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		m_inventory.InstantQuickWeaponSwitch = true;
		CharacterInventory inventory = m_inventory;
		inventory.OnPerformedQuickWeaponSwitch = (UnityAction)Delegate.Remove(inventory.OnPerformedQuickWeaponSwitch, new UnityAction(OnWeaponChanged));
	}

	private void OnWeaponChanged()
	{
		m_timer = 0f;
		base.Fsm.Event(m_fsmEvent);
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (m_idleTimeoutEvent != null)
		{
			m_timer += Time.deltaTime;
			if (m_timer >= m_idleTimeoutTime)
			{
				m_inventory.PerformWeaponSwap();
				base.Fsm.Event(m_idleTimeoutEvent);
				Finish();
			}
		}
	}
}
