using System;
using HutongGames.PlayMaker;
using UnityEngine.Events;

namespace Actions;

[ActionCategory("Combat")]
public class OverrideDeadEvent : FsmStateAction
{
	public FsmEvent m_deadEvent;

	private CharacterFSMUtilities m_fsmUtilities;

	public override void Awake()
	{
		base.Awake();
		if (base.Owner != null)
		{
			m_fsmUtilities = base.Owner.GetComponent<CharacterFSMUtilities>();
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
		CharacterFSMUtilities fsmUtilities = m_fsmUtilities;
		fsmUtilities.OnDeadOverride = (UnityAction)Delegate.Combine(fsmUtilities.OnDeadOverride, new UnityAction(OnDeadEvent));
		Finish();
	}

	public override void OnExit()
	{
		base.OnExit();
		CharacterFSMUtilities fsmUtilities = m_fsmUtilities;
		fsmUtilities.OnDeadOverride = (UnityAction)Delegate.Remove(fsmUtilities.OnDeadOverride, new UnityAction(OnDeadEvent));
	}

	private void OnDeadEvent()
	{
		base.Fsm.Event(m_deadEvent);
	}
}
