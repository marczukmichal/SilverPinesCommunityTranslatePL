using HutongGames.PlayMaker;
using PowerTools;
using UnityEngine;

namespace Actions;

[ActionCategory(ActionCategory.Movement)]
public class RopeClimb : BaseCharacterMovementAction
{
	public AnimationClip m_climbUpAnim;

	public AnimationClip m_climbDownAnim;

	public Vector2 m_feetOffset;

	[HutongGames.PlayMaker.Tooltip("Event to send if at the top of a rope.")]
	public FsmEvent m_ropeExitTopEvent;

	[HutongGames.PlayMaker.Tooltip("Event to send if at the bottom of a rope.")]
	public FsmEvent m_ropeExitBottomEvent;

	public Vector2 m_ropeClimbOffset;

	protected RopeCheck m_ropeCheck;

	protected SpriteAnim m_spriteAnim;

	protected SpriteAnimNodes m_spriteAnimNodes;

	protected ClimbableRope m_rope;

	private bool m_goingUp;

	private AnimationEventsHelper m_animationEventsHelper;

	private CharacterTraversalUtils m_traversal;

	private CharacterStance m_stance;

	private Vector2 m_moveInput;

	public override void Awake()
	{
		base.Awake();
		if (!(base.Owner == null))
		{
			m_ropeCheck = base.Owner.GetComponentInChildren<RopeCheck>();
			m_spriteAnim = base.Owner.GetComponent<SpriteAnim>();
			m_animationEventsHelper = base.Owner.GetComponent<AnimationEventsHelper>();
			m_spriteAnimNodes = base.Owner.GetComponentInChildren<SpriteAnimNodes>();
			m_traversal = base.Owner.GetComponent<CharacterTraversalUtils>();
			m_stance = base.Owner.GetComponent<CharacterStance>();
		}
	}

	protected override Vector2 CalculateVelocity()
	{
		return m_moveInput * GetMoveSpeed();
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_animationEventsHelper.OnAnimationEventStringEvent.AddListener(OnAnimationEvent);
		m_rope = m_traversal.GetRope();
		m_goingUp = true;
		Vector3 position = base.Owner.transform.position;
		position.x = m_rope.transform.position.x;
		base.Owner.transform.position = position;
		ApplyRopeForce();
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
		float speed = Mathf.Abs(m_input.MovementInput.y);
		ClimbableRope.RopeState ropeState = m_rope.GetRopeState(m_stance);
		if ((ropeState.m_isAtTop && m_goingUp) || (ropeState.m_isAtBottom && !m_goingUp))
		{
			speed = 0f;
		}
		m_spriteAnim.SetSpeed(speed);
		Vector2 ropeClimbOffset = m_ropeClimbOffset;
		if (m_direction.CurrentDirection == CharacterDirection.Facing.Left)
		{
			ropeClimbOffset.x *= -1f;
		}
		Vector2 vector = base.Owner.transform.position;
		vector += ropeClimbOffset;
		Vector3 position = base.Owner.transform.position;
		position.x = m_rope.GetXForPosition(vector);
		base.Owner.transform.position = position;
		ApplyRopeSnapToLegsPosition();
		if (ropeState.m_isAtTop && m_input.MovementInput.y > 0.01f)
		{
			base.Fsm.Event(m_ropeExitTopEvent);
		}
		if (ropeState.m_isAtBottom && m_input.MovementInput.y < -0.01f)
		{
			base.Fsm.Event(m_ropeExitBottomEvent);
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		m_animationEventsHelper.OnAnimationEventStringEvent.RemoveListener(OnAnimationEvent);
		m_ropeCheck.BlockRegrab(m_rope);
		m_spriteAnim.SetSpeed(1f);
		m_traversal.AttachToRope(null);
		m_rope.ExitRope();
	}

	private void OnAnimationEvent(string eventString)
	{
		if (eventString.Equals("RopeForce"))
		{
			ApplyRopeForce();
		}
	}

	private void ApplyRopeForce()
	{
		m_rope.ApplyRopeForce(base.Owner.GetComponent<Transform>().position);
	}

	private void ApplyRopeSnapToLegsPosition()
	{
		m_rope.ApplyRopeSnapToLegsPosition(m_spriteAnimNodes.GetPosition(SpriteAnimNodeType.FootPosition));
	}
}
