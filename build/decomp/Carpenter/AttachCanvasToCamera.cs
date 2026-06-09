using UnityEngine;

public class AttachCanvasToCamera : MonoBehaviour
{
	[SerializeField]
	private CameraAnchor m_camera;

	private void OnEnable()
	{
		if (m_camera.Item != null)
		{
			AttachToCamera(m_camera.Item);
		}
		m_camera.Register(AttachToCamera);
	}

	private void OnDisable()
	{
		m_camera.Unregister(AttachToCamera);
	}

	private void AttachToCamera(Camera camera)
	{
		GetComponent<Canvas>().worldCamera = camera;
	}
}
