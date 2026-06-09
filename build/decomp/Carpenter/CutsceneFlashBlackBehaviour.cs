using System;
using UnityEngine.Playables;

[Serializable]
public class CutsceneFlashBlackBehaviour : PlayableBehaviour
{
	public CutsceneFlashes.BlackFlashSettings m_blackFlashSettings;

	private CutsceneFlashes m_flashes;

	public override void OnBehaviourPlay(Playable playable, FrameData info)
	{
	}

	public override void OnBehaviourPause(Playable playable, FrameData info)
	{
		if (m_flashes != null)
		{
			m_flashes.Clear();
		}
	}

	public override void ProcessFrame(Playable playable, FrameData info, object playerData)
	{
		m_flashes = playerData as CutsceneFlashes;
		if (m_flashes != null)
		{
			m_flashes.ShowFlashes(m_blackFlashSettings, (float)(playable.GetTime() / playable.GetDuration()));
		}
	}
}
