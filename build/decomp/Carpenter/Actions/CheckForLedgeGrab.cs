using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory(ActionCategory.Character)]
public class CheckForLedgeGrab : FsmStateAction
{
	[UIHint(UIHint.Variable)]
	[RequiredField]
	public FsmBool m_isJumpingVariable;

	public bool m_everyFrame;

	public bool m_canClimbUp = true;

	public bool m_canClimbDown = true;

	public bool m_climbOnHorizontalMovement;

	public bool m_climbOnJumpInput;

	[HutongGames.PlayMaker.Tooltip("Event to send if trying to ledge grab detected.")]
	public FsmEvent m_useLedgeEvent;

	public bool m_useOffsetOverride;

	public Vector2 m_offsetOverride;

	private BaseCharacterInput m_input;

	private CharacterLedgeGrab m_ledgeGrab;

	private CharacterMovement m_movement;

	private float m_minimumHeightForClimbUp = 1.2f;

	private float m_minimumHeightForClimbDown = 1.8f;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_input = base.Owner.GetCharacterInputComponent();
			m_ledgeGrab = base.Owner.GetComponent<CharacterLedgeGrab>();
			m_movement = base.Owner.GetComponent<CharacterMovement>();
		}
	}

	public override void Reset()
	{
		base.Reset();
		m_useLedgeEvent = new FsmEvent("Ledge/Use");
	}

	public override void OnEnter()
	{
		CheckForLedge();
		if (!m_everyFrame)
		{
			Finish();
		}
		if (m_useOffsetOverride)
		{
			m_ledgeGrab.SetOffsetOverride(m_offsetOverride);
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		if (m_useOffsetOverride)
		{
			m_ledgeGrab.ClearOffsetOverride();
		}
	}

	public override void OnUpdate()
	{
		if (m_everyFrame)
		{
			CheckForLedge();
		}
	}

	private bool IsBlockedByJumping(CharacterLedgeGrab.DetectedLedge ledge)
	{
		return false;
	}

	private void CheckForLedge()
	{
		bool flag = !m_isJumpingVariable.Value;
		bool flag2 = m_input.MovementInput.y < -0.7f;
		bool num = m_input.MovementInput.y > 0.7f || (m_climbOnJumpInput && m_input.IsJumping);
		bool flag3 = Mathf.Abs(m_input.MovementInput.x) > 0.5f;
		bool flag4 = (num || (flag3 && m_climbOnHorizontalMovement) || !flag) && !flag2;
		CharacterLedgeGrab.DetectedLedge detectedForwardLedge = m_ledgeGrab.DetectedForwardLedge;
		CharacterLedgeGrab.DetectedLedge detectedClimbUpLedge = m_ledgeGrab.DetectedClimbUpLedge;
		CharacterLedgeGrab.DetectedLedge detectedClimbDownLedge = m_ledgeGrab.DetectedClimbDownLedge;
		if (flag4 && detectedClimbUpLedge.m_ledgeType == CharacterLedgeGrab.DetectedLedgeType.Climb && m_canClimbUp)
		{
			if (m_ledgeGrab.CheckLedgeValidity(detectedClimbUpLedge) && detectedClimbUpLedge.m_exitHeightOffsetUp > m_minimumHeightForClimbUp)
			{
				m_ledgeGrab.SetActiveLedge(detectedClimbUpLedge);
				base.Fsm.Event(m_useLedgeEvent);
			}
		}
		else if (flag4 && detectedForwardLedge.m_ledgeType != 0 && m_canClimbUp)
		{
			if (m_ledgeGrab.CheckLedgeValidity(detectedForwardLedge))
			{
				if (detectedForwardLedge.m_ledgeType == CharacterLedgeGrab.DetectedLedgeType.Vault)
				{
					base.Fsm.Event(m_useLedgeEvent);
					m_ledgeGrab.SetActiveLedge(detectedForwardLedge);
				}
				else if (detectedForwardLedge.m_exitHeightOffsetUp > m_minimumHeightForClimbUp && !IsBlockedByJumping(detectedForwardLedge))
				{
					base.Fsm.Event(m_useLedgeEvent);
					m_ledgeGrab.SetActiveLedge(detectedForwardLedge);
				}
			}
		}
		else if (flag2 && m_canClimbDown && (detectedClimbDownLedge.m_ledgeType == CharacterLedgeGrab.DetectedLedgeType.ClimbDown || detectedClimbDownLedge.m_ledgeType == CharacterLedgeGrab.DetectedLedgeType.ClimbDownPlatform) && m_ledgeGrab.CheckLedgeValidity(detectedClimbDownLedge) && (detectedClimbDownLedge.m_ledgeType == CharacterLedgeGrab.DetectedLedgeType.ClimbDownPlatform || detectedClimbDownLedge.m_exitHeightOffsetDown > m_minimumHeightForClimbDown))
		{
			m_ledgeGrab.SetActiveLedge(detectedClimbDownLedge);
			base.Fsm.Event(m_useLedgeEvent);
		}
	}
}
