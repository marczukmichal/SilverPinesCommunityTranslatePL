using UnityEngine;

[CreateAssetMenu(fileName = "Radio Broadcast Settings", menuName = "Settings/Radio Broadcast Settings")]
public class RadioBroadcastSettings : ScriptableObject
{
	public MinigameRadio.RadioMode m_mode;

	public float m_frequencyValue;

	public float m_validRange;

	public string m_fmodParameterName;

	public float m_fmodFrequencyRange;
}
