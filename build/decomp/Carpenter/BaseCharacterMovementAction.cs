using HutongGames.PlayMaker;
using UnityEngine;

public class BaseCharacterMovementAction : FsmStateAction
{
	public CharacterMovementSettings m_movementSettings;

	[HutongGames.PlayMaker.HideIf("HasMovemementSettings")]
	public float m_moveSpeed;

	protected CharacterMovement m_movement;

	protected CharacterDirection m_direction;

	protected BaseCharacterInput m_input;

	protected StatusEffectReceiver m_statusEffectReceiver;

	protected Vector2 m_velocity;

	protected virtual float Gravity => -10f;

	public bool HasMovemementSettings()
	{
		return m_movementSettings != null;
	}

	protected virtual float GetMoveSpeed()
	{
		if (m_movement.OverrideMovementSettings != null)
		{
			return m_movement.OverrideMovementSettings.MoveSpeed;
		}
		if (m_movementSettings != null)
		{
			return m_movementSettings.MoveSpeed;
		}
		return m_moveSpeed;
	}

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_input = base.Owner.GetCharacterInputComponent();
			m_movement = base.Owner.GetComponent<CharacterMovement>();
			m_direction = base.Owner.GetComponent<CharacterDirection>();
			m_statusEffectReceiver = base.Owner.GetComponent<StatusEffectReceiver>();
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_velocity = m_movement.PreviousVelocity;
		m_movement.MovementOverriden = true;
		m_movement.SetMovementSettingsOverride(m_movementSettings);
	}

	public override void OnExit()
	{
		base.OnExit();
		m_movement.MovementOverriden = false;
		m_movement.SetMovementSettingsOverride(null);
	}

	public override void OnPreprocess()
	{
		base.Fsm.HandleFixedUpdate = true;
	}

	public override void OnFixedUpdate()
	{
		m_velocity = CalculateVelocity();
		m_movement.Move(m_velocity, standingOnPlatform: false);
		if (m_movement.m_collisions.m_above || m_movement.m_collisions.m_below)
		{
			if (m_movement.m_collisions.m_slidingDownMaxSlope)
			{
				m_velocity.y += m_movement.m_collisions.m_slopeNormal.y * (0f - Gravity) * Time.deltaTime;
			}
			else
			{
				m_velocity.y = 0f;
			}
		}
	}

	protected virtual Vector2 CalculateVelocity()
	{
		return Vector2.zero;
	}
}
