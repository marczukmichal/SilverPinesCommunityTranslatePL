using HutongGames.PlayMaker;
using PowerTools;
using UnityEngine;

namespace Actions;

[ActionCategory(ActionCategory.Movement)]
public class LadderEnter : BaseCharacterMovementAction
{
	public AnimationClip m_ladderEnterStanding;

	public AnimationClip m_ladderEnterClimbDown;

	protected LadderCheck m_ladderCheck;

	protected SpriteAnim m_spriteAnim;

	[HutongGames.PlayMaker.Tooltip("Event to send after the animation is finished.")]
	public FsmEvent m_finishEvent;

	public float m_enterTopVerticalOffset;

	public float m_enterBottomVerticalOffset;

	private AnimationClip m_animationClip;

	private CharacterTraversalUtils m_traversal;

	public override void Awake()
	{
		base.Awake();
		if (!(base.Owner == null))
		{
			m_ladderCheck = base.Owner.GetComponentInChildren<LadderCheck>();
			m_input = base.Owner.GetCharacterInputComponent();
			m_spriteAnim = base.Owner.GetComponent<SpriteAnim>();
			m_traversal = base.Owner.GetComponent<CharacterTraversalUtils>();
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
		Ladder detectedLadder = m_ladderCheck.DetectedLadder;
		m_traversal.AttachToLadder(detectedLadder);
		Vector3 position = base.Owner.transform.position;
		Vector3 topBound = detectedLadder.TopBound;
		Vector3 bottomBound = detectedLadder.BottomBound;
		if (position.y + 1f >= topBound.y)
		{
			position.y = topBound.y + m_enterTopVerticalOffset;
			m_animationClip = m_ladderEnterClimbDown;
			m_traversal.m_ladderData.EnteredAtTop = true;
		}
		else
		{
			position.y = Mathf.Clamp(position.y, bottomBound.y + m_enterBottomVerticalOffset, topBound.y + m_enterTopVerticalOffset);
			m_animationClip = m_ladderEnterStanding;
			m_traversal.m_ladderData.EnteredAtTop = false;
		}
		position.x = detectedLadder.transform.position.x;
		base.Owner.transform.position = position;
		m_movement.ZForceDepthOverride = detectedLadder.transform.position.z;
		m_spriteAnim.Play(m_animationClip);
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (!m_spriteAnim.IsPlaying(m_animationClip))
		{
			base.Fsm.Event(m_finishEvent);
			Finish();
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		m_movement.ZForceDepthOverride = 0f;
	}
}
