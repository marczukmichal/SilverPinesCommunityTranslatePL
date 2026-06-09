using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveProfileMetadata
{
	[SerializeField]
	private int m_mostRecentSaveSlotIndex = -1;

	private HashSet<int> m_validSaveSlots = new HashSet<int>();

	public bool HasData => m_mostRecentSaveSlotIndex != -1;

	public int MostRecentSaveSlotIndex => m_mostRecentSaveSlotIndex;

	public void SetValidSaveSlot(int index)
	{
		if (!m_validSaveSlots.Contains(index))
		{
			m_validSaveSlots.Add(index);
		}
	}

	public bool HasSaveInSlot(int index)
	{
		return m_validSaveSlots.Contains(index);
	}

	public bool HasAnySaves()
	{
		return m_validSaveSlots.Count > 0;
	}

	public void SetMostRecentSaveSlotIndex(int index)
	{
		SetValidSaveSlot(index);
		m_mostRecentSaveSlotIndex = index;
	}
}
