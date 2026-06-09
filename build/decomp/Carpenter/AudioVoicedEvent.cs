using System;
using FMODUnity;
using UnityEngine;

[CreateAssetMenu(fileName = "NewAudioVoicedEvent", menuName = "Audio/VoiceLine Event")]
public class AudioVoicedEvent : ScriptableObject
{
	[Serializable]
	public class VoiceLine
	{
		[SerializeField]
		private EventReference m_FMODEvent;

		[SerializeField]
		private ClipSubtitles m_clipSubtitles;

		public EventReference FMODEvent => m_FMODEvent;

		public ClipSubtitles Subtitles => m_clipSubtitles;
	}

	[SerializeField]
	private VoiceLine[] m_voiceLines;

	[SerializeField]
	private CharacterSpeakerSettings m_speaker;

	public VoiceLine[] VoiceLines => m_voiceLines;

	public CharacterSpeakerSettings Speaker => m_speaker;

	public void TriggerOnPlayer()
	{
		Transform audioTransform = null;
		GameObject item = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item;
		if (item != null)
		{
			audioTransform = item.transform;
		}
		GlobalReferences.Instance.EventChannels.Audio.PlayAudioVoiced.Raise(new PlayAudioVoicedEventData(this, audioTransform, isPlayer: true));
	}
}
