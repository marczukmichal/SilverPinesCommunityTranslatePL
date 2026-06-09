using System;
using HutongGames.PlayMaker;
using UnityEngine.Events;

namespace Actions;

[ActionCategory("Status Effects")]
public class StatusEffectAmountChanged : FsmStateAction
{
	public StatusEffectDefinition m_definition;

	public FsmEvent m_triggerEvent;

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
		StatusEffectReceiver statusEffectReceiver = m_statusEffectReceiver;
		statusEffectReceiver.m_onStatusEffectAmountChanged = (UnityAction<StatusEffectInstance, int>)Delegate.Combine(statusEffectReceiver.m_onStatusEffectAmountChanged, new UnityAction<StatusEffectInstance, int>(OnStatusEffectAmountChanged));
		Finish();
	}

	public override void OnExit()
	{
		StatusEffectReceiver statusEffectReceiver = m_statusEffectReceiver;
		statusEffectReceiver.m_onStatusEffectAmountChanged = (UnityAction<StatusEffectInstance, int>)Delegate.Remove(statusEffectReceiver.m_onStatusEffectAmountChanged, new UnityAction<StatusEffectInstance, int>(OnStatusEffectAmountChanged));
	}

	private void OnStatusEffectAmountChanged(StatusEffectInstance effectInstance, int amount)
	{
		if (effectInstance.Definition == m_definition && amount > 0)
		{
			base.Fsm.Event(m_triggerEvent);
		}
	}
}
