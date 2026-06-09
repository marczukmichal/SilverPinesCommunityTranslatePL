using System;
using UnityEngine;
using UnityEngine.Events;

public class ValueAdjustMinigameData : MonoBehaviour, IPersistentComponent
{
	[Serializable]
	private class PersistentData
	{
		public float m_currentValue;

		public bool m_completed;
	}

	[SerializeField]
	private UnityEvent m_onMinigameComplete;

	[SerializeField]
	private float m_targetValueMin = 0.25f;

	[SerializeField]
	private float m_targetValueMax = 0.5f;

	[SerializeField]
	private float m_minValue;

	[SerializeField]
	private float m_maxValue = 1f;

	[SerializeField]
	private float m_startingValue = 0.5f;

	[SerializeField]
	private float m_changeRate = 1f;

	[SerializeField]
	private float m_completeWaitTime = 2f;

	[SerializeField]
	private bool m_loops;

	public UnityAction OnValueChangedEvent;

	public UnityAction OnMinigameCompletedEvent;

	private float m_currentValue;

	private float m_correctTime;

	private PersistentDataObject m_persistentDataObject;

	private PersistentData m_persistentData;

	public float CurrentValue => m_currentValue;

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
			m_currentValue = m_persistentData.m_currentValue;
			return;
		}
		m_persistentData = new PersistentData();
		m_persistentDataObject.Data = m_persistentData;
		m_persistentData.m_currentValue = (m_currentValue = m_startingValue);
	}

	private void Update()
	{
		if (m_persistentData != null && !m_persistentData.m_completed)
		{
			if (m_currentValue > m_targetValueMin && m_currentValue < m_targetValueMax)
			{
				m_correctTime += Time.deltaTime;
			}
			else
			{
				m_correctTime = 0f;
			}
			if (m_correctTime >= m_completeWaitTime)
			{
				CompleteMinigame();
			}
		}
	}

	public void IncreaseValue(float scalar = 1f)
	{
		AdjustValue(m_changeRate * Time.deltaTime * scalar);
	}

	public void DecreaseValue(float scalar = 1f)
	{
		AdjustValue((0f - m_changeRate) * Time.deltaTime * scalar);
	}

	private void AdjustValue(float adjustment)
	{
		m_currentValue += adjustment;
		if (m_loops)
		{
			if (m_currentValue < m_minValue)
			{
				m_currentValue = m_maxValue + (m_currentValue - m_minValue);
			}
			else if (m_currentValue > m_maxValue)
			{
				m_currentValue = m_minValue + (m_currentValue - m_maxValue);
			}
		}
		else
		{
			m_currentValue = Mathf.Clamp(m_currentValue, m_minValue, m_maxValue);
		}
		if (m_persistentData != null)
		{
			m_persistentData.m_currentValue = m_currentValue;
		}
		OnValueChangedEvent?.Invoke();
	}

	private void CompleteMinigame()
	{
		if (m_persistentData != null)
		{
			m_persistentData.m_completed = true;
		}
		OnMinigameCompletedEvent?.Invoke();
	}

	public bool IsComplete()
	{
		if (m_persistentData != null)
		{
			return m_persistentData.m_completed;
		}
		return false;
	}
}
