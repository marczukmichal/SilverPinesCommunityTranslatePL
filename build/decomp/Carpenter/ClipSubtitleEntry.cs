using System;
using UnityEngine;
using UnityEngine.Localization;

[Serializable]
public class ClipSubtitleEntry
{
	public CharacterSpeakerSettings m_speaker;

	public float m_startTime;

	public float m_endTime;

	[Multiline(3)]
	public string m_text;

	public LocalizedString m_localizedString;

	public string SubtitlesText => LocalizationUtility.GetLocalizedString(m_localizedString, m_text);
}
