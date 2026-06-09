using System;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Game Event Channels/Event Channel (Void)")]
public class VoidGameEventChannel : ScriptableObject
{
	private UnityAction m_onTriggered;

	public void Register(UnityAction listener)
	{
		m_onTriggered = (UnityAction)Delegate.Combine(m_onTriggered, listener);
	}

	public void Unregister(UnityAction listener)
	{
		m_onTriggered = (UnityAction)Delegate.Remove(m_onTriggered, listener);
	}

	public void Raise()
	{
		if (m_onTriggered != null)
		{
			m_onTriggered();
		}
	}
}
