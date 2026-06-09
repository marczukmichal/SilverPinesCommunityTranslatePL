using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class OverrideAIClip : PlayableAsset, ITimelineClipAsset
{
	public OverrideAIBehaviour m_settings = new OverrideAIBehaviour();

	public ClipCaps clipCaps => ClipCaps.None;

	public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
	{
		return ScriptPlayable<OverrideAIBehaviour>.Create(graph, m_settings);
	}
}
