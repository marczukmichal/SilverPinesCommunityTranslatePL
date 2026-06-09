using System;
using UnityEngine;

public class BaseVariable<T> : ScriptableObject where T : IComparable, IComparable<T>, IConvertible, IEquatable<T>
{
	[SerializeField]
	private T m_value;

	[SerializeField]
	private BaseReference<T> m_resetValue;

	public T Value
	{
		get
		{
			return m_value;
		}
		set
		{
			if (!m_value.Equals(value))
			{
				m_value = value;
				OnValueChanged(value);
			}
		}
	}

	public virtual void OnValueChanged(T newValue)
	{
	}

	public void SetValue(T value)
	{
		Value = value;
	}

	public void SetValue(BaseVariable<T> value)
	{
		Value = value.Value;
	}

	public string GetPersistentID()
	{
		return base.name;
	}

	public void Reset()
	{
		if (m_resetValue != null)
		{
			Value = m_resetValue.Value;
		}
	}
}
