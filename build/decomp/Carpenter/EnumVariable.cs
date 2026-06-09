using System;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Variables/Enum")]
public class EnumVariable<T> : BaseVariable<int> where T : Enum
{
	private UnityAction<T> m_onValueChanged;

	public T GetEnumValue()
	{
		return (T)Enum.ToObject(typeof(T), base.Value);
	}

	public void SetEnumValue(T newValue)
	{
		if (!GetEnumValue().Equals(newValue))
		{
			base.Value = Convert.ToInt32(newValue);
			m_onValueChanged?.Invoke(newValue);
		}
	}

	public void RegisterListener(UnityAction<T> listener)
	{
		m_onValueChanged = (UnityAction<T>)Delegate.Combine(m_onValueChanged, listener);
	}

	public void UnregisterListener(UnityAction<T> listener)
	{
		m_onValueChanged = (UnityAction<T>)Delegate.Remove(m_onValueChanged, listener);
	}
}
