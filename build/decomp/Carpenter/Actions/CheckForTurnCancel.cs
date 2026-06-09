using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.Character)]
public class CheckForTurnCancel : FsmStateAction
{
	[Tooltip("Event to trigger if cancelled")]
	public FsmEvent m_onTurnCancelled;

	private BaseCharacterInput m_input;

	private CharacterMovement m_movement;

	private CharacterDirection m_direction;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_input = base.Owner.GetCharacterInputComponent();
			m_movement = base.Owner.GetComponent<CharacterMovement>();
			m_direction = base.Owner.GetComponent<CharacterDirection>();
		}
	}

	public override void OnUpdate()
	{
		Check();
	}

	private void Check()
	{
		if (!m_direction.IsTurning)
		{
			return;
		}
		if (m_direction.DesiredDirection == CharacterDirection.Facing.Right)
		{
			if (m_input.MovementInput.x < -0.5f)
			{
				base.Fsm.Event(m_onTurnCancelled);
			}
		}
		else if (m_direction.DesiredDirection == CharacterDirection.Facing.Left && m_input.MovementInput.x > 0.5f)
		{
			base.Fsm.Event(m_onTurnCancelled);
		}
	}
}
