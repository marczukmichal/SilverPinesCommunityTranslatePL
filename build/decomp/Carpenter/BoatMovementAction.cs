using HutongGames.PlayMaker;

[ActionCategory(ActionCategory.Movement)]
public class BoatMovementAction : FsmStateAction
{
	private BoatMovement m_movement;

	private BaseCharacterInput m_input;

	private CharacterDirection m_direction;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_input = base.Owner.GetCharacterInputComponent();
			m_movement = base.Owner.GetComponent<BoatMovement>();
			m_direction = base.Owner.GetComponent<CharacterDirection>();
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		float x = m_input.MovementInput.x;
		m_movement.SetInput(x);
		if (m_input.MovementInput.x > GameUtils.Constants.s_inputMoveDeadzoneMinValue)
		{
			m_direction.DesiredDirection = CharacterDirection.Facing.Right;
		}
		else if (m_input.MovementInput.x < 0f - GameUtils.Constants.s_inputMoveDeadzoneMinValue)
		{
			m_direction.DesiredDirection = CharacterDirection.Facing.Left;
		}
	}
}
