using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Video;

public class TelevisionInteractable : BaseInteractable
{
	[SerializeField]
	private MeshRenderer m_meshRenderer;

	[SerializeField]
	private Texture m_defaultImage;

	[SerializeField]
	private bool m_tvStartsOn;

	[Header("Video")]
	[SerializeField]
	private TelevisionClipMetadata m_clipMetadata;

	[SerializeField]
	private VideoPlayer m_videoPlayer;

	[SerializeField]
	private bool m_looping;

	[Header("Noise")]
	[SerializeField]
	private float m_channelNoiseAmount = 0.4f;

	[SerializeField]
	private float m_noiseFadeTime = 0.5f;

	[Header("Audio")]
	[SerializeField]
	private AudioEvent m_channelSwitchAudioEvent;

	[Header("Virtual Camera Trigger")]
	[SerializeField]
	private GameObject m_virtualCameraTrigger;

	[Header("Light")]
	[SerializeField]
	private GameObject m_light;

	[Header("Deprecated")]
	[SerializeField]
	private VideoClip m_videoClip;

	private RenderTexture m_videoRenderTexture;

	private bool m_isPlaying;

	private bool m_isHidden;

	private bool m_hasPlayed;

	private bool m_isPaused;

	private float m_previousPlayhead;

	private ClipSubtitleEntry m_activeSubtitlesEntry;

	private EventInstance m_fmodEventInstance;

	public override string InteractString => LocalizationSettings.StringDatabase.GetLocalizedString("InteractPrompts", "Use", null, FallbackBehavior.UseProjectSettings);

	protected override void Start()
	{
		base.Start();
		if (m_virtualCameraTrigger != null)
		{
			m_virtualCameraTrigger.SetActive(value: false);
		}
		if (m_meshRenderer != null)
		{
			m_meshRenderer.material.SetFloat("_OnState", m_tvStartsOn ? 1f : 0f);
		}
		if (m_light != null)
		{
			m_light.SetActive(m_tvStartsOn);
		}
	}

	public override void Interact(BaseInteractor interactor)
	{
		if (!m_isPlaying && !m_hasPlayed)
		{
			if (m_channelSwitchAudioEvent != null)
			{
				m_channelSwitchAudioEvent.Play(base.transform.position);
			}
			EnableTelevision();
		}
		else if (m_isHidden && m_isPlaying)
		{
			m_meshRenderer.material.SetTexture("_TVInputTexture", m_videoRenderTexture);
			m_isHidden = false;
			if (m_fmodEventInstance.isValid())
			{
				m_fmodEventInstance.setVolume(1f);
			}
		}
		else
		{
			m_meshRenderer.material.SetTexture("_TVInputTexture", m_defaultImage);
			m_isHidden = true;
			if (m_virtualCameraTrigger != null)
			{
				m_virtualCameraTrigger.SetActive(value: false);
			}
			if (m_fmodEventInstance.isValid())
			{
				m_fmodEventInstance.setVolume(0f);
			}
		}
		if (m_channelSwitchAudioEvent != null)
		{
			m_channelSwitchAudioEvent.Play(base.transform.position);
		}
	}

	public void EnableTelevision()
	{
		if (base.isActiveAndEnabled)
		{
			if (m_light != null)
			{
				m_light.SetActive(value: true);
			}
			if (m_clipMetadata != null)
			{
				PlayVideo(m_clipMetadata.VideoClip);
				return;
			}
			Debug.LogError("m_videoClip is Deperecated - Need to setup a TelevisionClipMetadata for clip!");
			PlayVideo(m_videoClip);
		}
	}

	private void PlayVideo(VideoClip videoClip)
	{
		if (m_videoRenderTexture != null)
		{
			Object.DestroyImmediate(m_videoRenderTexture);
		}
		m_videoRenderTexture = new RenderTexture((int)videoClip.width, (int)videoClip.height, 16);
		m_meshRenderer.material.DOFloat(1f, "_OnState", 1f);
		m_meshRenderer.material.SetTexture("_TVInputTexture", m_videoRenderTexture);
		m_meshRenderer.material.SetFloat("_NoiseAmount", 0.1f);
		m_meshRenderer.material.DOFloat(m_channelNoiseAmount, "_NoiseAmount", m_noiseFadeTime);
		m_videoPlayer.clip = videoClip;
		m_videoPlayer.renderMode = VideoRenderMode.RenderTexture;
		m_videoPlayer.targetTexture = m_videoRenderTexture;
		m_videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
		m_videoPlayer.timeUpdateMode = VideoTimeUpdateMode.UnscaledGameTime;
		m_videoPlayer.Play();
		if (m_clipMetadata != null && !m_clipMetadata.FMODEvent.IsNull)
		{
			m_fmodEventInstance = RuntimeManager.CreateInstance(m_clipMetadata.FMODEvent);
			if (m_fmodEventInstance.isValid())
			{
				m_fmodEventInstance.set3DAttributes(base.transform.position.To3DAttributes());
				m_fmodEventInstance.start();
			}
		}
		m_videoPlayer.loopPointReached += OnVideoFinished;
		m_isPlaying = true;
		m_hasPlayed = true;
	}

	private void OnVideoFinished(VideoPlayer source)
	{
		if (m_looping)
		{
			EnableTelevision();
			return;
		}
		DOTween.Kill(m_meshRenderer.material);
		m_meshRenderer.material.DOFloat(1f, "_NoiseAmount", m_noiseFadeTime);
		if (m_channelSwitchAudioEvent != null)
		{
			m_channelSwitchAudioEvent.Play(base.transform.position);
		}
		StopVideo();
	}

	public void StopVideo()
	{
		m_isPlaying = false;
		m_meshRenderer.material.SetTexture("_TVInputTexture", m_defaultImage);
		if (m_videoPlayer.isPlaying)
		{
			m_videoPlayer.Stop();
		}
		if (m_videoRenderTexture != null)
		{
			Object.DestroyImmediate(m_videoRenderTexture);
			m_videoRenderTexture = null;
		}
		if (m_activeSubtitlesEntry != null)
		{
			GlobalReferences.Instance.EventChannels.Audio.ShowVoiceSubtitles.Raise(new SubtitlesEventData
			{
				m_enable = false
			});
			m_activeSubtitlesEntry = null;
		}
		if (m_fmodEventInstance.isValid())
		{
			m_fmodEventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
		}
		if (m_virtualCameraTrigger != null)
		{
			m_virtualCameraTrigger.SetActive(value: false);
		}
	}

	private void Update()
	{
		if (!m_isPlaying)
		{
			return;
		}
		bool flag = Time.timeScale <= 0f;
		if (m_isPaused != flag)
		{
			m_isPaused = flag;
			if (m_fmodEventInstance.isValid())
			{
				m_fmodEventInstance.setPaused(m_isPaused);
			}
			if (m_isPaused)
			{
				m_videoPlayer.Pause();
			}
			else
			{
				m_videoPlayer.Play();
			}
		}
		float previousPlayhead = (float)m_videoPlayer.time;
		if (m_clipMetadata != null)
		{
			ClipSubtitleEntry entryForTimestamp = m_clipMetadata.Subtitles.GetEntryForTimestamp((float)m_videoPlayer.time);
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
		m_previousPlayhead = previousPlayhead;
	}

	private void OnDestroy()
	{
		if (m_isPlaying)
		{
			StopVideo();
		}
	}
}
