using System;
using UnityEngine;

[Serializable]
public struct MapConnection
{
	[SerializeField]
	private int m_connectionID;

	[SerializeField]
	private bool m_isStart;

	[SerializeField]
	private MapConnectionType m_connectionType;

	[SerializeField]
	private ItemDefinition m_requiredItem;

	public int ConnectionID => m_connectionID;

	public bool IsStart => m_isStart;

	public MapConnectionType ConnectionType => m_connectionType;

	public ItemDefinition RequiredItem => m_requiredItem;
}
