using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.Character)]
public class CheckForLadderGrab : FsmStateAction
{
	protected LadderCheck m_ladderCheck;

	protected BaseCharacterInput m_input;

	[Tooltip("Event to send if ladder grab detected.")]
	public FsmEvent m_ladderDetectedEvent;

	public bool m_onlyOnce;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_ladderCheck = base.Owner.GetComponentInChildren<LadderCheck>();
			m_input = base.Owner.GetCharacterInputComponent();
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_ladderCheck.IsChecking = true;
	}

	public override void OnExit()
	{
		base.OnExit();
		m_ladderCheck.IsChecking = false;
	}

	public override void OnUpdate()
	{
		Ladder detectedLadder = m_ladderCheck.DetectedLadder;
		if (detectedLadder != null)
		{
			Ladder.LadderState ladderState = detectedLadder.GetLadderState(base.Owner.transform.position);
			if (!ladderState.m_isAtTop && m_input.MovementInput.y > 0.5f)
			{
				base.Fsm.Event(m_ladderDetectedEvent);
				Finish();
			}
			else if (!ladderState.m_isNearBottom && m_input.MovementInput.y < -0.5f)
			{
				base.Fsm.Event(m_ladderDetectedEvent);
				Finish();
			}
		}
		if (m_onlyOnce)
		{
			Finish();
		}
	}
}
