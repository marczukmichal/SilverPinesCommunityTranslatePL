using FMODUnity;
using UnityEngine;
using UnityEngine.Video;

[CreateAssetMenu(menuName = "Video/Video Metadata")]
public class VideoMetadata : ScriptableObject
{
	[SerializeField]
	private VideoClip m_videoClip;

	[SerializeField]
	private EventReference m_fmodStartEvent;

	private bool m_stopFmodEventOnVideoEnd;

	[SerializeField]
	private EventReference m_fmodEndEvent;

	[SerializeField]
	private bool m_skippable;

	[SerializeField]
	private bool m_fadeOut = true;

	[SerializeField]
	private ClipSubtitles m_subtitles;

	public VideoClip VideoClip => m_videoClip;

	public EventReference FMODStartEvent => m_fmodStartEvent;

	public bool StopFmodEventOnVideoEnd => m_stopFmodEventOnVideoEnd;

	public EventReference FMODEndEvent => m_fmodEndEvent;

	public bool Skippable => m_skippable;

	public bool FadeOut => m_fadeOut;

	public ClipSubtitles Subtitles => m_subtitles;
}
