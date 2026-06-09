using System;
using UnityEngine;
using UnityEngine.Events;

public class PersistentScriptCounter : MonoBehaviour, IPersistentComponent
{
	[Serializable]
	public class CountEvent
	{
		public int m_number;

		public UnityEvent m_event;
	}

	[Serializable]
	private class PersistentData
	{
		public int m_currentValue;
	}

	[SerializeField]
	private int m_startingValue;

	[SerializeField]
	private CountEvent[] m_counterEvents;

	private int m_currentValue;

	private PersistentDataObject m_persistentDataObject;

	private PersistentData m_persistentData;

	private void Start()
	{
		if (m_persistentData == null)
		{
			m_currentValue = m_startingValue;
		}
	}

	public void IncrementCounter()
	{
		m_currentValue++;
		if (m_persistentData != null)
		{
			m_persistentData.m_currentValue = m_currentValue;
		}
		TriggerEvent();
	}

	public void DecrementCounter()
	{
		m_currentValue--;
		if (m_persistentData != null)
		{
			m_persistentData.m_currentValue = m_currentValue;
		}
		TriggerEvent();
	}

	public void TriggerEvent()
	{
		CountEvent[] counterEvents = m_counterEvents;
		foreach (CountEvent countEvent in counterEvents)
		{
			if (countEvent.m_number == m_currentValue)
			{
				countEvent.m_event.Invoke();
				break;
			}
		}
	}

	public void ReceiveDataStoreEntry(PersistentDataObject dataEntry)
	{
		m_persistentDataObject = dataEntry;
		m_persistentData = ((m_persistentDataObject.Data != null) ? (m_persistentDataObject.Data as PersistentData) : null);
		if (m_persistentData != null)
		{
			m_currentValue = m_persistentData.m_currentValue;
			TriggerEvent();
			return;
		}
		m_persistentData = new PersistentData();
		m_persistentDataObject.Data = m_persistentData;
		m_currentValue = m_startingValue;
		m_persistentData.m_currentValue = m_currentValue;
	}

	public bool RequiresPersistentData()
	{
		return true;
	}
}
