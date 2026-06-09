using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class UIVideo : MonoBehaviour
{
	[Header("UI")]
	[SerializeField]
	private RawImage m_targetImage;

	[SerializeField]
	private Texture2D m_firstFrameTexture;

	[Header("Video Source")]
	[SerializeField]
	private VideoClip m_videoClip;

	private VideoPlayer m_videoPlayer;

	private RenderTexture m_temporaryRT;

	private bool m_renderTextureReady;

	private bool ShouldPlay => true;

	private void Awake()
	{
		m_targetImage.texture = m_firstFrameTexture;
		if (ShouldPlay)
		{
			SetupVideoPlayer();
		}
		else if (m_targetImage.texture == null)
		{
			m_targetImage.enabled = false;
		}
	}

	private IEnumerator Start()
	{
		yield return new WaitForEndOfFrame();
		if (m_videoPlayer != null)
		{
			m_videoPlayer.prepareCompleted += OnVideoPrepared;
			m_videoPlayer.Prepare();
		}
	}

	private void SetupVideoPlayer()
	{
		m_videoPlayer = base.gameObject.AddComponent<VideoPlayer>();
		m_videoPlayer.waitForFirstFrame = true;
		m_videoPlayer.isLooping = true;
		m_videoPlayer.skipOnDrop = true;
		m_videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
		m_videoPlayer.renderMode = VideoRenderMode.RenderTexture;
		m_videoPlayer.playOnAwake = false;
		if (m_videoClip != null)
		{
			m_videoPlayer.source = VideoSource.VideoClip;
			m_videoPlayer.clip = m_videoClip;
		}
	}

	private void OnVideoPrepared(VideoPlayer vp)
	{
		vp.prepareCompleted -= OnVideoPrepared;
		CreateRenderTextureFromVideo();
		m_renderTextureReady = true;
		vp.Play();
	}

	private void CreateRenderTextureFromVideo()
	{
		CleanupRenderTexture();
		int width = Mathf.Max(1, (int)m_videoPlayer.width);
		int height = Mathf.Max(1, (int)m_videoPlayer.height);
		m_temporaryRT = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32)
		{
			name = "TempVideoRT_" + base.gameObject.name,
			useMipMap = false,
			autoGenerateMips = false
		};
		m_temporaryRT.Create();
		m_videoPlayer.targetTexture = m_temporaryRT;
		if (m_targetImage != null)
		{
			m_targetImage.enabled = true;
			m_targetImage.texture = m_temporaryRT;
		}
	}

	public void Play()
	{
		if (!(m_videoPlayer != null) || m_renderTextureReady)
		{
			m_videoPlayer.Play();
		}
	}

	public void Pause()
	{
		if (m_videoPlayer != null && m_videoPlayer.isPlaying)
		{
			m_videoPlayer.Pause();
		}
	}

	public void Stop()
	{
		if (m_videoPlayer != null)
		{
			m_videoPlayer.Stop();
		}
	}

	private void CleanupRenderTexture()
	{
		if (!(m_temporaryRT == null))
		{
			if (m_targetImage != null && m_targetImage.texture == m_temporaryRT)
			{
				m_targetImage.texture = null;
			}
			if (m_videoPlayer != null && m_videoPlayer.targetTexture == m_temporaryRT)
			{
				m_videoPlayer.targetTexture = null;
			}
			m_temporaryRT.Release();
			Object.Destroy(m_temporaryRT);
			m_temporaryRT = null;
		}
	}

	private void OnDisable()
	{
		Stop();
	}

	private void OnDestroy()
	{
		CleanupRenderTexture();
	}
}
