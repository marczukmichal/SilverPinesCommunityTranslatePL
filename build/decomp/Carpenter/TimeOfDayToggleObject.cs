using System;
using UnityEngine;
using UnityEngine.Events;

public class TimeOfDayToggleObject : MonoBehaviour
{
	[SerializeField]
	private TimeOfDay.TimePeriod m_timeOfDay;

	private void Start()
	{
		TimeOfDay timeOfDay = GlobalReferences.Instance.TimeOfDay;
		timeOfDay.m_onTimeOfDayChanged = (UnityAction<TimeOfDay.TimePeriod>)Delegate.Combine(timeOfDay.m_onTimeOfDayChanged, new UnityAction<TimeOfDay.TimePeriod>(OnTimeOfDayChanged));
		OnTimeOfDayChanged(GlobalReferences.Instance.TimeOfDay.CurrentTimeOfDay);
	}

	private void OnDestroy()
	{
		TimeOfDay timeOfDay = GlobalReferences.Instance.TimeOfDay;
		timeOfDay.m_onTimeOfDayChanged = (UnityAction<TimeOfDay.TimePeriod>)Delegate.Remove(timeOfDay.m_onTimeOfDayChanged, new UnityAction<TimeOfDay.TimePeriod>(OnTimeOfDayChanged));
	}

	private void OnTimeOfDayChanged(TimeOfDay.TimePeriod timeOfDay)
	{
		base.gameObject.SetActive(timeOfDay == m_timeOfDay);
	}
}
