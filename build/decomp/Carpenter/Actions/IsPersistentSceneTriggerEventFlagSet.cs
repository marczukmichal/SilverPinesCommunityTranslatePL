using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("Utilities")]
public class IsPersistentSceneTriggerEventFlagSet : FsmStateAction
{
	[SerializeField]
	public PersistentSceneTriggerEvents m_persistentSceneTriggerEvent;

	[HutongGames.PlayMaker.Tooltip("Event to trigger if the variable is set")]
	public FsmEvent m_set;

	[HutongGames.PlayMaker.Tooltip("Event to trigger if the variable is not set")]
	public FsmEvent m_notSet;

	public bool m_everyFrame;

	public override void OnEnter()
	{
		Check();
		if (!m_everyFrame)
		{
			Finish();
		}
	}

	public override void OnUpdate()
	{
		Check();
	}

	private void Check()
	{
		if (m_persistentSceneTriggerEvent.IsSet)
		{
			base.Fsm.Event(m_set);
		}
		else
		{
			base.Fsm.Event(m_notSet);
		}
	}
}
