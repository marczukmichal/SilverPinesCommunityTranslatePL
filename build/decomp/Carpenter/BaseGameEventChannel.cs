using System;
using UnityEngine;
using UnityEngine.Events;

public abstract class BaseGameEventChannel<T> : ScriptableObject
{
	[SerializeField]
	private UnityAction<T> m_onTriggered;

	public void Register(UnityAction<T> listener)
	{
		m_onTriggered = (UnityAction<T>)Delegate.Combine(m_onTriggered, listener);
	}

	public void Unregister(UnityAction<T> listener)
	{
		m_onTriggered = (UnityAction<T>)Delegate.Remove(m_onTriggered, listener);
	}

	public void Raise(T value)
	{
		if (m_onTriggered != null)
		{
			m_onTriggered(value);
		}
	}
}
