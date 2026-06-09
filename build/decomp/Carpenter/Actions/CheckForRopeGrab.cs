using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.Character)]
public class CheckForRopeGrab : FsmStateAction
{
	protected RopeCheck m_ropeCheck;

	protected BaseCharacterInput m_input;

	[Tooltip("Event to send if rope grab detected.")]
	public FsmEvent m_ropeDetectedEvent;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_ropeCheck = base.Owner.GetComponentInChildren<RopeCheck>();
			m_input = base.Owner.GetCharacterInputComponent();
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_ropeCheck.IsChecking = true;
	}

	public override void OnExit()
	{
		base.OnExit();
		m_ropeCheck.IsChecking = false;
	}

	public override void OnUpdate()
	{
		if (m_ropeCheck.DetectedRope != null && m_ropeCheck.CanGrabRope)
		{
			if (m_input.MovementInput.y > 0.01f)
			{
				base.Fsm.Event(m_ropeDetectedEvent);
				Finish();
			}
			else if (m_input.MovementInput.y < -0.01f)
			{
				base.Fsm.Event(m_ropeDetectedEvent);
				Finish();
			}
		}
	}
}
