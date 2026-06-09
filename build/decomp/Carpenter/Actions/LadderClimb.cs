using HutongGames.PlayMaker;
using PowerTools;
using UnityEngine;

namespace Actions;

[ActionCategory(ActionCategory.Movement)]
public class LadderClimb : BaseCharacterMovementAction
{
	public AnimationClip m_climbUpAnim;

	public AnimationClip m_climbDownAnim;

	public float m_enterFromTopInitialOffset;

	public float m_enterFromBottomInitialoffset;

	[HutongGames.PlayMaker.Tooltip("Event to send if at the top of a ladder.")]
	public FsmEvent m_ladderExitTopEvent;

	[HutongGames.PlayMaker.Tooltip("Event to send if at the bottom of a ladder.")]
	public FsmEvent m_ladderExitBottomEvent;

	protected Rigidbody2D m_rigidbody;

	protected LadderCheck m_ladderCheck;

	protected SpriteAnim m_spriteAnim;

	private bool m_goingUp;

	private CharacterTraversalUtils m_traversal;

	private Vector2 m_moveInput;

	public override void Awake()
	{
		base.Awake();
		if (!(base.Owner == null))
		{
			m_rigidbody = base.Owner.GetComponent<Rigidbody2D>();
			m_ladderCheck = base.Owner.GetComponentInChildren<LadderCheck>();
			m_spriteAnim = base.Owner.GetComponent<SpriteAnim>();
			m_traversal = base.Owner.GetComponent<CharacterTraversalUtils>();
		}
	}

	protected override Vector2 CalculateVelocity()
	{
		return m_moveInput * GetMoveSpeed();
	}

	public override void OnEnter()
	{
		base.OnEnter();
		if (m_traversal.m_ladderData.m_attachedLadder == null)
		{
			m_traversal.AttachToLadder(m_ladderCheck.DetectedLadder);
		}
		Ladder ladder = m_traversal.GetLadder();
		m_goingUp = !m_traversal.m_ladderData.EnteredAtTop;
		Vector3 position = m_rigidbody.transform.position;
		position.x = ladder.transform.position.x;
		if (m_traversal.m_ladderData.EnteredAtTop)
		{
			position.y += m_enterFromTopInitialOffset;
		}
		else
		{
			position.y += m_enterFromBottomInitialoffset;
		}
		m_rigidbody.transform.position = position;
		m_spriteAnim.Play(m_goingUp ? m_climbUpAnim : m_climbDownAnim);
		m_movement.ZForceDepthOverride = ladder.transform.position.z;
	}

	public override void OnUpdate()
	{
		Vector2 movementInput = m_input.MovementInput;
		movementInput.x = 0f;
		movementInput.y = Mathf.Round(movementInput.y);
		m_moveInput = movementInput;
		if (m_moveInput.y < 0f && m_goingUp)
		{
			m_goingUp = false;
			float normalizedTime = m_spriteAnim.NormalizedTime;
			m_spriteAnim.Play(m_climbDownAnim);
			m_spriteAnim.NormalizedTime = 1f - normalizedTime % 1f;
		}
		else if (m_moveInput.y > 0f && !m_goingUp)
		{
			m_goingUp = true;
			float normalizedTime2 = m_spriteAnim.NormalizedTime;
			m_spriteAnim.Play(m_climbUpAnim);
			m_spriteAnim.NormalizedTime = 1f - normalizedTime2 % 1f;
		}
		m_spriteAnim.SetSpeed(Mathf.Abs(movementInput.y));
		Ladder.LadderState ladderState = m_traversal.GetLadder().GetLadderState(base.Owner.transform.position);
		if (ladderState.m_isAtTop && m_input.MovementInput.y > 0.01f)
		{
			base.Fsm.Event(m_ladderExitTopEvent);
		}
		if (ladderState.m_isAtBottom && m_input.MovementInput.y < -0.01f)
		{
			base.Fsm.Event(m_ladderExitBottomEvent);
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		m_spriteAnim.SetSpeed(1f);
		m_movement.ZForceDepthOverride = 0f;
		m_traversal.AttachToLadder(null);
	}
}
