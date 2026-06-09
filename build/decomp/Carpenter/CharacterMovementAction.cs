using HutongGames.PlayMaker;
using UnityEngine;

[ActionCategory(ActionCategory.Movement)]
public class CharacterMovementAction : BaseCharacterMovementAction
{
	public enum CharacterInputMode
	{
		Normal,
		OnlyAllowCurrentDirection
	}

	public enum DirectionSelectionMode
	{
		DirectionFromInput,
		DirectionFromVelocity,
		MaintainDirection
	}

	public FsmEvent m_onShouldTurnEvent;

	public CharacterInputMode m_inputMode;

	public DirectionSelectionMode m_directionSelectionMode;

	protected override Vector2 CalculateVelocity()
	{
		float num = m_input.MovementInput.x;
		if (m_statusEffectReceiver != null && m_statusEffectReceiver.IsStatusEffectActive(GlobalReferences.Instance.StatusEffects.Generic.Entangled))
		{
			num = 0f;
		}
		if (m_inputMode == CharacterInputMode.OnlyAllowCurrentDirection)
		{
			num = ((m_direction.CurrentDirection != CharacterDirection.Facing.Left) ? Mathf.Max(num, 0f) : Mathf.Min(num, 0f));
		}
		num = ((Mathf.Abs(num) < GameUtils.Constants.s_inputMoveDeadzoneMinValue) ? 0f : ((!(num < 0f)) ? 1f : (-1f)));
		float x = num * GetMoveSpeed();
		Vector2 velocity = m_velocity;
		velocity.x = x;
		velocity.y += Gravity * Time.deltaTime;
		return velocity;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_direction.HasTurnAnimation = m_onShouldTurnEvent != null;
	}

	public override void OnExit()
	{
		base.OnExit();
		m_direction.HasTurnAnimation = false;
	}

	private void UpdateDirection()
	{
		if (m_directionSelectionMode == DirectionSelectionMode.DirectionFromInput)
		{
			if (m_input.MovementInput.x > GameUtils.Constants.s_inputMoveDeadzoneMinValue)
			{
				m_direction.DesiredDirection = CharacterDirection.Facing.Right;
			}
			else if (m_input.MovementInput.x < 0f - GameUtils.Constants.s_inputMoveDeadzoneMinValue)
			{
				m_direction.DesiredDirection = CharacterDirection.Facing.Left;
			}
		}
		else if (m_directionSelectionMode == DirectionSelectionMode.DirectionFromVelocity)
		{
			if (m_movement.PreviousVelocity.x > GameUtils.Constants.s_inputMoveDeadzoneMinValue)
			{
				m_direction.DesiredDirection = CharacterDirection.Facing.Right;
			}
			else if (m_movement.PreviousVelocity.x < 0f - GameUtils.Constants.s_inputMoveDeadzoneMinValue)
			{
				m_direction.DesiredDirection = CharacterDirection.Facing.Left;
			}
		}
		if (m_input.FacingDirectionInput != 0)
		{
			m_direction.DesiredDirection = m_input.FacingDirectionInput;
		}
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (m_direction.HasTurnAnimation && !m_input.TurnDisabled)
		{
			UpdateDirection();
			if (m_direction.DesiredDirection != m_direction.CurrentDirection)
			{
				base.Fsm.Event(m_onShouldTurnEvent);
			}
		}
	}
}
