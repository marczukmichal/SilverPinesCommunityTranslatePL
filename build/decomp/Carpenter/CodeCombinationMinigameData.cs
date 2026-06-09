using System;
using UnityEngine;
using UnityEngine.Serialization;

[DisallowMultipleComponent]
[ShowInDesignerInspector]
public class CodeCombinationMinigameData : MonoBehaviour, IPersistentComponent
{
	[Serializable]
	private class PersistentData
	{
		public int[] m_currentValues;
	}

	[FormerlySerializedAs("m_code")]
	[SerializeField]
	private string m_targetCode;

	[SerializeField]
	private int[] m_startingSelectedIndices;

	private PersistentDataObject m_persistentDataObject;

	private PersistentData m_persistentData;

	private int DigitsCount
	{
		get
		{
			if (m_targetCode.Contains("/"))
			{
				int num = 1;
				string targetCode = m_targetCode;
				for (int i = 0; i < targetCode.Length; i++)
				{
					if (targetCode[i] == '/')
					{
						num++;
					}
				}
				return num;
			}
			return m_targetCode.Length;
		}
	}

	public string Code => m_targetCode;

	public void SetValueAtIndex(int index, int value)
	{
		if (m_persistentData != null)
		{
			m_persistentData.m_currentValues[index] = value;
		}
	}

	public int GetValueAtIndex(int index)
	{
		if (m_persistentData != null)
		{
			return m_persistentData.m_currentValues[index];
		}
		if (m_startingSelectedIndices.Length > index)
		{
			return m_startingSelectedIndices[index];
		}
		return 0;
	}

	public bool RequiresPersistentData()
	{
		return true;
	}

	public void ReceiveDataStoreEntry(PersistentDataObject dataEntry)
	{
		m_persistentDataObject = dataEntry;
		m_persistentData = ((m_persistentDataObject.Data != null) ? (m_persistentDataObject.Data as PersistentData) : null);
		if (m_persistentData == null)
		{
			m_persistentData = new PersistentData();
			m_persistentData.m_currentValues = new int[DigitsCount];
			Array.Copy(m_startingSelectedIndices, m_persistentData.m_currentValues, Mathf.Min(m_startingSelectedIndices.Length, m_persistentData.m_currentValues.Length));
			m_persistentDataObject.Data = m_persistentData;
		}
	}
}
