using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory(ActionCategory.Movement)]
public class LadderExit : BaseCharacterMovementAction
{
	public enum LadderExitPoint
	{
		Top,
		Bottom
	}

	public enum Mode
	{
		Ladder,
		Rope
	}

	protected Ladder m_ladder;

	protected ClimbableRope m_rope;

	public LadderExitPoint m_exitPoint;

	public Mode m_mode;

	public float m_startHeightOffset;

	private CharacterTraversalUtils m_traversal;

	public override void Awake()
	{
		base.Awake();
		if (!(base.Owner == null))
		{
			m_traversal = base.Owner.GetComponent<CharacterTraversalUtils>();
		}
	}

	public override void OnEnter()
	{
		Vector3 position;
		if (m_mode == Mode.Ladder)
		{
			m_ladder = m_traversal.GetLadder();
			position = ((m_exitPoint == LadderExitPoint.Top) ? m_ladder.TopBound : m_ladder.BottomBound);
		}
		else
		{
			m_rope = m_traversal.GetRope();
			position = ((m_exitPoint == LadderExitPoint.Top) ? m_rope.TopBound : m_rope.BottomBound);
		}
		position.y += m_startHeightOffset;
		position.z -= 0.2f;
		base.Owner.transform.position = position;
		m_movement.FixedDepthMovement = true;
	}

	public override void OnExit()
	{
		Vector3 position = ((m_mode != 0) ? ((m_exitPoint == LadderExitPoint.Top) ? m_rope.TopBound : m_rope.BottomBound) : ((m_exitPoint == LadderExitPoint.Top) ? m_ladder.TopBound : m_ladder.BottomBound));
		position.z = base.Owner.transform.position.z;
		m_movement.PositionAndUndoAnimRootNode(position);
		m_movement.FixedDepthMovement = false;
	}
}
