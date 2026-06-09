using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[ExecuteAlways]
public class TimeOfDayParent : MonoBehaviour
{
	[SerializeField]
	private TimeOfDay m_timeOfDay;

	private List<TimeOfDaySceneLighting> m_lightings;

	private void Awake()
	{
		TimeOfDay timeOfDay = m_timeOfDay;
		timeOfDay.m_onTimeOfDayChanged = (UnityAction<TimeOfDay.TimePeriod>)Delegate.Combine(timeOfDay.m_onTimeOfDayChanged, new UnityAction<TimeOfDay.TimePeriod>(SetTimeOfDay));
		m_lightings = new List<TimeOfDaySceneLighting>(GetComponentsInChildren<TimeOfDaySceneLighting>(includeInactive: true));
		SetTimeOfDay(m_timeOfDay.CurrentTimeOfDay);
	}

	private void OnDestroy()
	{
		TimeOfDay timeOfDay = m_timeOfDay;
		timeOfDay.m_onTimeOfDayChanged = (UnityAction<TimeOfDay.TimePeriod>)Delegate.Remove(timeOfDay.m_onTimeOfDayChanged, new UnityAction<TimeOfDay.TimePeriod>(SetTimeOfDay));
	}

	private void SetTimeOfDay(TimeOfDay.TimePeriod timePeriod)
	{
		foreach (TimeOfDaySceneLighting lighting in m_lightings)
		{
			if (lighting.gameObject.activeSelf)
			{
				lighting.gameObject.SetActive(value: false);
			}
		}
		foreach (TimeOfDaySceneLighting lighting2 in m_lightings)
		{
			lighting2.gameObject.SetActive(lighting2.TimePeriod == timePeriod);
		}
	}
}
