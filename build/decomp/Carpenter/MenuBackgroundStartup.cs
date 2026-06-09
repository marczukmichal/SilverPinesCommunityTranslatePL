using UnityEngine;

public class MenuBackgroundStartup : MonoBehaviour
{
	[Header("Time Of Day")]
	[SerializeField]
	private TimeOfDay m_timeOfDay;

	[SerializeField]
	private TimeOfDay.TimePeriod m_startingTimeOfDay;

	[Header("Weather")]
	[SerializeField]
	private WeatherSettings m_weatherSettings;

	[SerializeField]
	private Weather m_activeWeather;

	[SerializeField]
	private float m_startingRain;

	private void Awake()
	{
		if ((bool)m_activeWeather)
		{
			m_activeWeather.SetActiveWeatherSettings(m_weatherSettings);
		}
		if (m_timeOfDay != null)
		{
			m_timeOfDay.SetTimeOfDay(m_startingTimeOfDay);
		}
	}
}
