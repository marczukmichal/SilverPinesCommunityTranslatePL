using System;
using UnityEngine;

[Serializable]
public class DialogueSpeakerConfiguration
{
	[SerializeField]
	private CharacterSpeakerSettings m_speakerSettings;

	[SerializeField]
	private DialogueSpeakerSide m_speakerSide;

	public CharacterSpeakerSettings Speaker => m_speakerSettings;

	public DialogueSpeakerSide Side => m_speakerSide;
}
