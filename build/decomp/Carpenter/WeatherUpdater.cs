using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class WeatherUpdater : MonoBehaviour
{
	[SerializeField]
	private Weather m_weather;

	[DebugCommand("weather_debug", "Enable weather debug info overlay", "weather_debug <true/false>", typeof(bool), false)]
	private static bool m_weatherDebug;

	private void OnGUI()
	{
		if (m_weatherDebug)
		{
			m_weather.DebugGUI();
		}
	}

	private void Start()
	{
		m_weather.WeatherChanged();
	}

	private void Update()
	{
		m_weather.Update();
	}

	[DebugCommand("weather", "Sets the current weather settings for the game", "weather [settings_name]", typeof(string), false)]
	public void SetWeather(string weatherName)
	{
		WeatherSettings weatherSettings = (WeatherSettings)((AsyncOperationHandle)Addressables.LoadAssetAsync<WeatherSettings>("Assets/ScriptableObjects/Settings/Weather/" + weatherName + ".asset")).WaitForCompletion();
		if (weatherSettings != null)
		{
			m_weather.SetActiveWeatherSettings(weatherSettings);
		}
	}
}
