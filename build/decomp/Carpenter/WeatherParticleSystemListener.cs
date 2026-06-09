using System;
using UnityEngine;
using UnityEngine.Events;

public class WeatherParticleSystemListener : MonoBehaviour
{
	[SerializeField]
	private Weather m_weather;

	[SerializeField]
	private AnimationCurve m_curve;

	private ParticleSystem m_particlesystem;

	private float m_baseRateOverTime;

	private ParticleSystem WeatherParticleSystem
	{
		get
		{
			if (m_particlesystem == null)
			{
				m_particlesystem = GetComponent<ParticleSystem>();
				m_baseRateOverTime = m_particlesystem.emission.rateOverTimeMultiplier;
			}
			return m_particlesystem;
		}
	}

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
		ParticleSystem.EmissionModule emission = WeatherParticleSystem.emission;
		emission.rateOverTimeMultiplier = m_curve.Evaluate(newRain) * m_baseRateOverTime;
	}
}
