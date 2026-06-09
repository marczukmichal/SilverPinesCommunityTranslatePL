using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.Movement)]
public class IsClimbingStairs : FsmStateAction
{
	protected CharacterMovement m_movement;

	[Tooltip("Event to trigger on climbing up stairs")]
	public FsmEvent m_onClimbingUpStairsEvent;

	[Tooltip("Event to trigger on climbing down stairs")]
	public FsmEvent m_onClimbingDownStairsEvent;

	[Tooltip("Event to trigger on when on stairs but not going up or down")]
	public FsmEvent m_onStairsIdleEvent;

	[Tooltip("Event to trigger on not climbing stairs")]
	public FsmEvent m_onNotClimbingStairsEvent;

	public bool m_checkOnUpdate;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_movement = base.Owner.GetComponent<CharacterMovement>();
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
		if (!m_checkOnUpdate)
		{
			RunCheck();
			Finish();
		}
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (m_checkOnUpdate)
		{
			RunCheck();
		}
	}

	private void RunCheck()
	{
		if (!(m_movement != null))
		{
			return;
		}
		if (m_movement.AttachedStairs != null)
		{
			if (m_movement.PreviousVelocity.y > 0.1f)
			{
				if (m_onClimbingUpStairsEvent != null)
				{
					base.Fsm.Event(m_onClimbingUpStairsEvent);
				}
			}
			else if (m_movement.PreviousVelocity.y < -0.1f)
			{
				if (m_onClimbingDownStairsEvent != null)
				{
					base.Fsm.Event(m_onClimbingDownStairsEvent);
				}
			}
			else if (m_onStairsIdleEvent != null)
			{
				base.Fsm.Event(m_onStairsIdleEvent);
			}
		}
		else if (m_onNotClimbingStairsEvent != null)
		{
			base.Fsm.Event(m_onNotClimbingStairsEvent);
		}
	}
}
