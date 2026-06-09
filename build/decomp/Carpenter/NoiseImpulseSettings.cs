using UnityEngine;

[CreateAssetMenu(fileName = "NoiseImpulseSetting", menuName = "Settings/Noise Impulse Settings")]
public class NoiseImpulseSettings : ScriptableObject
{
	[Tooltip("The distance this noise will travel")]
	[SerializeField]
	public float m_distance;

	[Tooltip("The intensity of the noise")]
	[SerializeField]
	public float m_intensity;

	[Tooltip("How long this noise impulse will last")]
	[SerializeField]
	public float m_duration;
}
