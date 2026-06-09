using UnityEngine;
using UnityEngine.Rendering.Universal;

public class AttachToMainCameraCameraStack : MonoBehaviour
{
	[SerializeField]
	private Camera m_canvasCamera;

	private void OnEnable()
	{
		if (Camera.main != null)
		{
			AttachToCamera(Camera.main);
		}
		GlobalReferences.Instance.EventChannels.Camera.NewCamera.Register(AttachToCamera);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.Camera.NewCamera.Unregister(AttachToCamera);
	}

	private void AttachToCamera(Camera camera)
	{
		UniversalAdditionalCameraData universalAdditionalCameraData = camera.GetUniversalAdditionalCameraData();
		if (!universalAdditionalCameraData.cameraStack.Contains(m_canvasCamera))
		{
			universalAdditionalCameraData.cameraStack.Add(m_canvasCamera);
		}
	}
}
