using System;
using UnityEngine;

[Serializable]
public class ReloadAnimations
{
	[Header("Main")]
	[SerializeField]
	private AnimationClip m_startReloadAnimation;

	[SerializeField]
	private AnimationClip m_mainReloadAnimation;

	[SerializeField]
	private AnimationClip m_finishReloadAnimation;

	[Header("Front Arm")]
	[SerializeField]
	private AnimationClip m_startReloadArmAnimation;

	[SerializeField]
	private AnimationClip m_mainReloadArmAnimation;

	[SerializeField]
	private AnimationClip m_finishReloadArmAnimation;

	public AnimationClip StartReloadAnimation => m_startReloadAnimation;

	public AnimationClip MainReloadAnimation => m_mainReloadAnimation;

	public AnimationClip FinishReloadAnimation => m_finishReloadAnimation;

	public AnimationClip StartReloadArmAnimation => m_startReloadArmAnimation;

	public AnimationClip MainReloadArmAnimation => m_mainReloadArmAnimation;

	public AnimationClip FinishReloadArmAnimation => m_finishReloadArmAnimation;
}
