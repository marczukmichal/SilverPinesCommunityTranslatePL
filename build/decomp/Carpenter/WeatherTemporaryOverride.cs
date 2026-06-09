using UnityEngine;

public class WeatherTemporaryOverride : MonoBehaviour
{
	[SerializeField]
	private WeatherSettings m_weatherOverride;

	[SerializeField]
	private bool m_instant;

	private WeatherSettings m_previousWeatherSettings;

	private void OnEnable()
	{
		Weather weather = GlobalReferences.Instance.Weather;
		m_previousWeatherSettings = weather.ActiveWeatherSettings;
		weather.SetActiveWeatherSettings(m_weatherOverride, m_instant);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.Weather.SetActiveWeatherSettings(m_previousWeatherSettings, m_instant);
	}
}
