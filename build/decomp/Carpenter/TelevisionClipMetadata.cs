using System;
using FMODUnity;
using UnityEngine;
using UnityEngine.Video;

[Serializable]
[CreateAssetMenu(menuName = "Settings/Television Clip Metadata")]
public class TelevisionClipMetadata : ScriptableObject
{
	[SerializeField]
	private VideoClip m_videoClip;

	[SerializeField]
	private EventReference m_FMODEvent;

	[SerializeField]
	private ClipSubtitles m_subtitles;

	public VideoClip VideoClip => m_videoClip;

	public EventReference FMODEvent => m_FMODEvent;

	public ClipSubtitles Subtitles => m_subtitles;
}
