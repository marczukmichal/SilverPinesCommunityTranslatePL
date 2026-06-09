using System;
using HutongGames.PlayMaker;
using UnityEngine;
using UnityEngine.Events;

namespace Actions;

[ActionCategory("AI")]
public class AITargetedLeapControlAction : BaseAIFSMControlAction
{
	public Transform m_targetTransform;

	public FsmEvent m_targetReachedEvent;

	private CharacterTargetedLeap m_targetedLeap;

	private CharacterDirection.Facing m_moveFacing;

	private Vector2 m_targetPosition;

	public override Vector2 MoveDirection => Vector2.zero;

	public override bool ShouldJump => true;

	public override CharacterDirection.Facing FacingDirectionInput => m_moveFacing;

	public override void Awake()
	{
		base.Awake();
		if (base.Owner != null && m_aiBrain != null)
		{
			m_targetedLeap = m_aiBrain.GetComponent<CharacterTargetedLeap>();
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_targetedLeap.ForceLeapObject(m_targetTransform.gameObject);
		CharacterTargetedLeap targetedLeap = m_targetedLeap;
		targetedLeap.m_onLeapFinished = (UnityAction)Delegate.Combine(targetedLeap.m_onLeapFinished, new UnityAction(OnLeapDone));
		Vector3 position = m_aiBrain.transform.position;
		m_targetPosition = m_targetTransform.position;
		if (m_targetPosition.x < position.x)
		{
			m_moveFacing = CharacterDirection.Facing.Left;
		}
		else
		{
			m_moveFacing = CharacterDirection.Facing.Right;
		}
	}

	private void OnLeapDone()
	{
		base.Fsm.Event(m_targetReachedEvent);
		Finish();
	}

	public override void OnExit()
	{
		base.OnExit();
		CharacterTargetedLeap targetedLeap = m_targetedLeap;
		targetedLeap.m_onLeapFinished = (UnityAction)Delegate.Remove(targetedLeap.m_onLeapFinished, new UnityAction(OnLeapDone));
		m_targetedLeap.ForceLeapObject(null);
	}
}
