using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ApartmentBuzzer : MonoBehaviour
{
	private enum BuzzerStep
	{
		WaitForFloor,
		WaitForLetter,
		WaitForDialog
	}

	[SerializeField]
	private ProgressionVariable m_callUnblockedProgressionVariable;

	[Header("Audio Events")]
	[SerializeField]
	private AudioEvent m_validButtonAudio;

	[SerializeField]
	private AudioEvent m_invalidButtonAudio;

	[Header("Voice Lines")]
	[SerializeField]
	private AudioVoicedEvent m_noResponseVoiceEvent;

	[SerializeField]
	private AudioVoicedEvent m_successVoiceEvent;

	[SerializeField]
	private AudioVoicedEvent m_intercomChannelBlockedVoiceEvent;

	[Header("Logic")]
	[SerializeField]
	private string m_correctApartmentNumber = "4B";

	[Header("Visuals")]
	[SerializeField]
	private Image m_callLightSprite;

	[SerializeField]
	private Image m_callLightGlow;

	[SerializeField]
	private Color m_callLightOffColor;

	[SerializeField]
	private Color m_callLightOnColor;

	[Header("Text")]
	[SerializeField]
	private TextMeshProUGUI m_text;

	[Header("Unity Events")]
	[SerializeField]
	private UnityEvent m_correctNumberCalled;

	private bool m_playedBlockedVoiceEvent;

	private BuzzerStep m_currentBuzzerStep;

	private string m_currentInput = "";

	private bool m_finishedVoice;

	private string CurrentInput
	{
		get
		{
			return m_currentInput;
		}
		set
		{
			m_currentInput = value;
			m_text.text = value;
		}
	}

	private void Start()
	{
		if (m_callUnblockedProgressionVariable.Value)
		{
			SetCallLight(enabled: false);
			m_text.text = "";
		}
		else
		{
			SetCallLight(enabled: true);
		}
	}

	public void PressedButton(string input)
	{
		if (!m_callUnblockedProgressionVariable.Value)
		{
			if (!m_playedBlockedVoiceEvent)
			{
				m_playedBlockedVoiceEvent = true;
				GlobalReferences.Instance.EventChannels.Audio.PlayAudioVoiced.Raise(new PlayAudioVoicedEventData(m_intercomChannelBlockedVoiceEvent, null, isPlayer: false));
			}
			PlayInvalidButtonAudio();
			return;
		}
		if (m_currentBuzzerStep == BuzzerStep.WaitForDialog)
		{
			CurrentInput = "";
			m_currentBuzzerStep = BuzzerStep.WaitForFloor;
		}
		if (m_currentBuzzerStep == BuzzerStep.WaitForFloor)
		{
			if (char.IsDigit(input[0]))
			{
				CurrentInput += input;
				m_currentBuzzerStep = BuzzerStep.WaitForLetter;
				PlayValidButtonAudio();
			}
			else
			{
				PlayInvalidButtonAudio();
			}
		}
		else if (m_currentBuzzerStep == BuzzerStep.WaitForLetter)
		{
			if (char.IsLetter(input[0]))
			{
				CurrentInput += input;
				m_currentBuzzerStep = BuzzerStep.WaitForDialog;
			}
			else
			{
				CurrentInput = input ?? "";
			}
			PlayValidButtonAudio();
		}
	}

	private void PlayValidButtonAudio()
	{
		if (!(m_validButtonAudio == null))
		{
			AudioManager.PlayAudioEvent(m_validButtonAudio, Vector3.zero, 1f, useOcclusion: false);
		}
	}

	private void PlayInvalidButtonAudio()
	{
		if (!(m_invalidButtonAudio == null))
		{
			AudioManager.PlayAudioEvent(m_invalidButtonAudio, Vector3.zero, 1f, useOcclusion: false);
		}
	}

	private void SetCallLight(bool enabled)
	{
		m_callLightSprite.color = (enabled ? m_callLightOnColor : m_callLightOffColor);
		m_callLightGlow.gameObject.SetActive(enabled);
	}

	private IEnumerator TriggerCall()
	{
		bool hasResponse = m_currentInput.Equals(m_correctApartmentNumber);
		m_finishedVoice = false;
		AudioVoicedEvent voiceLineEvent = (hasResponse ? m_successVoiceEvent : m_noResponseVoiceEvent);
		GlobalReferences.Instance.EventChannels.Audio.PlayAudioVoiced.Raise(new PlayAudioVoicedEventData(voiceLineEvent, null, isPlayer: false));
		GlobalReferences.Instance.EventChannels.Audio.OnAudioVoicedEventFinished.Register(OnVoiceEventDone);
		SetCallLight(enabled: true);
		yield return new WaitForSecondsRealtime(1f);
		if (hasResponse)
		{
			m_correctNumberCalled.Invoke();
		}
		CurrentInput = "";
		m_currentBuzzerStep = BuzzerStep.WaitForFloor;
		SetCallLight(enabled: false);
	}

	public void CallButtonPressed()
	{
		if (CurrentInput.Length == 2)
		{
			StartCoroutine(TriggerCall());
		}
		else
		{
			PlayInvalidButtonAudio();
		}
	}

	private void OnVoiceEventDone()
	{
		m_finishedVoice = true;
		GlobalReferences.Instance.EventChannels.Audio.OnAudioVoicedEventFinished.Unregister(OnVoiceEventDone);
	}

	private void OnDestroy()
	{
		GlobalReferences.Instance.EventChannels.Audio.OnAudioVoicedEventFinished.Unregister(OnVoiceEventDone);
	}

	private bool IsFinishedVoiceLines()
	{
		return m_finishedVoice;
	}
}
