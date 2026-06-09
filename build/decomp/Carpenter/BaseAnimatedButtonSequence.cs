using System;
using UnityEngine;

public abstract class BaseAnimatedButtonSequence : MonoBehaviour, IPersistentComponent
{
	[Serializable]
	private class PersistentData
	{
		public bool m_isDone;
	}

	[SerializeField]
	protected int m_sequenceCount = 3;

	protected bool m_isDone;

	private int m_doneSequences;

	private PersistentDataObject m_persistentDataObject;

	private PersistentData m_persistentData;

	public int SequenceCount => m_sequenceCount;

	public bool IsDone => m_isDone;

	public int DoneSequences => m_doneSequences;

	public virtual void OnProgress()
	{
		m_doneSequences++;
		OnProgressUpdated();
		if (m_doneSequences == m_sequenceCount)
		{
			OnComplete();
		}
	}

	public virtual void OnProgressUpdated()
	{
	}

	public virtual void OnComplete()
	{
		m_isDone = true;
		if (m_persistentData != null)
		{
			m_persistentData.m_isDone = true;
		}
	}

	public abstract void OnCancel();

	public virtual void ResetProgress()
	{
		m_doneSequences = 0;
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
			if (m_persistentData.m_isDone)
			{
				m_doneSequences = m_sequenceCount;
				OnProgressUpdated();
				OnComplete();
			}
		}
		else
		{
			m_persistentData = new PersistentData();
			m_persistentDataObject.Data = m_persistentData;
		}
	}
}
