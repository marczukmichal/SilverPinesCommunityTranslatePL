using System;
using UnityEngine;
using UnityEngine.Events;

public class ValueAdjustMinigame : MonoBehaviour, IMinigameComponent
{
	[SerializeField]
	private UnityEvent m_onComplete;

	private ValueAdjustMinigameData m_data;

	private void OnDestroy()
	{
		if (m_data != null)
		{
			ValueAdjustMinigameData data = m_data;
			data.OnMinigameCompletedEvent = (UnityAction)Delegate.Remove(data.OnMinigameCompletedEvent, new UnityAction(OnMinigameComplete));
		}
	}

	public void Setup(GameObject parent)
	{
		m_data = parent.GetComponent<ValueAdjustMinigameData>();
		if (m_data != null)
		{
			ValueAdjustMinigameData data = m_data;
			data.OnMinigameCompletedEvent = (UnityAction)Delegate.Combine(data.OnMinigameCompletedEvent, new UnityAction(OnMinigameComplete));
		}
	}

	private void OnMinigameComplete()
	{
		m_onComplete.Invoke();
	}

	public void IncreaseValue()
	{
		if (m_data != null)
		{
			m_data.IncreaseValue();
		}
	}

	public void DecreaseValue()
	{
		if (m_data != null)
		{
			m_data.DecreaseValue();
		}
	}
}
