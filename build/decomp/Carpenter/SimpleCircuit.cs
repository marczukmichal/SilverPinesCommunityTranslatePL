using System;
using UnityEngine;
using UnityEngine.Events;

public class SimpleCircuit : MonoBehaviour, IPersistentComponent
{
	[Serializable]
	private class PersistentData
	{
		public bool m_isOn;
	}

	[SerializeField]
	private bool m_startOn;

	private bool m_isOn;

	public UnityAction<bool> OnCircuitChanged;

	private PersistentDataObject m_persistentDataObject;

	private PersistentData m_persistentData;

	private void Awake()
	{
		SetState(m_startOn);
	}

	public void SetState(bool state)
	{
		m_isOn = state;
		if (m_persistentData != null)
		{
			m_persistentData.m_isOn = m_isOn;
		}
		OnCircuitChanged?.Invoke(state);
	}

	public void ToggleState(int id)
	{
		SetState(!GetState());
	}

	public bool GetState()
	{
		return m_isOn;
	}

	public void ReceiveDataStoreEntry(PersistentDataObject dataEntry)
	{
		m_persistentDataObject = dataEntry;
		m_persistentData = ((m_persistentDataObject.Data != null) ? (m_persistentDataObject.Data as PersistentData) : null);
		if (m_persistentData != null)
		{
			SetState(m_persistentData.m_isOn);
			return;
		}
		m_persistentData = new PersistentData();
		m_persistentDataObject.Data = m_persistentData;
		m_persistentData.m_isOn = m_isOn;
	}

	public bool RequiresPersistentData()
	{
		return true;
	}
}
