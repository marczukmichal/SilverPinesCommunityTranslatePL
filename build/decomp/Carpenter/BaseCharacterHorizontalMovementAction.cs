using HutongGames.PlayMaker;

public class BaseCharacterHorizontalMovementAction : FsmStateAction
{
	public CharacterMovementSettings m_movementSettings;

	[HideIf("HasMovemementSettings")]
	public FsmFloat m_moveSpeed;

	protected CharacterMovement m_movement;

	protected CharacterDirection m_direction;

	protected BaseCharacterInput m_input;

	public bool HasMovemementSettings()
	{
		return m_movementSettings != null;
	}

	protected float GetMoveSpeed()
	{
		if (m_movementSettings != null)
		{
			return m_movementSettings.MoveSpeed;
		}
		return m_moveSpeed.Value;
	}

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_input = base.Owner.GetCharacterInputComponent();
			m_movement = base.Owner.GetComponent<CharacterMovement>();
			m_direction = base.Owner.GetComponent<CharacterDirection>();
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
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
		m_movement.MoveHorizontally(CalculateXVelocity());
	}

	protected virtual float CalculateXVelocity()
	{
		return 0f;
	}
}
