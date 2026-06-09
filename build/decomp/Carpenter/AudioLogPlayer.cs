using System;
using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
using TMPro;
using UnityEngine;

public class AudioLogPlayer : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI m_audioLogNameText;

	[SerializeField]
	private TextMeshProUGUI m_playbackTimeText;

	[SerializeField]
	private RectTransform m_rectTransform;

	[Header("Audio Events")]
	[SerializeField]
	private AudioEvent m_startPlayingAudioLogEvent;

	[SerializeField]
	private AudioEvent m_stopPlayingAudioLogEvent;

	private EventInstance m_instance;

	private int m_eventLength;

	private bool m_isPlaying;

	public bool IsPlaying => m_isPlaying;

	private void OnEnable()
	{
		m_rectTransform.pivot = new Vector2(0.5f, 1f);
		GlobalReferences.Instance.Anchors.Lore.AudioLogPlayerAnchor.Set(this);
		GlobalReferences.Instance.EventChannels.Lore.LorePickup.Register(OnFindLore);
		GlobalReferences.Instance.EventChannels.Lore.PlayLoreEntryAsAudioLog.Register(OnPlayAudioLog);
		GlobalReferences.Instance.EventChannels.Lore.StopPlayingAudioLog.Register(StopPlaying);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.Anchors.Lore.AudioLogPlayerAnchor.Set(null);
		GlobalReferences.Instance.EventChannels.Lore.LorePickup.Unregister(OnFindLore);
		GlobalReferences.Instance.EventChannels.Lore.PlayLoreEntryAsAudioLog.Unregister(OnPlayAudioLog);
		GlobalReferences.Instance.EventChannels.Lore.StopPlayingAudioLog.Unregister(StopPlaying);
	}

	private void OnFindLore(LoreEntry loreEntry)
	{
		if (loreEntry.IsAudioLog)
		{
			OnPlayAudioLog(loreEntry);
		}
	}

	private void OnPlayAudioLog(LoreEntry loreEntry)
	{
		m_audioLogNameText.text = loreEntry.Title;
		DOTween.Kill(m_rectTransform);
		m_rectTransform.DOPivotY(0f, 0.5f).SetUpdate(isIndependentUpdate: true);
		if (m_instance.isValid())
		{
			m_instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		}
		m_instance = RuntimeManager.CreateInstance(loreEntry.AudioLog.FMODEvent);
		m_instance.start();
		m_instance.getDescription(out var description);
		description.getLength(out m_eventLength);
		if (m_eventLength == 0)
		{
			Debug.LogWarning("Audio Event for " + loreEntry.name + " reported event length of 0. Missing Timeline Parameter?");
		}
		m_isPlaying = true;
		if (m_startPlayingAudioLogEvent != null)
		{
			m_startPlayingAudioLogEvent.Play2D();
		}
		GlobalReferences.Instance.EventChannels.Lore.AudioLogPlaybackStatusChanged.Raise(value: true);
	}

	private void Update()
	{
		if (m_isPlaying)
		{
			if (!m_instance.isValid())
			{
				StopPlaying();
			}
			m_instance.getTimelinePosition(out var position);
			TimeSpan timeSpan = TimeSpan.FromMilliseconds(position);
			TimeSpan timeSpan2 = TimeSpan.FromMilliseconds(m_eventLength);
			m_playbackTimeText.text = $"{timeSpan:mm':'ss}<color=grey><size=14>/{timeSpan2:mm':'ss}</size></color>";
			m_instance.getPlaybackState(out var state);
			if (state == PLAYBACK_STATE.STOPPED)
			{
				StopPlaying();
			}
		}
	}

	private void StopPlaying()
	{
		DOTween.Kill(m_rectTransform);
		m_isPlaying = false;
		m_rectTransform.DOPivotY(1f, 0.5f).SetUpdate(isIndependentUpdate: true);
		if (m_instance.isValid())
		{
			m_instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			m_instance.release();
		}
		if (m_stopPlayingAudioLogEvent != null)
		{
			m_stopPlayingAudioLogEvent.Play2D();
		}
		GlobalReferences.Instance.EventChannels.Lore.AudioLogPlaybackStatusChanged.Raise(value: false);
	}
}
