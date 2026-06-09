using HutongGames.PlayMaker;
using UnityEngine;

[ActionCategory(ActionCategory.Movement)]
public class MeleeComboCharacterMovementAction : BaseCharacterHorizontalMovementAction
{
	public float m_movementWindowLength;

	protected override float CalculateXVelocity()
	{
		Vector2 movementInput = m_input.MovementInput;
		if (m_direction.CurrentDirection == CharacterDirection.Facing.Left)
		{
			movementInput.x = Mathf.Clamp(movementInput.x, -1f, 0f);
			movementInput.x *= -1f;
		}
		else if (m_direction.CurrentDirection == CharacterDirection.Facing.Right)
		{
			movementInput.x = Mathf.Clamp(movementInput.x, 0f, 1f);
		}
		return (movementInput * GetMoveSpeed()).x;
	}
}
