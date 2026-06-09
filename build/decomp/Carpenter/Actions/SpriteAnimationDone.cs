using HutongGames.PlayMaker;
using PowerTools;
using UnityEngine;

namespace Actions;

[ActionCategory(ActionCategory.Animation)]
public class SpriteAnimationDone : FsmStateAction
{
	[SerializeField]
	public AnimationClip m_animationClip;

	private SpriteAnim m_spriteAnim;

	[HutongGames.PlayMaker.Tooltip("Event to send after the animation is finished.")]
	public FsmEvent m_finishEvent;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_spriteAnim = base.Owner.GetComponent<SpriteAnim>();
		}
	}

	public override void OnUpdate()
	{
		if (!m_spriteAnim.IsPlaying(m_animationClip))
		{
			base.Fsm.Event(m_finishEvent);
			Finish();
		}
	}
}
