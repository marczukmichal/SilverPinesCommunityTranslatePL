using HutongGames.PlayMaker;
using UnityEngine;

[ActionCategory(ActionCategory.Movement)]
public class JumpCharacterMovementAction : BaseCharacterMovementAction
{
	public float jumpHeight = 4f;

	public float timeToJumpApex = 0.4f;

	private float m_calculatedJumpGravity;

	private float m_jumpVelocity;

	public Vector2 m_forcedInput;

	private bool m_shouldAddJumpVelocity;

	protected override float Gravity => m_calculatedJumpGravity;

	public override void OnEnter()
	{
		base.OnEnter();
		m_calculatedJumpGravity = (0f - 2f * jumpHeight) / Mathf.Pow(timeToJumpApex, 2f);
		m_jumpVelocity = Mathf.Abs(m_calculatedJumpGravity) * timeToJumpApex;
		m_shouldAddJumpVelocity = true;
		m_movement.StartJump();
		m_movement.ForcedMovementStairsBehavior = true;
	}

	public override void OnExit()
	{
		base.OnExit();
		m_movement.ForcedMovementStairsBehavior = false;
	}

	protected override Vector2 CalculateVelocity()
	{
		float num = m_forcedInput.x;
		if (m_direction.CurrentDirection == CharacterDirection.Facing.Left)
		{
			num *= -1f;
		}
		float x = num * GetMoveSpeed();
		Vector2 previousVelocity = m_movement.PreviousVelocity;
		previousVelocity.x = x;
		previousVelocity.y += Gravity * Time.deltaTime;
		if (m_shouldAddJumpVelocity)
		{
			previousVelocity.y = m_jumpVelocity;
			m_shouldAddJumpVelocity = false;
		}
		return previousVelocity;
	}
}
