using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.Character)]
public class EnterLedgeGrab : FsmStateAction
{
	[Tooltip("Event to send if trying to climb up detected.")]
	public FsmEvent m_climbUpDetectedEvent;

	[Tooltip("Event to send if trying to climb with swing detected.")]
	public FsmEvent m_climbUpSwingEvent;

	[Tooltip("Event to send if trying to climb with jump grab detected.")]
	public FsmEvent m_climbUpJumpGrabEvent;

	[Tooltip("Event to send if a vaultable ledge is detected.")]
	public FsmEvent m_vaultDetectedEvent;

	[Tooltip("Event to send if we try to climb down")]
	public FsmEvent m_climbDownDetectedEvent;

	[Tooltip("Event to send if we try to climb down free climb ledge")]
	public FsmEvent m_freeClimbDownDetectedEvent;

	private CharacterLedgeGrab m_ledgeGrab;

	private CharacterMovement m_movement;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_ledgeGrab = base.Owner.GetComponent<CharacterLedgeGrab>();
			m_movement = base.Owner.GetComponent<CharacterMovement>();
		}
	}

	public override void Reset()
	{
		base.Reset();
		m_climbUpDetectedEvent = new FsmEvent("Ledge/ClimbUp");
		m_climbUpSwingEvent = new FsmEvent("Ledge/GrabSwing");
		m_climbUpJumpGrabEvent = new FsmEvent("Ledge/JumpGrab");
		m_climbDownDetectedEvent = new FsmEvent("Ledge/ClimbDown");
		m_freeClimbDownDetectedEvent = new FsmEvent("Ledge/FreeClimbDown");
		m_vaultDetectedEvent = new FsmEvent("Ledge/Vault");
	}

	public override void OnEnter()
	{
		SendLedgeEvent();
		Finish();
	}

	private void SendLedgeEvent()
	{
		CharacterLedgeGrab.DetectedLedge activeLedge = m_ledgeGrab.ActiveLedge;
		if (activeLedge.m_ledgeType == CharacterLedgeGrab.DetectedLedgeType.None)
		{
			return;
		}
		if (activeLedge.m_ledgeType == CharacterLedgeGrab.DetectedLedgeType.ClimbDown)
		{
			base.Fsm.Event(m_climbDownDetectedEvent);
			return;
		}
		if (activeLedge.m_ledgeType == CharacterLedgeGrab.DetectedLedgeType.ClimbDownPlatform)
		{
			base.Fsm.Event(m_freeClimbDownDetectedEvent);
			return;
		}
		if (activeLedge.m_ledgeType == CharacterLedgeGrab.DetectedLedgeType.Vault)
		{
			base.Fsm.Event(m_vaultDetectedEvent);
			return;
		}
		if (activeLedge.CanSwingFromMovememnt(m_movement))
		{
			base.Fsm.Event(m_climbUpSwingEvent);
			return;
		}
		bool flag = false;
		if (!m_movement.IsGrounded && m_movement.GetDistanceFromGround() > 1f && activeLedge.m_ledgeHeightOffset < 1.4f)
		{
			flag = true;
		}
		if (flag)
		{
			base.Fsm.Event(m_climbUpJumpGrabEvent);
		}
		else
		{
			base.Fsm.Event(m_climbUpDetectedEvent);
		}
	}
}
