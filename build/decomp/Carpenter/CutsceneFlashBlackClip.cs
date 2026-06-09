using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class CutsceneFlashBlackClip : PlayableAsset, ITimelineClipAsset
{
	public CutsceneFlashBlackBehaviour m_behaviour = new CutsceneFlashBlackBehaviour();

	public ClipCaps clipCaps => ClipCaps.None;

	public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
	{
		return ScriptPlayable<CutsceneFlashBlackBehaviour>.Create(graph, m_behaviour);
	}
}
