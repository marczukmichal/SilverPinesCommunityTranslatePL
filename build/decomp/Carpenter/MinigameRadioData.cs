using System;
using System.Collections;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class MinigameRadioData : MonoBehaviour, IPersistentComponent
{
	[Serializable]
	private class RadioBroadcastChannel
	{
		[SerializeField]
		public RadioBroadcastSettings m_settings;

		[SerializeField]
		public RadioBroadcastSubtitles m_subtitles;
	}

	[Serializable]
	private class PersistentData
	{
		public float m_frquencyValue;

		public MinigameRadio.RadioMode m_radioMode;
	}

	[SerializeField]
	private float m_startingFrequencyValue;

	[SerializeField]
	private MinigameRadio.RadioMode m_startingMode;

	private float m_frequencyValue;

	private MinigameRadio.RadioMode m_radioMode;

	[SerializeField]
	private RadioBroadcastChannel[] m_channels;

	[SerializeField]
	private AnimationCurve m_fmodParameterDistanceCurve;

	[SerializeField]
	private EventReference m_FMODEvent;

	[SerializeField]
	private float m_subtitlesDistance = 10f;

	private RadioBroadcastChannel m_activeChannel;

	private EventInstance m_fmodEventInstance;

	private ClipSubtitleEntry m_activeSubtitlesEntry;

	private PersistentDataObject m_persistentDataObject;

	private PersistentData m_persistentData;

	public float FrequencyValue
	{
		get
		{
			return m_frequencyValue;
		}
		set
		{
			m_frequencyValue = value;
			if (m_persistentData != null)
			{
				m_persistentData.m_frquencyValue = value;
			}
			UpdateFMODEvent();
		}
	}

	public MinigameRadio.RadioMode RadioMode
	{
		get
		{
			return m_radioMode;
		}
		set
		{
			m_radioMode = value;
			if (m_persistentData != null)
			{
				m_persistentData.m_radioMode = value;
			}
			UpdateFMODEvent();
		}
	}

	public bool TryBroadcast()
	{
		RadioBroadcastChannel[] channels = m_channels;
		foreach (RadioBroadcastChannel radioBroadcastChannel in channels)
		{
			if (radioBroadcastChannel.m_settings.m_mode == m_radioMode && Mathf.Abs(radioBroadcastChannel.m_settings.m_frequencyValue - m_frequencyValue) < radioBroadcastChannel.m_settings.m_validRange)
			{
				return true;
			}
		}
		return false;
	}

	private void OnEnable()
	{
		if (!m_FMODEvent.IsNull)
		{
			m_fmodEventInstance = RuntimeManager.CreateInstance(m_FMODEvent);
			m_fmodEventInstance.set3DAttributes(base.transform.position.To3DAttributes());
			m_fmodEventInstance.start();
		}
		if (m_persistentData == null || m_frequencyValue <= 0f)
		{
			m_frequencyValue = m_startingFrequencyValue;
			m_radioMode = m_startingMode;
		}
		UpdateFMODEvent();
	}

	private void OnDisable()
	{
		if (m_fmodEventInstance.isValid())
		{
			m_fmodEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			m_fmodEventInstance.clearHandle();
		}
		if (m_activeSubtitlesEntry != null)
		{
			GlobalReferences.Instance.EventChannels.Audio.ShowVoiceSubtitles.Raise(new SubtitlesEventData
			{
				m_enable = false
			});
			m_activeSubtitlesEntry = null;
		}
	}

	private void UpdateFMODEvent()
	{
		RadioBroadcastChannel activeChannel = null;
		RadioBroadcastChannel[] channels = m_channels;
		foreach (RadioBroadcastChannel radioBroadcastChannel in channels)
		{
			float value = 0f;
			if (radioBroadcastChannel.m_settings.m_mode == m_radioMode)
			{
				float num = Mathf.Abs(radioBroadcastChannel.m_settings.m_frequencyValue - m_frequencyValue);
				if (num < radioBroadcastChannel.m_settings.m_fmodFrequencyRange)
				{
					value = m_fmodParameterDistanceCurve.Evaluate(1f - num / radioBroadcastChannel.m_settings.m_fmodFrequencyRange);
					activeChannel = radioBroadcastChannel;
				}
			}
			if (m_fmodEventInstance.isValid())
			{
				m_fmodEventInstance.setParameterByName(radioBroadcastChannel.m_settings.m_fmodParameterName, value);
			}
		}
		m_activeChannel = activeChannel;
	}

	public void SetActiveChannel(int index)
	{
		ResetTimelinePosition();
		if (m_channels.Length >= index)
		{
			RadioMode = m_channels[index].m_settings.m_mode;
			StartCoroutine(SwitchToFrequency(m_channels[index].m_settings.m_frequencyValue));
		}
	}

	private IEnumerator SwitchToFrequency(float targetFrequency)
	{
		float tuningSpeed = 10f;
		while (FrequencyValue != targetFrequency)
		{
			FrequencyValue = Mathf.MoveTowards(FrequencyValue, targetFrequency, Time.deltaTime * tuningSpeed);
			yield return new WaitForEndOfFrame();
		}
	}

	public void ResetTimelinePosition()
	{
		Debug.Log(m_fmodEventInstance.setTimelinePosition(0));
	}

	private void Update()
	{
		if (m_activeChannel == null || !(m_activeChannel.m_subtitles != null))
		{
			return;
		}
		ClipSubtitleEntry clipSubtitleEntry = null;
		float num = 0f;
		GameObject item = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item;
		if (item != null)
		{
			num = Vector2.Distance(base.transform.position, item.transform.position);
		}
		if (num < m_subtitlesDistance)
		{
			m_fmodEventInstance.getTimelinePosition(out var position);
			double totalSeconds = TimeSpan.FromMilliseconds(position).TotalSeconds;
			clipSubtitleEntry = m_activeChannel.m_subtitles.Subtitles.GetEntryForTimestamp((float)totalSeconds);
		}
		if (m_activeSubtitlesEntry != clipSubtitleEntry)
		{
			if (clipSubtitleEntry != null)
			{
				GlobalReferences.Instance.EventChannels.Audio.ShowVoiceSubtitles.Raise(new SubtitlesEventData
				{
					m_enable = true,
					m_speakerSettings = clipSubtitleEntry.m_speaker,
					m_text = clipSubtitleEntry.SubtitlesText
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
		m_activeSubtitlesEntry = clipSubtitleEntry;
	}

	public void ReceiveDataStoreEntry(PersistentDataObject dataEntry)
	{
		m_persistentDataObject = dataEntry;
		m_persistentData = ((m_persistentDataObject.Data != null) ? (m_persistentDataObject.Data as PersistentData) : null);
		if (m_persistentData != null)
		{
			m_frequencyValue = m_persistentData.m_frquencyValue;
			m_radioMode = m_persistentData.m_radioMode;
			UpdateFMODEvent();
		}
		else
		{
			m_persistentData = new PersistentData();
			m_persistentDataObject.Data = m_persistentData;
		}
	}

	public bool RequiresPersistentData()
	{
		return true;
	}
}
