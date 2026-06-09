using UnityEngine;
using UnityEngine.Events;

public class LightBeamSensor : MonoBehaviour
{
	[SerializeField]
	public UnityEvent m_lightDetected;

	[SerializeField]
	public UnityEvent m_lightLost;

	private DetectableLightBeam m_detectedLightBeam;

	public DetectableLightBeam DetectedLightBeam => m_detectedLightBeam;

	public void SetDetectedLightBeam(DetectableLightBeam lightBeam)
	{
		if (m_detectedLightBeam != lightBeam)
		{
			m_detectedLightBeam = lightBeam;
			if (m_detectedLightBeam != null)
			{
				m_lightDetected?.Invoke();
			}
			else
			{
				m_lightLost?.Invoke();
			}
		}
	}
}
