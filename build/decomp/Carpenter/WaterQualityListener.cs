using UnityEngine;
using UnityEngine.Rendering;

public class WaterQualityListener : MonoBehaviour
{
	private MeshRenderer m_meshRenderer;

	private LocalKeyword m_localKeyword;

	private void Awake()
	{
		m_meshRenderer = GetComponent<MeshRenderer>();
		m_localKeyword = new LocalKeyword(m_meshRenderer.material.shader, "_SCREENSPACE_REFLECTIONS");
		GlobalReferences.Instance.EventChannels.UserPreferences.GraphicsQualityChanged.Register(OnGraphicsQualityChanged);
		OnGraphicsQualityChanged();
	}

	private void OnDestroy()
	{
		GlobalReferences.Instance.EventChannels.UserPreferences.GraphicsQualityChanged.Unregister(OnGraphicsQualityChanged);
	}

	private void OnGraphicsQualityChanged()
	{
		switch (GlobalReferences.Instance.UserPreferences.GraphicsQualitySettings.Water)
		{
		case GraphicsQualityLevel.High:
			m_meshRenderer.material.SetKeyword(in m_localKeyword, value: true);
			break;
		case GraphicsQualityLevel.Medium:
			m_meshRenderer.material.SetKeyword(in m_localKeyword, value: true);
			break;
		case GraphicsQualityLevel.Low:
			m_meshRenderer.material.SetKeyword(in m_localKeyword, value: false);
			break;
		}
	}
}
