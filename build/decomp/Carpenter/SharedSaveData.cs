using System;
using UnityEngine;

[Serializable]
public class SharedSaveData
{
	[SerializeField]
	private int m_saveProfileIndex;

	public int SaveProfileIndex
	{
		get
		{
			return m_saveProfileIndex;
		}
		set
		{
			m_saveProfileIndex = value;
		}
	}
}
