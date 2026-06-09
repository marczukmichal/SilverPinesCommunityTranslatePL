using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TripSwitchesMinigameController : MonoBehaviour
{
	[SerializeField]
	private FuseTripSwitch[] m_switches;

	[SerializeField]
	private PlayMakerFSM m_powerSwitch;

	[SerializeField]
	private Image m_powerLightSprite;

	[SerializeField]
	private Color m_mainPowerOnColor;

	[SerializeField]
	private Color m_mainPowerOffColor;

	[SerializeField]
	private ProgressionVariable m_mainPowerProgressionVariable;

	[SerializeField]
	private ParticleSystem m_powerBreakParticleSystem;

	[SerializeField]
	private int m_maxAllowedSwitches;

	[SerializeField]
	private AudioVoicedEvent m_powerTrippedAudioEvent;

	[SerializeField]
	private AudioVoicedEvent m_mainPowerOffAudioEvent;

	[SerializeField]
	private AudioEvent m_powerTrippedSound;

	private bool m_playedTrippedAudioEvent;

	private bool m_playedPowerOffAudioEvent;

	[SerializeField]
	private UnityEvent m_onComplete;

	private void Start()
	{
		FuseTripSwitch[] switches = m_switches;
		foreach (FuseTripSwitch obj in switches)
		{
			obj.OnStateChanged = (UnityAction<bool>)Delegate.Combine(obj.OnStateChanged, new UnityAction<bool>(OnSwitchUpdated));
		}
		UpdatePowerSwitchState(sendSwitchEvent: false);
	}

	private void OnSwitchUpdated(bool arg0)
	{
		if (IsComplete())
		{
			m_onComplete.Invoke();
		}
	}

	public bool IsComplete()
	{
		FuseTripSwitch[] switches = m_switches;
		for (int i = 0; i < switches.Length; i++)
		{
			if (!switches[i].CurrentState)
			{
				return false;
			}
		}
		return true;
	}

	public void IncreaseMaxAllowedSwitches(int amount)
	{
		m_maxAllowedSwitches += amount;
	}

	public bool OnTripSwitchPressedValidate()
	{
		if (m_mainPowerProgressionVariable != null && !m_mainPowerProgressionVariable.Value)
		{
			if (m_mainPowerOffAudioEvent != null && !m_playedPowerOffAudioEvent)
			{
				m_playedPowerOffAudioEvent = true;
				GlobalReferences.Instance.EventChannels.Audio.PlayAudioVoiced.Raise(new PlayAudioVoicedEventData(m_mainPowerOffAudioEvent, null, isPlayer: true));
			}
			return false;
		}
		int num = 0;
		FuseTripSwitch[] switches = m_switches;
		for (int i = 0; i < switches.Length; i++)
		{
			if (switches[i].IsTripSwitchActive())
			{
				num++;
			}
		}
		if (num >= m_maxAllowedSwitches)
		{
			if (m_powerTrippedAudioEvent != null && !m_playedTrippedAudioEvent)
			{
				m_playedTrippedAudioEvent = true;
				GlobalReferences.Instance.EventChannels.Audio.PlayAudioVoiced.Raise(new PlayAudioVoicedEventData(m_powerTrippedAudioEvent, null, isPlayer: true));
			}
			m_powerTrippedSound?.Play2D();
			if (m_powerBreakParticleSystem != null)
			{
				m_powerBreakParticleSystem.Play();
			}
			TripSwitches();
			return false;
		}
		return true;
	}

	private void TripSwitches()
	{
		if (m_mainPowerProgressionVariable != null)
		{
			m_mainPowerProgressionVariable.Value = false;
		}
		UpdatePowerSwitchState(sendSwitchEvent: true);
		FuseTripSwitch[] switches = m_switches;
		for (int i = 0; i < switches.Length; i++)
		{
			switches[i].TripIfActive();
		}
	}

	private void UpdatePowerSwitchState(bool sendSwitchEvent)
	{
		if (!(m_mainPowerProgressionVariable == null))
		{
			bool value = m_mainPowerProgressionVariable.Value;
			if (sendSwitchEvent && m_powerSwitch != null)
			{
				m_powerSwitch.SendEvent(value ? "Minigame/Power/On" : "Minigame/Power/Off");
			}
			if (m_powerLightSprite != null)
			{
				m_powerLightSprite.color = (value ? m_mainPowerOnColor : m_mainPowerOffColor);
			}
		}
	}

	public void MainPowerBreakerSwitch()
	{
		if (!(m_mainPowerProgressionVariable == null))
		{
			m_mainPowerProgressionVariable.Value = !m_mainPowerProgressionVariable.Value;
			UpdatePowerSwitchState(sendSwitchEvent: true);
			if (!m_mainPowerProgressionVariable.Value)
			{
				TripSwitches();
			}
		}
	}
}
