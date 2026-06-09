using UnityEngine;

public class LightShadowsQualityListener : MonoBehaviour
{
	private LightShadows m_startingShadowType;

	private Light m_light;

	private void Awake()
	{
		m_light = GetComponent<Light>();
		if (m_light != null)
		{
			m_startingShadowType = m_light.shadows;
			if (m_startingShadowType != 0)
			{
				GlobalReferences.Instance.EventChannels.UserPreferences.GraphicsQualityChanged.Register(OnGraphicsQualityChanged);
				OnGraphicsQualityChanged();
			}
		}
	}

	private void OnDestroy()
	{
		GlobalReferences.Instance.EventChannels.UserPreferences.GraphicsQualityChanged.Unregister(OnGraphicsQualityChanged);
	}

	private void OnGraphicsQualityChanged()
	{
		GraphicsQualityLevel lighting = GlobalReferences.Instance.UserPreferences.GraphicsQualitySettings.Lighting;
		LightShadows shadows = m_startingShadowType;
		switch (lighting)
		{
		case GraphicsQualityLevel.High:
			shadows = m_startingShadowType;
			break;
		case GraphicsQualityLevel.Medium:
			if (m_startingShadowType == LightShadows.Soft)
			{
				shadows = LightShadows.Hard;
			}
			break;
		case GraphicsQualityLevel.Low:
			shadows = LightShadows.None;
			break;
		}
		m_light.shadows = shadows;
	}
}
