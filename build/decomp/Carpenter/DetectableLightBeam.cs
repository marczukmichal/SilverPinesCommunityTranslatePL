using UnityEngine;

public class DetectableLightBeam : MonoBehaviour
{
	[SerializeField]
	private Transform m_root;

	public Vector3 RootPosition => m_root.position;

	public void OnTriggerEnter2D(Collider2D collision)
	{
		LightBeamSensor componentInParent = collision.GetComponentInParent<LightBeamSensor>();
		if (componentInParent != null)
		{
			componentInParent.SetDetectedLightBeam(this);
		}
	}

	public void OnTriggerExit2D(Collider2D collision)
	{
		LightBeamSensor componentInParent = collision.GetComponentInParent<LightBeamSensor>();
		if (componentInParent != null)
		{
			componentInParent.SetDetectedLightBeam(null);
		}
	}
}
