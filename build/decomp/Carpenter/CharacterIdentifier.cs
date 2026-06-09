using UnityEngine;

public class CharacterIdentifier : MonoBehaviour
{
	public enum CharacterFaction
	{
		Unknown,
		Player,
		Monsters
	}

	[SerializeField]
	private CharacterFaction m_faction;

	[SerializeField]
	private CharacterSpeakerSettings m_speakerSettings;

	public CharacterFaction Faction => m_faction;

	public CharacterSpeakerSettings SpeakerSettings => m_speakerSettings;

	public string CharacterName
	{
		get
		{
			if (!(m_speakerSettings != null))
			{
				return "Unknown";
			}
			return m_speakerSettings.CharacterName;
		}
	}

	public Color SubtitlesColor
	{
		get
		{
			if (!(m_speakerSettings != null))
			{
				return Color.white;
			}
			return m_speakerSettings.SubtitlesColor;
		}
	}

	public static GameObject FindGameObjectForSpeakerSettings(CharacterSpeakerSettings speakerSettings)
	{
		CharacterIdentifier[] array = Object.FindObjectsByType<CharacterIdentifier>(FindObjectsSortMode.None);
		foreach (CharacterIdentifier characterIdentifier in array)
		{
			if (characterIdentifier.SpeakerSettings == speakerSettings)
			{
				return characterIdentifier.gameObject;
			}
		}
		return null;
	}
}
