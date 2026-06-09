using UnityEngine;

public class PlayAudioVoicedEventData
{
	public AudioVoicedEvent m_voiceLineEvent;

	public Transform m_transform;

	public CharacterSpeakerSettings m_speakerSettings;

	public bool m_isPlayer;

	public bool m_skippable;

	public float m_delay;

	public PlayAudioVoicedEventData(AudioVoicedEvent voiceLineEvent, Transform audioTransform, bool isPlayer, bool skippable = false, float delay = 0f)
	{
		m_voiceLineEvent = voiceLineEvent;
		m_transform = audioTransform;
		if ((bool)voiceLineEvent)
		{
			m_speakerSettings = voiceLineEvent.Speaker;
		}
		m_isPlayer = isPlayer;
		m_skippable = skippable;
		m_delay = delay;
	}
}
