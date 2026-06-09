using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class RuntimeSet<T> : ScriptableObject, IEnumerable
{
	protected List<T> m_items = new List<T>();

	private UnityAction<T> m_onItemAdded;

	private UnityAction<T> m_onItemRemoved;

	public IReadOnlyCollection<T> Items => m_items.AsReadOnly();

	public void Register(UnityAction<T> onItemAddedListener, UnityAction<T> onItemRemovedListener)
	{
		m_onItemAdded = (UnityAction<T>)Delegate.Combine(m_onItemAdded, onItemAddedListener);
		m_onItemRemoved = (UnityAction<T>)Delegate.Combine(m_onItemRemoved, onItemRemovedListener);
	}

	public void Unregister(UnityAction<T> onItemAddedListener, UnityAction<T> onItemRemovedListener)
	{
		m_onItemAdded = (UnityAction<T>)Delegate.Remove(m_onItemAdded, onItemAddedListener);
		m_onItemRemoved = (UnityAction<T>)Delegate.Remove(m_onItemRemoved, onItemRemovedListener);
	}

	public void Add(T thing)
	{
		if (!m_items.Contains(thing))
		{
			m_items.Add(thing);
			m_onItemAdded?.Invoke(thing);
		}
	}

	public IEnumerator GetEnumerator()
	{
		return m_items.GetEnumerator();
	}

	public void Remove(T thing)
	{
		if (m_items.Contains(thing))
		{
			m_items.Remove(thing);
			m_onItemRemoved?.Invoke(thing);
		}
	}

	public bool Contains(T thing)
	{
		return m_items.Contains(thing);
	}
}
