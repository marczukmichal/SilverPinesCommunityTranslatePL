using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class EnableCutsceneModeClip : PlayableAsset, ITimelineClipAsset
{
	public EnableCutsceneModeBehaviour m_cutsceneMode = new EnableCutsceneModeBehaviour();

	public ClipCaps clipCaps => ClipCaps.None;

	public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
	{
		return ScriptPlayable<EnableCutsceneModeBehaviour>.Create(graph, m_cutsceneMode);
	}
}
