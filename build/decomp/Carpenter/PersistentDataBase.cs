using System;
using UnityEngine;

[Serializable]
public class PersistentDataBase<T>
{
	[SerializeField]
	private string m_guid;

	[SerializeField]
	public T Data;

	public string GUID => m_guid;

	public PersistentDataBase(string guid)
	{
		m_guid = guid;
	}
}
