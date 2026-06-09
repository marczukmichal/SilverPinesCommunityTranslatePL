using System;
using HutongGames.PlayMaker;
using UnityEngine.Events;

namespace Actions;

[ActionCategory("Combat")]
public class OverrideHitReactEvent : FsmStateAction
{
	public FsmEvent m_hitReactEvent;

	public bool m_clearHitReactFlag;

	public bool m_ignoreKnockdown;

	public bool m_useStatelessHitReact;

	private CharacterFSMUtilities m_fsmUtilities;

	private CharacterHitReact m_hitReact;

	public override void Awake()
	{
		base.Awake();
		if (base.Owner != null)
		{
			m_fsmUtilities = base.Owner.GetComponent<CharacterFSMUtilities>();
			m_hitReact = base.Owner.GetComponent<CharacterHitReact>();
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
		if (m_ignoreKnockdown)
		{
			m_fsmUtilities.AllowHitReactDuringKnockdown = true;
		}
		if (m_useStatelessHitReact)
		{
			m_hitReact.StatelessHitReactEnabled = true;
		}
		CharacterFSMUtilities fsmUtilities = m_fsmUtilities;
		fsmUtilities.OnHitOverride = (UnityAction)Delegate.Combine(fsmUtilities.OnHitOverride, new UnityAction(OnHitEvent));
		Finish();
	}

	public override void OnExit()
	{
		base.OnExit();
		CharacterFSMUtilities fsmUtilities = m_fsmUtilities;
		fsmUtilities.OnHitOverride = (UnityAction)Delegate.Remove(fsmUtilities.OnHitOverride, new UnityAction(OnHitEvent));
		if (m_ignoreKnockdown)
		{
			m_fsmUtilities.AllowHitReactDuringKnockdown = false;
		}
		if (m_useStatelessHitReact)
		{
			m_hitReact.StatelessHitReactEnabled = false;
		}
	}

	private void OnHitEvent()
	{
		base.Fsm.Event(m_hitReactEvent);
		if (m_clearHitReactFlag)
		{
			m_hitReact.ClearHitFlag();
		}
	}
}
