using UnityEngine;

[CreateAssetMenu(fileName = "Radio Broadcast Subtitles", menuName = "Settings/Radio Broadcast Subtitles")]
public class RadioBroadcastSubtitles : ScriptableObject
{
	[SerializeField]
	private ClipSubtitles m_subtitles;

	[SerializeField]
	private CharacterSpeakerSettings m_defaultSpeakerSettings;

	public ClipSubtitles Subtitles => m_subtitles;
}
