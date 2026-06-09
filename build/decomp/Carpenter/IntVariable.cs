using System;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Variables/Int")]
public class IntVariable : BaseVariable<int>
{
	[Tooltip("Event to trigger if this value changes")]
	[SerializeField]
	private IntGameEventChannel _valueChangedEvent;

	private UnityAction<int> m_onValueChanged;

	public void RegisterListener(UnityAction<int> listener)
	{
		m_onValueChanged = (UnityAction<int>)Delegate.Combine(m_onValueChanged, listener);
	}

	public void UnregisterListener(UnityAction<int> listener)
	{
		m_onValueChanged = (UnityAction<int>)Delegate.Remove(m_onValueChanged, listener);
	}

	public override void OnValueChanged(int newValue)
	{
		if ((bool)_valueChangedEvent)
		{
			_valueChangedEvent.Raise(newValue);
		}
		m_onValueChanged?.Invoke(newValue);
	}
}
