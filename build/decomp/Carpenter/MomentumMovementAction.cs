using HutongGames.PlayMaker;
using UnityEngine;

[ActionCategory(ActionCategory.Movement)]
public class MomentumMovementAction : BaseCharacterHorizontalMovementAction
{
	public enum InputType
	{
		Forced,
		ReadInput
	}

	public float m_acceleration = 1f;

	public InputType m_inputType;

	public float m_forcedHorizontalInput;

	private float m_currentXMovement;

	public override void OnEnter()
	{
		base.OnEnter();
		m_currentXMovement = m_movement.PreviousVelocity.x;
	}

	protected override float CalculateXVelocity()
	{
		float num = 0f;
		switch (m_inputType)
		{
		case InputType.Forced:
			num = m_forcedHorizontalInput;
			break;
		case InputType.ReadInput:
			num = m_input.MovementInput.x;
			break;
		}
		if (m_direction.CurrentDirection == CharacterDirection.Facing.Left)
		{
			num *= -1f;
		}
		float target = num * GetMoveSpeed();
		m_currentXMovement = Mathf.MoveTowards(m_currentXMovement, target, Time.deltaTime * m_acceleration);
		return m_currentXMovement;
	}
}
