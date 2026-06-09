using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory(ActionCategory.Animation)]
public class PlayArmAnimation : FsmStateAction
{
	public AnimationClip m_animationClip;

	private CharacterArmAnimations m_armAnimations;

	[UIHint(UIHint.Variable)]
	public FsmObject m_animationVariable;

	private AnimationClip m_playingClip;

	protected virtual AnimationClip GetAnimationClip()
	{
		if (m_animationVariable != null && !m_animationVariable.IsNone && m_animationVariable.Value is AnimationClip)
		{
			return m_animationVariable.Value as AnimationClip;
		}
		return m_animationClip;
	}

	public override void Awake()
	{
		base.Awake();
		if (base.Owner != null)
		{
			m_armAnimations = base.Owner.GetComponentInChildren<CharacterArmAnimations>(includeInactive: true);
		}
	}

	public override void OnEnter()
	{
		m_playingClip = GetAnimationClip();
		m_armAnimations.Play(m_playingClip);
	}

	public override void OnUpdate()
	{
		if (m_playingClip == null || !m_armAnimations.SpriteAnim.IsPlaying(m_playingClip))
		{
			Finish();
		}
	}

	public override void OnExit()
	{
		m_armAnimations.Play(null);
	}
}
