using System;
using UnityEngine;

public class RadioProgressionHelper : MonoBehaviour, IPersistentComponent
{
	[Serializable]
	public class ProgressionState
	{
		public GameObject m_gameObject;

		[Header("Bool Requirement")]
		public ProgressionVariable m_boolRequirementVariable;

		[Header("Int Requirement")]
		public ProgressionVariableInt m_intRequirementVariable;

		public int m_intRequirementValue;
	}

	[Serializable]
	private class PersistentData
	{
		public int m_setIndex = -1;

		public bool m_triggered;
	}

	[SerializeField]
	private ProgressionVariableInt m_currentRadioIndex;

	[SerializeField]
	private ProgressionState[] m_states;

	private PersistentDataObject m_persistentDataObject;

	private PersistentData m_persistentData;

	private int GetNextIndex()
	{
		int value = m_currentRadioIndex.Value;
		if ((bool)m_states[value].m_boolRequirementVariable && !m_states[value].m_boolRequirementVariable.Value)
		{
			return -1;
		}
		if ((bool)m_states[value].m_intRequirementVariable && m_states[value].m_intRequirementVariable.Value < m_states[value].m_intRequirementValue)
		{
			return -1;
		}
		m_currentRadioIndex.Value++;
		return value;
	}

	private void SetIndex(int index)
	{
		m_persistentData.m_setIndex = index;
		for (int i = 0; i < m_states.Length; i++)
		{
			bool active = i == index;
			if ((bool)m_states[i].m_gameObject)
			{
				m_states[i].m_gameObject.SetActive(active);
			}
		}
	}

	public void TriggerRadio()
	{
		if (m_persistentData != null && m_persistentData.m_setIndex != -1 && !m_persistentData.m_triggered)
		{
			m_persistentData.m_triggered = true;
			GameObject gameObject = m_states[m_persistentData.m_setIndex].m_gameObject;
			if (gameObject != null)
			{
				MinigameRadioData component = gameObject.GetComponent<MinigameRadioData>();
				component.SetActiveChannel(0);
				component.ResetTimelinePosition();
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
		if (m_persistentData != null && m_persistentData.m_setIndex != -1)
		{
			SetIndex(m_persistentData.m_setIndex);
			return;
		}
		m_persistentData = new PersistentData();
		m_persistentDataObject.Data = m_persistentData;
		SetIndex(GetNextIndex());
	}
}
