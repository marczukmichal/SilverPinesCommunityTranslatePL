using System;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(menuName = "Settings/Character Speaker")]
public class CharacterSpeakerSettings : ScriptableObject
{
	[Serializable]
	public class SpeakerPortraitSetting
	{
		public string m_id;

		public Sprite m_sprite;
	}

	[SerializeField]
	private LocalizedString m_characterNameStringReference;

	[SerializeField]
	private Color m_subtitlesColor;

	[SerializeField]
	private Sprite m_portrait;

	[SerializeField]
	private SpeakerPortraitSetting[] m_alternativeSpeakerPortraits;

	[SerializeField]
	private MusicSettings m_music;

	public string CharacterName
	{
		get
		{
			if (m_characterNameStringReference.IsEmpty)
			{
				return base.name;
			}
			return m_characterNameStringReference.GetLocalizedString();
		}
	}

	public Color SubtitlesColor => m_subtitlesColor;

	public Sprite Portrait => m_portrait;

	public SpeakerPortraitSetting[] AlternativeSpeakerPortraits => m_alternativeSpeakerPortraits;

	public MusicSettings Music => m_music;

	public Sprite GetPortraitSprite(string alternativePortraitID)
	{
		if (!string.IsNullOrEmpty(alternativePortraitID))
		{
			SpeakerPortraitSetting[] alternativeSpeakerPortraits = m_alternativeSpeakerPortraits;
			foreach (SpeakerPortraitSetting speakerPortraitSetting in alternativeSpeakerPortraits)
			{
				if (speakerPortraitSetting.m_id.Equals(alternativePortraitID))
				{
					return speakerPortraitSetting.m_sprite;
				}
			}
		}
		return m_portrait;
	}
}
