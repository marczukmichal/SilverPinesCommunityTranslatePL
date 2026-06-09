using System;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Variables/Float")]
public class FloatVariable : BaseVariable<float>
{
	[Tooltip("Event to trigger if this value changes")]
	[SerializeField]
	private FloatGameEventChannel _valueChangedEvent;

	private UnityAction<float> m_onValueChanged;

	public void RegisterListener(UnityAction<float> listener)
	{
		m_onValueChanged = (UnityAction<float>)Delegate.Combine(m_onValueChanged, listener);
	}

	public void UnregisterListener(UnityAction<float> listener)
	{
		m_onValueChanged = (UnityAction<float>)Delegate.Remove(m_onValueChanged, listener);
	}

	public override void OnValueChanged(float newValue)
	{
		if ((bool)_valueChangedEvent)
		{
			_valueChangedEvent.Raise(newValue);
		}
		m_onValueChanged?.Invoke(newValue);
	}
}
