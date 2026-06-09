using HutongGames.PlayMaker;
using PowerTools;
using UnityEngine;

namespace Actions;

[ActionCategory(ActionCategory.Character)]
public class IsMovingHorizontally : FsmStateAction
{
	public enum AnimationTimeRestriction
	{
		None,
		Normalised,
		Real
	}

	[HutongGames.PlayMaker.Tooltip("Event to trigger if moving")]
	public FsmEvent m_onMovingEvent;

	[HutongGames.PlayMaker.Tooltip("Event to trigger if not moving")]
	public FsmEvent m_onNotMovingEvent;

	[HutongGames.PlayMaker.Tooltip("If true read input instead of rigibody velocity")]
	[SerializeField]
	public bool m_readInput;

	[HutongGames.PlayMaker.Tooltip("Select if there is a restriction on when these events will be sent based on the current animation time")]
	[SerializeField]
	public AnimationTimeRestriction m_AnimationTimeRestriction;

	[HutongGames.PlayMaker.Tooltip("Where in the animation these events are allowed to be sent")]
	public Vector2 m_onlyDuringAnimationTime = new Vector2(0f, 1f);

	public float m_requiredTimeThreshold = 0.1f;

	private BaseCharacterInput m_input;

	private SpriteAnim m_spriteAnim;

	private LegacyCharacterMovement m_legacyMovement;

	private CharacterMovement m_movement;

	private static readonly float m_minMovementThreshold = 0.1f;

	private float m_notMovingTime;

	private float m_isMovingTime;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_input = base.Owner.GetCharacterInputComponent();
			m_spriteAnim = base.Owner.GetComponent<SpriteAnim>();
			m_legacyMovement = base.Owner.GetComponent<LegacyCharacterMovement>();
			m_movement = base.Owner.GetComponent<CharacterMovement>();
		}
	}

	public override void OnEnter()
	{
		m_notMovingTime = 0f;
		m_isMovingTime = 0f;
		Check();
	}

	public override void OnUpdate()
	{
		Check();
	}

	private void Check()
	{
		bool flag = true;
		if (m_spriteAnim != null && m_AnimationTimeRestriction != 0)
		{
			if (m_AnimationTimeRestriction == AnimationTimeRestriction.Normalised)
			{
				float num = m_spriteAnim.NormalizedTime % 1f;
				flag = num >= m_onlyDuringAnimationTime.x && num <= m_onlyDuringAnimationTime.y;
			}
			else
			{
				flag = m_spriteAnim.Time >= m_onlyDuringAnimationTime.x && m_spriteAnim.Time <= m_onlyDuringAnimationTime.y;
			}
		}
		if (flag)
		{
			bool flag2 = (m_readInput ? (Mathf.Abs(m_input.MovementInput.x) > m_minMovementThreshold) : ((!(m_movement != null)) ? (Mathf.Abs(m_legacyMovement.RelativeVelocity.x) > m_minMovementThreshold) : (Mathf.Abs(m_movement.PreviousVelocity.x) > m_minMovementThreshold)));
			if (flag2)
			{
				m_isMovingTime += Time.deltaTime;
				m_notMovingTime = 0f;
			}
			else
			{
				m_notMovingTime += Time.deltaTime;
				m_isMovingTime = 0f;
			}
			if (m_onMovingEvent != null && flag2 && m_isMovingTime > m_requiredTimeThreshold)
			{
				base.Fsm.Event(m_onMovingEvent);
			}
			if (m_onNotMovingEvent != null && !flag2 && m_notMovingTime > m_requiredTimeThreshold)
			{
				base.Fsm.Event(m_onNotMovingEvent);
			}
		}
	}
}
