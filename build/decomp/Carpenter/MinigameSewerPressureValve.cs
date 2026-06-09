using System;
using UnityEngine;
using UnityEngine.Events;

public class MinigameSewerPressureValve : MonoBehaviour
{
	[SerializeField]
	private float m_rateOfChange;

	[SerializeField]
	private MinigameSewerPressureGauge m_gauge;

	[SerializeField]
	private MinigameInteractiveDial m_dial;

	[Header("Enabled State")]
	[SerializeField]
	private ProgressionVariable m_variable;

	[SerializeField]
	private AudioVoicedEvent m_disabledVoiceEvent;

	private float m_value;

	private bool m_triggeredVO;

	public float Value => m_value;

	private void Start()
	{
		MinigameInteractiveDial dial = m_dial;
		dial.OnDialRotated = (UnityAction<float>)Delegate.Combine(dial.OnDialRotated, new UnityAction<float>(OnDialChanged));
		m_triggeredVO = false;
	}

	private void OnDialChanged(float delta)
	{
		if (m_variable != null && !m_variable.Value)
		{
			if (m_disabledVoiceEvent != null && !m_triggeredVO)
			{
				m_triggeredVO = true;
				m_disabledVoiceEvent.TriggerOnPlayer();
			}
		}
		else
		{
			m_value += delta * m_rateOfChange;
			m_value = Mathf.Clamp01(m_value);
			OnValueChanged();
		}
	}

	private void OnValueChanged()
	{
		m_gauge.SetValue(m_value);
	}
}
