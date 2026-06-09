using System;
using FMODUnity;
using UnityEngine;
using UnityEngine.Localization;

[Serializable]
public class DialogueLine
{
	[SerializeField]
	private EventReference m_voiceLineClip;

	[SerializeField]
	public LocalizedString m_subtitlesStringReference;

	[SerializeField]
	private string m_rawString = "";

	[SerializeField]
	private CharacterSpeakerSettings m_speakerSettings;

	[SerializeField]
	private string m_alternativeSpriteID;

	public EventReference VoiceLineClip => m_voiceLineClip;

	public string Subtitles => LocalizationUtility.GetLocalizedString(m_subtitlesStringReference, m_rawString);

	public bool IsLocalized
	{
		get
		{
			if (m_subtitlesStringReference != null)
			{
				return !m_subtitlesStringReference.IsEmpty;
			}
			return false;
		}
	}

	public string RawString => m_rawString;

	public CharacterSpeakerSettings SpeakerSettings => m_speakerSettings;

	public string AlternativeSpriteID => m_alternativeSpriteID;
}
