using System;
using HutongGames.PlayMaker;
using UnityEngine;
using UnityEngine.Events;

namespace Actions;

[ActionCategory("Minigames")]
public class GetValueAdjustMinigameValues : FsmStateAction
{
	[SerializeField]
	public ValueAdjustMinigameData m_valueAdjustMinigameData;

	[SerializeField]
	public FsmEvent m_valueChangedEvent;

	[UIHint(UIHint.Variable)]
	public FsmFloat m_currentValue;

	public override void OnEnter()
	{
		if (m_valueChangedEvent != null)
		{
			ValueAdjustMinigameData valueAdjustMinigameData = m_valueAdjustMinigameData;
			valueAdjustMinigameData.OnValueChangedEvent = (UnityAction)Delegate.Combine(valueAdjustMinigameData.OnValueChangedEvent, new UnityAction(ValueChanged));
		}
		UpdateVariables();
		Finish();
	}

	public override void OnExit()
	{
		if (m_valueChangedEvent != null)
		{
			ValueAdjustMinigameData valueAdjustMinigameData = m_valueAdjustMinigameData;
			valueAdjustMinigameData.OnValueChangedEvent = (UnityAction)Delegate.Remove(valueAdjustMinigameData.OnValueChangedEvent, new UnityAction(ValueChanged));
		}
	}

	private void ValueChanged()
	{
		base.Fsm.Event(m_valueChangedEvent);
		UpdateVariables();
	}

	private void UpdateVariables()
	{
		if (m_currentValue != null)
		{
			m_currentValue.Value = m_valueAdjustMinigameData.CurrentValue;
		}
	}
}
