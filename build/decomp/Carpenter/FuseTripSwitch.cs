using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FuseTripSwitch : MonoBehaviour, IPersistentComponent
{
	[Serializable]
	private class PersistentData
	{
		public bool m_set;
	}

	[SerializeField]
	private Image m_image;

	[SerializeField]
	private TripSwitchesMinigameController m_controller;

	[SerializeField]
	private bool m_state;

	[SerializeField]
	private ProgressionVariable m_startingStateVariable;

	[SerializeField]
	private Sprite m_onSprite;

	[SerializeField]
	private Sprite m_offSprite;

	[SerializeField]
	private AudioEvent m_turnOnAudioEvent;

	[SerializeField]
	private AudioEvent m_turnOffAudioEvent;

	public UnityAction<bool> OnStateChanged;

	private PersistentDataObject m_persistentDataObject;

	private PersistentData m_persistentData;

	public bool CurrentState => m_state;

	private void Start()
	{
		bool state = m_state;
		if (m_startingStateVariable != null)
		{
			state = m_startingStateVariable.Value;
		}
		SetState(state, sendEvent: false);
	}

	public void ToggleState()
	{
		SetState(!m_state);
	}

	private void SetState(bool state, bool sendEvent = true)
	{
		m_image.sprite = (state ? m_onSprite : m_offSprite);
		m_state = state;
		if (sendEvent)
		{
			if (m_persistentData != null)
			{
				m_persistentData.m_set = state;
			}
			OnStateChanged?.Invoke(state);
		}
	}

	public void TripIfActive()
	{
		if (m_state)
		{
			SetState(state: false);
		}
	}

	public bool IsTripSwitchActive()
	{
		return m_state;
	}

	public void Pressed()
	{
		if (!IsTripSwitchActive())
		{
			m_turnOnAudioEvent?.Play2D();
			if (m_controller.OnTripSwitchPressedValidate())
			{
				ToggleState();
			}
		}
		else
		{
			m_turnOffAudioEvent?.Play2D();
			if (!m_controller.IsComplete())
			{
				ToggleState();
			}
		}
	}

	public bool RequiresPersistentData()
	{
		return true;
	}

	public void ReceiveDataStoreEntry(PersistentDataObject dataEntry)
	{
		m_persistentDataObject = dataEntry;
		m_persistentData = ((m_persistentDataObject.Data != null) ? (m_persistentDataObject.Data as PersistentData) : null);
		if (m_persistentData != null)
		{
			SetState(m_persistentData.m_set, sendEvent: false);
			return;
		}
		m_persistentData = new PersistentData();
		m_persistentDataObject.Data = m_persistentData;
		m_persistentData.m_set = m_state;
	}
}
