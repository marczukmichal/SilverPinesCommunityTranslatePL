using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("AI")]
public class AIMoveToControlAction : BaseAIFSMControlAction
{
	[RequiredField]
	public FsmGameObject m_targetTransform;

	public FsmEvent m_targetReachedEvent;

	public bool m_shouldSprint;

	private CharacterDirection.Facing m_moveFacing;

	private Vector2 m_targetPosition;

	private bool m_reachedGoal;

	public override bool ShouldSprint => m_shouldSprint;

	public override Vector2 MoveDirection
	{
		get
		{
			if (m_reachedGoal)
			{
				return Vector2.zero;
			}
			Vector2 vector = m_aiBrain.transform.position;
			Vector2 vector2 = m_targetPosition - vector;
			if (m_moveFacing == CharacterDirection.Facing.Right && vector2.x < 0f)
			{
				m_reachedGoal = true;
				return Vector2.zero;
			}
			if (m_moveFacing == CharacterDirection.Facing.Left && vector2.x > 0f)
			{
				m_reachedGoal = true;
				return Vector2.zero;
			}
			vector2.y = 0f;
			return vector2.normalized;
		}
	}

	public override CharacterDirection.Facing FacingDirectionInput => m_moveFacing;

	public override void OnEnter()
	{
		base.OnEnter();
		Vector3 position = m_aiBrain.transform.position;
		m_reachedGoal = false;
		m_targetPosition = m_targetTransform.Value.transform.position;
		if (m_targetPosition.x < position.x)
		{
			m_moveFacing = CharacterDirection.Facing.Left;
		}
		else
		{
			m_moveFacing = CharacterDirection.Facing.Right;
		}
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (m_reachedGoal)
		{
			base.Fsm.Event(m_targetReachedEvent);
			Finish();
		}
	}

	public override void OnExit()
	{
		base.OnExit();
	}
}
