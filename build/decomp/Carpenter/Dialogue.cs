using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Dialogue")]
public class Dialogue : ScriptableObject
{
	[SerializeField]
	private DialogueSpeakerConfiguration[] m_speakerConfig;

	[SerializeField]
	private DialogueLine[] m_lines = new DialogueLine[0];

	public int LineCount => m_lines.Length;

	public DialogueLine GetLine(int index)
	{
		return m_lines[index];
	}

	public MusicSettings GetMusicSettings()
	{
		if (m_speakerConfig != null)
		{
			DialogueSpeakerConfiguration[] speakerConfig = m_speakerConfig;
			foreach (DialogueSpeakerConfiguration dialogueSpeakerConfiguration in speakerConfig)
			{
				if (dialogueSpeakerConfiguration.Speaker != null && dialogueSpeakerConfiguration.Speaker.Music != null)
				{
					return dialogueSpeakerConfiguration.Speaker.Music;
				}
			}
		}
		return null;
	}

	public DialogueSpeakerSide GetSpeakerSide(DialogueLine line)
	{
		if (m_speakerConfig != null)
		{
			DialogueSpeakerConfiguration[] speakerConfig = m_speakerConfig;
			foreach (DialogueSpeakerConfiguration dialogueSpeakerConfiguration in speakerConfig)
			{
				if (dialogueSpeakerConfiguration.Speaker != null && dialogueSpeakerConfiguration.Speaker == line.SpeakerSettings)
				{
					return dialogueSpeakerConfiguration.Side;
				}
			}
		}
		if (Application.isPlaying)
		{
			Debug.LogError("Speaker not defined to dialogue line with speaker " + line.SpeakerSettings);
		}
		return DialogueSpeakerSide.Left;
	}

	public List<CharacterSpeakerSettings> GetSpeakers()
	{
		List<CharacterSpeakerSettings> list = new List<CharacterSpeakerSettings>();
		if (m_speakerConfig != null)
		{
			DialogueSpeakerConfiguration[] speakerConfig = m_speakerConfig;
			foreach (DialogueSpeakerConfiguration dialogueSpeakerConfiguration in speakerConfig)
			{
				if (dialogueSpeakerConfiguration.Speaker != null)
				{
					list.Add(dialogueSpeakerConfiguration.Speaker);
				}
			}
		}
		return list;
	}
}
