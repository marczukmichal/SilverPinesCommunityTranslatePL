using System;
using UnityEngine;
using UnityEngine.Events;

public class WeatherWindRotator : MonoBehaviour
{
	[SerializeField]
	private Weather m_weather;

	[SerializeField]
	private Transform m_windRotateTrasnform;

	[SerializeField]
	private float m_windRotateAmount;

	private void Start()
	{
		WeatherUpdated(m_weather.CurrentRainAmount, m_weather.CurrentWindAmount);
	}

	private void OnEnable()
	{
		Weather weather = m_weather;
		weather.m_onWeatherChanged = (UnityAction<float, float>)Delegate.Combine(weather.m_onWeatherChanged, new UnityAction<float, float>(WeatherUpdated));
	}

	private void OnDisable()
	{
		Weather weather = m_weather;
		weather.m_onWeatherChanged = (UnityAction<float, float>)Delegate.Remove(weather.m_onWeatherChanged, new UnityAction<float, float>(WeatherUpdated));
	}

	private void WeatherUpdated(float newRain, float newWind)
	{
		if (m_windRotateTrasnform != null)
		{
			float z = newWind * m_windRotateAmount;
			m_windRotateTrasnform.rotation = Quaternion.Euler(0f, 0f, z);
		}
	}
}
