using System.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class MinigamePhotoController : MonoBehaviour
{
	[SerializeField]
	private Image m_flashOverlay;

	[SerializeField]
	private Canvas m_minigameCanvas;

	[SerializeField]
	private TakingPhotoStateData m_photoStateData;

	[SerializeField]
	private float m_photoSize = 1f;

	[SerializeField]
	private Vector2 m_photoOffset = Vector2.zero;

	[SerializeField]
	private AudioEvent m_takePhotoAudio;

	private static bool m_takingMinigamePhoto;

	private RenderTexture m_photoRenderTexture;

	public static bool TakingMinigamePhoto => m_takingMinigamePhoto;

	private void Start()
	{
		m_flashOverlay.gameObject.SetActive(value: false);
	}

	public void TakePhotograph()
	{
		StartCoroutine(TakePhotographCoroutine());
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

	private IEnumerator TakePhotographCoroutine()
	{
		if (m_takePhotoAudio != null)
		{
			m_takePhotoAudio.Play2D();
		}
		m_flashOverlay.gameObject.SetActive(value: true);
		yield return new WaitForSeconds(0.04f);
		m_flashOverlay.gameObject.SetActive(value: false);
		Camera main = Camera.main;
		m_minigameCanvas.renderMode = RenderMode.ScreenSpaceCamera;
		m_minigameCanvas.worldCamera = main;
		m_minigameCanvas.planeDistance = 1f;
		UniversalAdditionalCameraData component = main.GetComponent<UniversalAdditionalCameraData>();
		component.renderPostProcessing = false;
		m_photoStateData.Reset();
		m_photoStateData.m_size = m_photoSize;
		m_photoStateData.m_offset = m_photoOffset;
		GlobalReferences.Instance.EventChannels.Photos.PrepareForPhoto.Raise(value: true);
		CreateNewRenderTextureIfNeeded();
		main.targetTexture = m_photoRenderTexture;
		m_takingMinigamePhoto = true;
		UniversalRenderPipelineAsset obj = QualitySettings.renderPipeline as UniversalRenderPipelineAsset;
		float renderScale = obj.renderScale;
		obj.renderScale = 1f;
		main.Render();
		obj.renderScale = renderScale;
		m_takingMinigamePhoto = false;
		main.targetTexture = null;
		GlobalReferences.Instance.EventChannels.Photos.PrepareForPhoto.Raise(value: false);
		GlobalReferences.Instance.EventChannels.Photos.PhotoRenderTextureUpdated.Raise(m_photoRenderTexture);
		component.renderPostProcessing = true;
		m_minigameCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
		m_minigameCanvas.worldCamera = null;
		m_minigameCanvas.enabled = false;
		m_minigameCanvas.enabled = true;
	}

	private void OnDrawGizmosSelected()
	{
		if (m_minigameCanvas != null)
		{
			RectTransform component = m_minigameCanvas.GetComponent<RectTransform>();
			Vector3 vector = new Vector3(component.rect.height, component.rect.height, 0f);
			vector *= m_photoSize;
			vector = component.TransformVector(vector);
			Vector3 center = (Vector3)m_photoOffset + component.transform.position;
			Gizmos.color = Color.magenta;
			Gizmos.DrawWireCube(center, vector);
			Gizmos.color = Color.white;
		}
	}
}
