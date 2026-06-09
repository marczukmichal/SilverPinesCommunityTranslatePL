using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.VFX;

public class WeatherVisualEffectListener : MonoBehaviour
{
	[SerializeField]
	private Weather m_weather;

	[SerializeField]
	private AnimationCurve m_curve;

	[FormerlySerializedAs("m_parameterName")]
	[SerializeField]
	private string m_rainParameterName = "RainAmount";

	[SerializeField]
	private string m_windParameterName = "WindAmount";

	private VisualEffect m_effect;

	private VisualEffect WeatherVisualEffect
	{
		get
		{
			if (m_effect == null)
			{
				m_effect = GetComponent<VisualEffect>();
			}
			return m_effect;
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
		if (WeatherVisualEffect.HasFloat(m_rainParameterName))
		{
			WeatherVisualEffect.SetFloat(m_rainParameterName, m_curve.Evaluate(newRain));
		}
		if (WeatherVisualEffect.HasFloat(m_windParameterName))
		{
			WeatherVisualEffect.SetFloat(m_windParameterName, m_curve.Evaluate(newWind));
		}
	}
}
