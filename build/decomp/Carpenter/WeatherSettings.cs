using UnityEngine;

[CreateAssetMenu(menuName = "Misc/Weather Settings")]
public class WeatherSettings : AddressableScriptableObject<WeatherSettings>
{
	[Header("Rain Settings")]
	[SerializeField]
	private float m_baseRainAmount;

	[SerializeField]
	private float m_rainVarianceAmount;

	[SerializeField]
	private float m_rainVarianceFrequency;

	[Header("Wind Settings")]
	[SerializeField]
	private float m_baseWindAmount;

	[SerializeField]
	private float m_windVarianceAmount;

	[SerializeField]
	private float m_windVarianceFrequency;

	public float BaseRainAmount => m_baseRainAmount;

	public float RainVarianceAmount => m_rainVarianceAmount;

	public float RainVarianceFrequency => m_rainVarianceFrequency;

	public float BaseWindAmount => m_baseWindAmount;

	public float WindVarianceAmount => m_windVarianceAmount;

	public float WindVarianceFrequency => m_windVarianceFrequency;
}
