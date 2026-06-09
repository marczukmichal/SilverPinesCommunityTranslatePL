using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class CutsceneFlashImageClip : PlayableAsset, ITimelineClipAsset
{
	[SerializeField]
	private CutsceneFlashImageBehaviour m_behaviour = new CutsceneFlashImageBehaviour();

	public ClipCaps clipCaps => ClipCaps.None;

	public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
	{
		return ScriptPlayable<CutsceneFlashImageBehaviour>.Create(graph, m_behaviour);
	}
}
