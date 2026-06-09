using Cinemachine;
using UnityEngine;

public class BoatCamera : MonoBehaviour
{
	[SerializeField]
	private CinemachineVirtualCamera m_virtualCamera;

	[SerializeField]
	private float m_zDepth;

	[SerializeField]
	private float m_heightOffset;

	private void FixedUpdate()
	{
		if (!(Camera.main == null))
		{
			CameraTargetData cameraTargetData = GlobalReferences.Instance.CameraTargetData;
			if (cameraTargetData.m_active && m_virtualCamera.enabled)
			{
				Vector3 position = cameraTargetData.m_position;
				position.y += m_heightOffset;
				position.z = m_zDepth;
				base.transform.position = position;
			}
		}
	}
}
