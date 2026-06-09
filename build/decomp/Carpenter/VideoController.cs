using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoController : MonoBehaviour
{
	[SerializeField]
	private float m_fadeOutTime;

	[SerializeField]
	private RawImage m_rawImage;

	[SerializeField]
	private VideoPlayer m_videoPlayer;

	[SerializeField]
	private GameObject m_edgeFadeContainer;

	[Header("Skipping")]
	[SerializeField]
	private Slider m_skipProgressSlider;

	[SerializeField]
	private float m_skipHoldTime = 1f;

	[SerializeField]
	private CanvasGroup m_skipCanvasGroup;

	private RenderTexture m_activeRenderTexture;

	private bool m_isSkipping;

	private float m_skipTimer;

	private bool m_hasSkipped;

	private EventInstance m_audioEventInstance;

	private VideoMetadata m_activeVideoMetadata;

	private ClipSubtitleEntry m_activeSubtitlesEntry;

	public UnityAction OnVideoFinished;

	public void StartVideoClip(VideoMetadata video)
	{
		m_activeSubtitlesEntry = null;
		m_activeVideoMetadata = video;
		m_activeRenderTexture = new RenderTexture((int)video.VideoClip.width, (int)video.VideoClip.height, 32);
		m_rawImage.texture = m_activeRenderTexture;
		m_skipCanvasGroup.alpha = 0f;
		m_videoPlayer.clip = video.VideoClip;
		m_videoPlayer.isLooping = false;
		m_videoPlayer.playOnAwake = false;
		m_videoPlayer.renderMode = VideoRenderMode.RenderTexture;
		m_videoPlayer.targetTexture = m_activeRenderTexture;
		m_videoPlayer.loopPointReached += OnVideoEnded;
		m_skipProgressSlider.gameObject.SetActive(value: false);
		m_videoPlayer.targetCameraAlpha = 0f;
		GlobalReferences.Instance.EventChannels.VideoCutscenes.PlayingVideoCutsceneChanged.Raise(value: true);
		GlobalReferences.Instance.EventChannels.Audio.PauseMusic.Raise(value: true);
		GlobalReferences.Instance.GameMenuState.SetInMenu(GameMenuState.GameMenu.VideoCutscene);
		float num = (float)video.VideoClip.width / (float)video.VideoClip.height;
		float num2 = (float)Screen.width / (float)Screen.height - num;
		m_edgeFadeContainer.SetActive(num2 > 0.1f);
		m_videoPlayer.prepareCompleted += OnVideoPlayerPrepared;
		m_videoPlayer.Prepare();
	}

	private void OnVideoPlayerPrepared(VideoPlayer source)
	{
		if (!m_activeVideoMetadata.FMODStartEvent.IsNull)
		{
			m_audioEventInstance = RuntimeManager.CreateInstance(m_activeVideoMetadata.FMODStartEvent);
			m_audioEventInstance.start();
		}
		m_videoPlayer.Play();
	}

	private void OnVideoEnded(VideoPlayer source)
	{
		EndVideo(onSkipping: false);
	}

	private void EndVideo(bool onSkipping)
	{
		if (m_audioEventInstance.isValid() && (m_activeVideoMetadata.StopFmodEventOnVideoEnd || onSkipping))
		{
			m_audioEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			m_audioEventInstance.release();
		}
		if (!m_activeVideoMetadata.FMODEndEvent.IsNull)
		{
			RuntimeManager.CreateInstance(m_activeVideoMetadata.FMODEndEvent).start();
		}
		OnVideoFinished?.Invoke();
		if (!m_activeVideoMetadata.FadeOut)
		{
			Color white = Color.white;
			white.a = 0f;
			m_rawImage.color = white;
			GlobalReferences.Instance.EventChannels.VideoCutscenes.PlayingVideoCutsceneChanged.Raise(value: false);
			Cleanup();
		}
		else
		{
			m_rawImage.DOFade(0f, m_fadeOutTime).SetUpdate(isIndependentUpdate: true).OnStart(delegate
			{
				GlobalReferences.Instance.EventChannels.VideoCutscenes.PlayingVideoCutsceneChanged.Raise(value: false);
			})
				.OnComplete(Cleanup);
		}
		GlobalReferences.Instance.EventChannels.Audio.ShowVoiceSubtitles.Raise(new SubtitlesEventData
		{
			m_enable = false
		});
	}

	private void Update()
	{
		bool flag = false;
		if (m_activeVideoMetadata.Skippable && (GameInputManager.GameInputActions.Game.Skip.IsPressed() || GameInputManager.GameInputActions.UI.Submit.IsPressed() || GameInputManager.GameInputActions.UI.Click.IsPressed()))
		{
			flag = true;
		}
		if (flag != m_isSkipping)
		{
			m_isSkipping = flag;
			m_skipCanvasGroup.alpha = (m_isSkipping ? 1f : 0f);
			m_skipProgressSlider.gameObject.SetActive(m_isSkipping);
			m_skipTimer = 0f;
			m_skipProgressSlider.value = 0f;
		}
		if (m_isSkipping && !m_hasSkipped)
		{
			m_skipTimer += Time.unscaledDeltaTime;
			m_skipProgressSlider.value = Mathf.Clamp01(m_skipTimer / m_skipHoldTime);
			if (m_skipTimer >= m_skipHoldTime)
			{
				SkipVideo();
			}
		}
		if (m_activeVideoMetadata.Subtitles == null)
		{
			return;
		}
		ClipSubtitleEntry entryForTimestamp = m_activeVideoMetadata.Subtitles.GetEntryForTimestamp((float)m_videoPlayer.time);
		if (m_activeSubtitlesEntry != entryForTimestamp)
		{
			if (entryForTimestamp != null)
			{
				GlobalReferences.Instance.EventChannels.Audio.ShowVoiceSubtitles.Raise(new SubtitlesEventData
				{
					m_enable = true,
					m_speakerSettings = entryForTimestamp.m_speaker,
					m_text = entryForTimestamp.SubtitlesText
				});
			}
			else
			{
				GlobalReferences.Instance.EventChannels.Audio.ShowVoiceSubtitles.Raise(new SubtitlesEventData
				{
					m_enable = false
				});
			}
		}
		m_activeSubtitlesEntry = entryForTimestamp;
	}

	private void SkipVideo()
	{
		m_hasSkipped = true;
		m_videoPlayer.Pause();
		EndVideo(onSkipping: true);
	}

	private void Cleanup()
	{
		Object.Destroy(base.gameObject);
	}

	private void OnDestroy()
	{
		DOTween.Kill(m_rawImage);
		Object.DestroyImmediate(m_activeRenderTexture);
		GlobalReferences.Instance.GameMenuState.ClearInMenu(GameMenuState.GameMenu.VideoCutscene);
		GlobalReferences.Instance.EventChannels.Audio.PauseMusic.Raise(value: false);
	}
}
