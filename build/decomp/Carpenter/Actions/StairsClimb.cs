using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory(ActionCategory.Movement)]
public class StairsClimb : BaseCharacterMovementAction
{
	public FsmEvent m_exitEvent;

	private CharacterTraversalUtils m_traversalUtils;

	private Stairs m_attachedStairs;

	private Vector2 m_stairsMovement;

	public override void Awake()
	{
		base.Awake();
		if (!(base.Owner == null))
		{
			m_traversalUtils = base.Owner.GetComponent<CharacterTraversalUtils>();
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_movement.FixedDepthMovement = true;
	}

	public override void OnExit()
	{
		base.OnExit();
		m_movement.SnapToGround();
		m_movement.FixedDepthMovement = false;
	}

	public override void OnFixedUpdate()
	{
		base.OnFixedUpdate();
		Vector3 position = base.Owner.transform.position;
		float target = m_attachedStairs.transform.position.z + m_attachedStairs.ZDepthOffset;
		position.z = Mathf.MoveTowards(position.z, target, Time.deltaTime * 10f);
		base.Owner.transform.position = position;
	}

	protected override Vector2 CalculateVelocity()
	{
		return m_stairsMovement;
	}

	private void StairsEntryMoveTowardsPosition(Vector2 position)
	{
	}

	private void StairsMovementInput(bool goingUp)
	{
	}
}
