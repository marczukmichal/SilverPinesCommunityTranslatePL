using System;
using UnityEngine;
using UnityEngine.Events;

public abstract class RuntimeAnchor<T> : ScriptableObject where T : class
{
	private T m_item;

	private UnityAction<T> m_onItemChanged;

	public T Item => m_item;

	public Type GetSystemType()
	{
		return m_item.GetType();
	}

	public void Set(T item)
	{
		if (m_item != item)
		{
			m_item = item;
			m_onItemChanged?.Invoke(item);
		}
	}

	public void Register(UnityAction<T> listener)
	{
		m_onItemChanged = (UnityAction<T>)Delegate.Combine(m_onItemChanged, listener);
	}

	public void Unregister(UnityAction<T> listener)
	{
		m_onItemChanged = (UnityAction<T>)Delegate.Remove(m_onItemChanged, listener);
	}
}
