using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory(ActionCategory.Animation)]
public class SetNextAnimationStartTime : FsmStateAction
{
	[SerializeField]
	public AnimationClip m_animationClip;

	[HutongGames.PlayMaker.Tooltip("The normalized animation time to jump to in the matching animation clip")]
	[SerializeField]
	public float m_startTime = 0.5f;

	private AnimationEventsHelper m_animHelper;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_animHelper = base.Owner.GetComponent<AnimationEventsHelper>();
		}
	}

	public override void OnEnter()
	{
		m_animHelper.SetNextAnimStartTime(m_animationClip, m_startTime);
		Finish();
	}
}
