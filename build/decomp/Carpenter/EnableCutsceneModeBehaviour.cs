using System;
using UnityEngine.Playables;

[Serializable]
public class EnableCutsceneModeBehaviour : PlayableBehaviour
{
	public override void OnBehaviourPlay(Playable playable, FrameData info)
	{
		GlobalReferences.Instance.EventChannels.GameCutscene.SetGameCutsceneEnabled.Raise(value: true);
	}

	public override void OnBehaviourPause(Playable playable, FrameData info)
	{
		GlobalReferences.Instance.EventChannels.GameCutscene.SetGameCutsceneEnabled.Raise(value: false);
	}

	public override void ProcessFrame(Playable playable, FrameData info, object playerData)
	{
	}
}
