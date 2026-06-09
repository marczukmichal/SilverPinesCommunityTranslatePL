using System.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.Universal;

public class PhotoCameraController : MonoBehaviour
{
	[SerializeField]
	private Camera m_camera;

	private RenderTexture m_photoRenderTexture;

	private static bool m_takingPhoto;

	public static bool TakingPhoto => m_takingPhoto;

	private void OnEnable()
	{
		m_takingPhoto = false;
		GlobalReferences.Instance.EventChannels.Photos.TakePhoto.Register(TakePhoto);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.Photos.TakePhoto.Unregister(TakePhoto);
	}

	private void OnDestroy()
	{
		if (m_photoRenderTexture != null)
		{
			RenderTexture.ReleaseTemporary(m_photoRenderTexture);
			m_photoRenderTexture = null;
		}
	}

	public void TakePhoto()
	{
		StartCoroutine(TakePhotoCoroutine());
	}

	private void CreateNewRenderTextureIfNeeded()
	{
		if (m_photoRenderTexture != null && (m_photoRenderTexture.width != Screen.width || m_photoRenderTexture.height != Screen.height))
		{
			RenderTexture.ReleaseTemporary(m_photoRenderTexture);
			m_photoRenderTexture = null;
		}
		if (m_photoRenderTexture == null)
		{
			RenderTextureDescriptor desc = new RenderTextureDescriptor(Screen.width, Screen.height, GraphicsFormat.R8G8B8A8_UNorm, GraphicsFormat.D32_SFloat_S8_UInt);
			m_photoRenderTexture = RenderTexture.GetTemporary(desc);
		}
	}

	private IEnumerator TakePhotoCoroutine()
	{
		yield return new WaitForEndOfFrame();
		GlobalReferences.Instance.EventChannels.Photos.PrepareForPhoto.Raise(value: true);
		CreateNewRenderTextureIfNeeded();
		m_camera.targetTexture = m_photoRenderTexture;
		m_takingPhoto = true;
		UniversalRenderPipelineAsset obj = QualitySettings.renderPipeline as UniversalRenderPipelineAsset;
		float renderScale = obj.renderScale;
		obj.renderScale = 1f;
		m_camera.Render();
		obj.renderScale = renderScale;
		m_takingPhoto = false;
		m_camera.targetTexture = null;
		GlobalReferences.Instance.EventChannels.Photos.PrepareForPhoto.Raise(value: false);
		GlobalReferences.Instance.EventChannels.Photos.PhotoRenderTextureUpdated.Raise(m_photoRenderTexture);
	}
}
