using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Camera))]
public class CameraQualityListener : MonoBehaviour
{
	[SerializeField]
	private Camera m_camera;

	private void Reset()
	{
		m_camera = GetComponent<Camera>();
	}

	private void OnEnable()
	{
		UpdateCamera();
		GlobalReferences.Instance.EventChannels.UserPreferences.GraphicsQualityChanged.Register(UpdateCamera);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.UserPreferences.GraphicsQualityChanged.Unregister(UpdateCamera);
	}

	private void UpdateCamera()
	{
		AntiAliasingOption antiAliasing = GlobalReferences.Instance.UserPreferences.GraphicsQualitySettings.AntiAliasing;
		UniversalAdditionalCameraData universalAdditionalCameraData = m_camera.GetUniversalAdditionalCameraData();
		universalAdditionalCameraData.antialiasingQuality = AntialiasingQuality.High;
		switch (antiAliasing)
		{
		case AntiAliasingOption.Disabled:
			universalAdditionalCameraData.antialiasing = AntialiasingMode.None;
			break;
		case AntiAliasingOption.FXAA:
			universalAdditionalCameraData.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
			break;
		case AntiAliasingOption.TAA:
			universalAdditionalCameraData.antialiasing = AntialiasingMode.TemporalAntiAliasing;
			break;
		}
	}
}
