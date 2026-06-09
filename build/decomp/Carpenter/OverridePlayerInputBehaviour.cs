using System;
using UnityEngine.Playables;

[Serializable]
public class OverridePlayerInputBehaviour : PlayableBehaviour
{
	public PlayerInputOverride m_overrideType;

	public override void OnBehaviourPlay(Playable playable, FrameData info)
	{
		GlobalReferences.Instance.EventChannels.Generic.PlayerInputOverride.Raise(m_overrideType);
	}

	public override void OnBehaviourPause(Playable playable, FrameData info)
	{
		GlobalReferences.Instance.EventChannels.Generic.PlayerInputOverride.Raise(PlayerInputOverride.None);
	}

	public override void ProcessFrame(Playable playable, FrameData info, object playerData)
	{
	}
}
