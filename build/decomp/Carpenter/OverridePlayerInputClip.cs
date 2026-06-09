using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class OverridePlayerInputClip : PlayableAsset, ITimelineClipAsset
{
	public OverridePlayerInputBehaviour m_inputOverride = new OverridePlayerInputBehaviour();

	public ClipCaps clipCaps => ClipCaps.None;

	public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
	{
		return ScriptPlayable<OverridePlayerInputBehaviour>.Create(graph, m_inputOverride);
	}
}
