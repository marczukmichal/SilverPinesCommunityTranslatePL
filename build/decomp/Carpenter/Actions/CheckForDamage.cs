using System;
using HutongGames.PlayMaker;
using UnityEngine.Events;

namespace Actions;

[ActionCategory("Combat")]
public class CheckForDamage : FsmStateAction
{
	[Tooltip("Event to trigger when hit")]
	public FsmEvent m_event;

	[UIHint(UIHint.Variable)]
	public FsmInt m_addToVariable;

	private CharacterHealth m_health;

	private bool m_onHit;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_health = base.Owner.GetComponent<CharacterHealth>();
		}
	}

	public override void OnEnter()
	{
		m_onHit = false;
		if (m_health != null)
		{
			CharacterHealth health = m_health;
			health.OnTakenDamage = (UnityAction<int>)Delegate.Combine(health.OnTakenDamage, new UnityAction<int>(OnHit));
		}
	}

	public override void OnExit()
	{
		if (m_health != null)
		{
			CharacterHealth health = m_health;
			health.OnTakenDamage = (UnityAction<int>)Delegate.Remove(health.OnTakenDamage, new UnityAction<int>(OnHit));
		}
	}

	private void OnHit(int damage)
	{
		m_onHit = true;
		if (m_addToVariable != null)
		{
			m_addToVariable.Value += damage;
		}
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (m_onHit)
		{
			base.Fsm.Event(m_event);
			m_onHit = false;
		}
	}
}
