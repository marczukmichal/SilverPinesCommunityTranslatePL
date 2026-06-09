using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.Movement)]
public class CheckForTurn : FsmStateAction
{
	[Tooltip("Event to trigger when turn animation is needed")]
	public FsmEvent m_turnAnimation;

	public bool m_onceOnly;

	public bool m_canTurnWithAimInput;

	public float m_allowedTimeInState = -1f;

	private BaseCharacterInput m_input;

	private CharacterDirection m_characterDirection;

	private bool m_turned;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_input = base.Owner.GetCharacterInputComponent();
			m_characterDirection = base.Owner.GetComponent<CharacterDirection>();
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
		if (m_turnAnimation != null)
		{
			m_characterDirection.HasTurnAnimation = true;
		}
		m_turned = false;
	}

	public override void OnUpdate()
	{
		if ((m_onceOnly && m_turned) || (m_allowedTimeInState > 0f && base.State.StateTime > m_allowedTimeInState) || (m_input != null && m_input.TurnDisabled))
		{
			return;
		}
		CharacterDirection.Facing desiredDirection = m_characterDirection.DesiredDirection;
		if (m_input != null)
		{
			if (m_input.MovementInput.x > GameUtils.Constants.s_inputMoveDeadzoneMinValue)
			{
				m_characterDirection.DesiredDirection = CharacterDirection.Facing.Right;
			}
			else if (m_input.MovementInput.x < 0f - GameUtils.Constants.s_inputMoveDeadzoneMinValue)
			{
				m_characterDirection.DesiredDirection = CharacterDirection.Facing.Left;
			}
			else if (m_canTurnWithAimInput && m_input.AimDirectionInput.x > GameUtils.Constants.s_inputMoveDeadzoneMinValue)
			{
				m_characterDirection.DesiredDirection = CharacterDirection.Facing.Right;
			}
			else if (m_canTurnWithAimInput && m_input.AimDirectionInput.x < 0f - GameUtils.Constants.s_inputMoveDeadzoneMinValue)
			{
				m_characterDirection.DesiredDirection = CharacterDirection.Facing.Left;
			}
			else if (m_input.FacingDirectionInput != 0)
			{
				m_characterDirection.DesiredDirection = m_input.FacingDirectionInput;
			}
		}
		if (m_characterDirection.DesiredDirection != m_characterDirection.CurrentDirection)
		{
			base.Fsm.Event(m_turnAnimation);
		}
		if (desiredDirection != m_characterDirection.CurrentDirection)
		{
			m_turned = true;
		}
	}

	public override void OnExit()
	{
		base.OnEnter();
		if (m_turnAnimation != null)
		{
			m_characterDirection.HasTurnAnimation = false;
		}
	}
}
