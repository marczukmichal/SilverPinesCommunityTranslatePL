using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("Dialogue")]
public class ShowSubtitlesAction : FsmStateAction
{
	[SerializeField]
	public ClipSubtitlesAsset m_subtitles;

	private float m_timer;

	private ClipSubtitleEntry m_activeSubtitlesEntry;

	public override void OnEnter()
	{
		m_timer = 0f;
		m_activeSubtitlesEntry = null;
	}

	public override void OnExit()
	{
		if (m_activeSubtitlesEntry != null)
		{
			GlobalReferences.Instance.EventChannels.Audio.ShowVoiceSubtitles.Raise(new SubtitlesEventData
			{
				m_enable = false
			});
			m_activeSubtitlesEntry = null;
		}
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		m_timer += Time.deltaTime;
		ClipSubtitleEntry entryForTimestamp = m_subtitles.Subtitles.GetEntryForTimestamp(m_timer);
		if (m_activeSubtitlesEntry != entryForTimestamp)
		{
			if (entryForTimestamp != null)
			{
				GlobalReferences.Instance.EventChannels.Audio.ShowVoiceSubtitles.Raise(new SubtitlesEventData
				{
					m_enable = true,
					m_speakerSettings = entryForTimestamp.m_speaker,
					m_text = entryForTimestamp.SubtitlesText
				});
			}
			else
			{
				GlobalReferences.Instance.EventChannels.Audio.ShowVoiceSubtitles.Raise(new SubtitlesEventData
				{
					m_enable = false
				});
			}
		}
		m_activeSubtitlesEntry = entryForTimestamp;
	}
}
