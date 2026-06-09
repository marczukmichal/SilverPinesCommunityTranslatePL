using HutongGames.PlayMaker;
using UnityEngine;

[ActionCategory(ActionCategory.Movement)]
public class ForcedCharacterMovementAction : BaseCharacterHorizontalMovementAction
{
	public Vector2 m_forcedInput;

	protected override float CalculateXVelocity()
	{
		Vector2 forcedInput = m_forcedInput;
		if (m_direction.CurrentDirection == CharacterDirection.Facing.Left)
		{
			forcedInput.x *= -1f;
		}
		return (forcedInput * GetMoveSpeed()).x;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_movement.ForcedMovementStairsBehavior = true;
	}

	public override void OnExit()
	{
		base.OnExit();
		m_movement.ForcedMovementStairsBehavior = false;
	}
}
