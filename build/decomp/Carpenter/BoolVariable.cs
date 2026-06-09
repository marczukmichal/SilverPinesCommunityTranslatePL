using System;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Variables/Bool")]
public class BoolVariable : BaseVariable<bool>
{
	[Tooltip("Event to trigger if this value changes")]
	[SerializeField]
	private BoolGameEventChannel _valueChangedEvent;

	private UnityAction<bool> m_onValueChanged;

	public void RegisterListener(UnityAction<bool> listener)
	{
		m_onValueChanged = (UnityAction<bool>)Delegate.Combine(m_onValueChanged, listener);
	}

	public void UnregisterListener(UnityAction<bool> listener)
	{
		m_onValueChanged = (UnityAction<bool>)Delegate.Remove(m_onValueChanged, listener);
	}

	public override void OnValueChanged(bool newValue)
	{
		if ((bool)_valueChangedEvent)
		{
			_valueChangedEvent.Raise(newValue);
		}
		m_onValueChanged?.Invoke(newValue);
	}
}
