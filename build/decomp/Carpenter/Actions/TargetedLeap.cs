using DG.Tweening;
using HutongGames.PlayMaker;
using PowerTools;
using UnityEngine;

namespace Actions;

[ActionCategory(ActionCategory.Character)]
public class TargetedLeap : FsmStateAction
{
	public enum Mode
	{
		Linear,
		Path
	}

	[HutongGames.PlayMaker.Tooltip("Event to send on reached target.")]
	public FsmEvent m_targetReached;

	public AnimationClip m_animationClip;

	public float m_jumpSpeed = 10f;

	public float m_jumpHeight = 0.25f;

	public Ease m_ease = Ease.Linear;

	private Vector2 m_targetPosition;

	protected SpriteAnim m_spriteAnim;

	protected CharacterTargetedLeap m_targetedLeap;

	protected CharacterMovement m_movement;

	protected Rigidbody2D m_rigidbody2D;

	private bool m_done;

	public Mode m_mode;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_targetedLeap = base.Owner.GetComponent<CharacterTargetedLeap>();
			m_movement = base.Owner.GetComponent<CharacterMovement>();
			m_rigidbody2D = base.Owner.GetComponent<Rigidbody2D>();
			m_spriteAnim = base.Owner.GetComponent<SpriteAnim>();
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_done = false;
		m_targetedLeap.IsChecking = false;
		m_movement.enabled = false;
		Vector2 position = m_rigidbody2D.position;
		Vector2 detectedLeapPosition = m_targetedLeap.DetectedLeapPosition;
		if (m_mode == Mode.Path)
		{
			Vector2 vector = new Vector2((position.x + detectedLeapPosition.x) / 2f, Mathf.Max(position.y, detectedLeapPosition.y) + m_jumpHeight);
			Vector2[] path = new Vector2[3] { position, vector, detectedLeapPosition };
			m_rigidbody2D.DOPath(path, m_jumpSpeed, PathType.CatmullRom, PathMode.Sidescroller2D).OnComplete(delegate
			{
				m_done = true;
			}).SetEase(m_ease)
				.SetSpeedBased(isSpeedBased: true)
				.SetUpdate(UpdateType.Fixed);
		}
		else if (m_mode == Mode.Linear)
		{
			m_rigidbody2D.DOMove(detectedLeapPosition, m_jumpSpeed).OnComplete(delegate
			{
				m_done = true;
			}).SetEase(m_ease)
				.SetSpeedBased(isSpeedBased: true)
				.SetUpdate(UpdateType.Fixed);
		}
		m_spriteAnim.Play(m_animationClip);
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (m_done && !m_spriteAnim.IsPlaying(m_animationClip))
		{
			base.Fsm.Event(m_targetReached);
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		m_targetedLeap.OnLeapDone();
		m_movement.enabled = true;
	}
}
