using UnityEngine;
using UnityEngine.Events;

public class Weather : ScriptableObject
{
	private float m_currentRainAmount;

	private float m_currentWindAmount;

	[SerializeField]
	private float m_transitionSpeed;

	public UnityAction<float, float> m_onWeatherChanged;

	private WeatherSettings m_activeWeatherSettings;

	public float CurrentRainAmount => m_currentRainAmount;

	public float CurrentWindAmount => m_currentWindAmount;

	public WeatherSettings ActiveWeatherSettings => m_activeWeatherSettings;

	private float GetTargetRainAmount(WeatherSettings settings)
	{
		return Mathf.Clamp01(settings.BaseRainAmount + Mathf.Lerp(-1f, 1f, Mathf.PerlinNoise1D(Time.time * settings.RainVarianceFrequency)) * settings.RainVarianceAmount);
	}

	private float GetTargetWindAmount(WeatherSettings settings)
	{
		return Mathf.Clamp01(settings.BaseWindAmount + Mathf.Lerp(-1f, 1f, Mathf.PerlinNoise1D(Time.time * settings.WindVarianceFrequency)) * settings.WindVarianceAmount);
	}

	public void SetActiveWeatherSettings(WeatherSettings weatherSettings, bool forceInstant)
	{
		bool num = m_activeWeatherSettings == null || forceInstant;
		m_activeWeatherSettings = weatherSettings;
		if (num)
		{
			m_currentRainAmount = GetTargetRainAmount(weatherSettings);
			m_currentWindAmount = GetTargetWindAmount(weatherSettings);
		}
	}

	public void SetActiveWeatherSettings(WeatherSettings weatherSettings)
	{
		SetActiveWeatherSettings(weatherSettings, forceInstant: false);
	}

	public void Update()
	{
		float currentRainAmount = m_currentRainAmount;
		if (m_activeWeatherSettings != null)
		{
			m_currentRainAmount = Mathf.MoveTowards(m_currentRainAmount, GetTargetRainAmount(m_activeWeatherSettings), m_transitionSpeed * Time.deltaTime);
			m_currentWindAmount = Mathf.MoveTowards(m_currentWindAmount, GetTargetWindAmount(m_activeWeatherSettings), m_transitionSpeed * Time.deltaTime);
		}
		if (currentRainAmount != m_currentRainAmount)
		{
			WeatherChanged();
		}
	}

	public void WeatherChanged()
	{
		m_onWeatherChanged?.Invoke(m_currentRainAmount, m_currentWindAmount);
		Shader.SetGlobalFloat("_GlobalWindMultiplier", m_currentWindAmount);
	}

	public void DebugGUI()
	{
		GUILayout.Label("Current Rain Amount: " + CurrentRainAmount);
		GUILayout.Label("Current Wind Amount: " + CurrentWindAmount);
		if (m_activeWeatherSettings != null)
		{
			GUILayout.Label("Weather Settings: " + m_activeWeatherSettings.name);
		}
	}
}
