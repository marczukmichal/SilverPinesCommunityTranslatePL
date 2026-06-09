using System;
using UnityEngine;

[Serializable]
public class PersistentDataObject
{
	[SerializeField]
	private string m_guid;

	[SerializeReference]
	public object Data;

	public string GUID => m_guid;

	public PersistentDataObject(string guid)
	{
		m_guid = guid;
	}
}
