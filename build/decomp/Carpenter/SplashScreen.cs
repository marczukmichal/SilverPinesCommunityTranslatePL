using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class SplashScreen : MonoBehaviour
{
	[SerializeField]
	private RawImage m_videoRawImage;

	[SerializeField]
	private VideoPlayer m_videoPlayer;

	[SerializeField]
	private VideoMetadata m_videoMetadata;

	[SerializeField]
	private Image m_blackOverlay;

	[SerializeField]
	private CanvasGroup m_canvasGroup;

	[SerializeField]
	private bool m_playVideo;

	private RenderTexture m_videoRenderTexture;

	private bool m_done;

	private float m_startTime;

	private EventInstance m_audioEventInstance;

	private void Start()
	{
		m_startTime = Time.unscaledTime;
		if (m_playVideo)
		{
			m_videoPlayer.prepareCompleted += OnVideoPlayerPrepared;
			m_videoRenderTexture = new RenderTexture((int)m_videoMetadata.VideoClip.width, (int)m_videoMetadata.VideoClip.height, 16);
			m_videoRawImage.texture = m_videoRenderTexture;
			m_videoPlayer.clip = m_videoMetadata.VideoClip;
			m_videoPlayer.isLooping = false;
			m_videoPlayer.playOnAwake = false;
			m_videoPlayer.renderMode = VideoRenderMode.RenderTexture;
			m_videoPlayer.targetTexture = m_videoRenderTexture;
			m_videoPlayer.Play();
			m_videoPlayer.loopPointReached += OnVideoEnded;
			m_blackOverlay.DOFade(0f, 1f).SetDelay(0.5f);
		}
		else
		{
			Proceed();
		}
	}

	private void OnVideoPlayerPrepared(VideoPlayer source)
	{
		if (!m_videoMetadata.FMODStartEvent.IsNull)
		{
			m_audioEventInstance = RuntimeManager.CreateInstance(m_videoMetadata.FMODStartEvent);
			m_audioEventInstance.start();
		}
		m_videoPlayer.Play();
	}

	private void OnEnable()
	{
		GameInputManager.GameInputActions.Game.Proceed.performed += ProgressInputPressed;
	}

	private void OnDestroy()
	{
		if (GameInputManager.GameInputActions != null)
		{
			GameInputManager.GameInputActions.Game.Proceed.performed -= ProgressInputPressed;
		}
	}

	private void ProgressInputPressed(InputAction.CallbackContext callback)
	{
		if (!(Time.unscaledTime - m_startTime < 0.5f) && callback.performed)
		{
			Proceed();
		}
	}

	private void FadeOutComplete()
	{
		SceneManager.UnloadSceneAsync(base.gameObject.scene);
	}

	private void OnVideoEnded(VideoPlayer source)
	{
		Proceed();
	}

	private void StopAudio()
	{
		if (m_audioEventInstance.isValid() && m_videoMetadata.StopFmodEventOnVideoEnd)
		{
			m_audioEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			m_audioEventInstance.release();
		}
		if (!m_videoMetadata.FMODEndEvent.IsNull)
		{
			RuntimeManager.CreateInstance(m_videoMetadata.FMODEndEvent).start();
		}
	}

	private void Proceed()
	{
		if (!m_done)
		{
			StopAudio();
			GlobalReferences.Instance.EventChannels.LevelTransition.ShowLoadingScreen.Raise(value: false);
			m_done = true;
			AsyncOperationHandle asyncOperationHandle = Addressables.LoadSceneAsync("MenuScenes/SystemMenu", LoadSceneMode.Additive);
			asyncOperationHandle.Completed += delegate
			{
				m_canvasGroup.DOFade(0f, 0.5f).OnComplete(FadeOutComplete);
			};
		}
	}
}
