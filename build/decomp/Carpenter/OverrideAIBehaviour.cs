using System;
using UnityEngine;
using UnityEngine.Playables;

[Serializable]
public class OverrideAIBehaviour : PlayableBehaviour, IAIBrainControlAction
{
	public enum OverrideBehaviorMode
	{
		None,
		DisableAI
	}

	public OverrideBehaviorMode m_overrideMode;

	private GameObject m_activeObject;

	public Vector2 MoveDirection => Vector2.zero;

	public override void ProcessFrame(Playable playable, FrameData info, object playerData)
	{
		GameObject gameObject = playerData as GameObject;
		if (gameObject != null)
		{
			m_activeObject = gameObject;
			ApplyAIOverride(m_activeObject, m_overrideMode);
		}
	}

	public override void OnBehaviourPause(Playable playable, FrameData info)
	{
		base.OnBehaviourPause(playable, info);
		ApplyAIOverride(m_activeObject, OverrideBehaviorMode.None);
	}

	private void ApplyAIOverride(GameObject aiCharacter, OverrideBehaviorMode mode)
	{
		if (aiCharacter == null || !Application.isPlaying)
		{
			return;
		}
		AIBrain component = aiCharacter.GetComponent<AIBrain>();
		switch (mode)
		{
		case OverrideBehaviorMode.None:
			if (component != null)
			{
				component.ActiveControlAction = null;
			}
			break;
		case OverrideBehaviorMode.DisableAI:
			if (component != null)
			{
				component.ActiveControlAction = this;
			}
			break;
		}
	}
}
