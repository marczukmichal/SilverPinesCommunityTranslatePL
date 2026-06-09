using System;
using HutongGames.PlayMaker;
using UnityEngine.Events;

namespace Actions;

[ActionCategory("Status Effects")]
public class IsStatusEffectActive : FsmStateAction
{
	public StatusEffectDefinition m_definition;

	public FsmEvent m_activeEvent;

	public FsmEvent m_inactiveEvent;

	public bool m_checkOnUpdate;

	protected StatusEffectReceiver m_statusEffectReceiver;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_statusEffectReceiver = base.Owner.GetComponent<StatusEffectReceiver>();
		}
	}

	public override void OnEnter()
	{
		if (m_checkOnUpdate)
		{
			StatusEffectReceiver statusEffectReceiver = m_statusEffectReceiver;
			statusEffectReceiver.m_onStatusEffectActiveChanged = (UnityAction<StatusEffectInstance, bool>)Delegate.Combine(statusEffectReceiver.m_onStatusEffectActiveChanged, new UnityAction<StatusEffectInstance, bool>(StatusEffectActiveChangedCallback));
		}
		if (m_statusEffectReceiver.IsStatusEffectActive(m_definition))
		{
			base.Fsm.Event(m_activeEvent);
		}
		else
		{
			base.Fsm.Event(m_inactiveEvent);
		}
		Finish();
	}

	public override void OnExit()
	{
		if (m_checkOnUpdate)
		{
			StatusEffectReceiver statusEffectReceiver = m_statusEffectReceiver;
			statusEffectReceiver.m_onStatusEffectActiveChanged = (UnityAction<StatusEffectInstance, bool>)Delegate.Remove(statusEffectReceiver.m_onStatusEffectActiveChanged, new UnityAction<StatusEffectInstance, bool>(StatusEffectActiveChangedCallback));
		}
	}

	private void StatusEffectActiveChangedCallback(StatusEffectInstance statusEffect, bool active)
	{
		if (statusEffect.Definition.Type == m_definition.Type)
		{
			if (active)
			{
				base.Fsm.Event(m_activeEvent);
			}
			else
			{
				base.Fsm.Event(m_inactiveEvent);
			}
		}
	}
}
