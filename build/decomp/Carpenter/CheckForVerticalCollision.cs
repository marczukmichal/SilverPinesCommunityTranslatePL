using System;
using HutongGames.PlayMaker;
using UnityEngine;
using UnityEngine.Events;

[ActionCategory(ActionCategory.Movement)]
public class CheckForVerticalCollision : FsmStateAction
{
	[UIHint(UIHint.Variable)]
	public FsmFloat m_relativeVelocity;

	public FsmEvent m_onLandOnGroundEvent;

	public float m_minFallSpeedForFallEvent = 3f;

	protected CharacterMovement m_charMovement;

	[DebugCommand("fall_debug_log", "Enable logging of fall events", "fall_debug_log <true/false>", typeof(bool), false)]
	private static bool m_fall_debug_log;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_charMovement = base.Owner.GetComponent<CharacterMovement>();
		}
	}

	public override void OnEnter()
	{
		if (m_charMovement != null)
		{
			CharacterMovement charMovement = m_charMovement;
			charMovement.OnVerticalCollision = (UnityAction<float>)Delegate.Combine(charMovement.OnVerticalCollision, new UnityAction<float>(OnCollision));
		}
		Finish();
	}

	public override void OnExit()
	{
		if (m_charMovement != null)
		{
			CharacterMovement charMovement = m_charMovement;
			charMovement.OnVerticalCollision = (UnityAction<float>)Delegate.Remove(charMovement.OnVerticalCollision, new UnityAction<float>(OnCollision));
		}
	}

	private void OnCollision(float relativeVelocity)
	{
		if (m_relativeVelocity != null)
		{
			m_relativeVelocity.Value = relativeVelocity;
		}
		if (m_fall_debug_log)
		{
			Debug.Log("CheckForVerticalCollision (" + base.State.Name + ") value is:" + relativeVelocity);
		}
		if (Mathf.Abs(relativeVelocity) > m_minFallSpeedForFallEvent && !m_charMovement.IsForceFall)
		{
			base.Fsm.Event(m_onLandOnGroundEvent);
		}
	}
}
