using System;
using HutongGames.PlayMaker;
using UnityEngine.Events;

namespace Actions;

[ActionCategory("Combat")]
public class OverrideKnockdownEvent : FsmStateAction
{
	public FsmEvent m_knockdownEvent;

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
		fsmUtilities.OnKnockedDownOverride = (UnityAction)Delegate.Combine(fsmUtilities.OnKnockedDownOverride, new UnityAction(OnKnockedDownEvent));
		Finish();
	}

	public override void OnExit()
	{
		base.OnExit();
		CharacterFSMUtilities fsmUtilities = m_fsmUtilities;
		fsmUtilities.OnKnockedDownOverride = (UnityAction)Delegate.Remove(fsmUtilities.OnKnockedDownOverride, new UnityAction(OnKnockedDownEvent));
	}

	private void OnKnockedDownEvent()
	{
		base.Fsm.Event(m_knockdownEvent);
	}
}
