using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Misc/Time Of Day")]
public class TimeOfDay : ScriptableObject
{
	public enum TimePeriod
	{
		Evening,
		Night,
		Day
	}

	private TimePeriod m_currentTimeOfDay;

	private TimePeriod m_queuedTimeOfDay;

	public UnityAction<TimePeriod> m_onTimeOfDayChanged;

	public TimePeriod CurrentTimeOfDay => m_currentTimeOfDay;

	public string TimeString()
	{
		return m_currentTimeOfDay.ToString();
	}

	public void SetTimeOfDay(TimePeriod timeOfDay)
	{
		if (m_currentTimeOfDay != timeOfDay)
		{
			m_queuedTimeOfDay = (m_currentTimeOfDay = timeOfDay);
			m_onTimeOfDayChanged?.Invoke(timeOfDay);
		}
	}

	public void SetEvening()
	{
		SetTimeOfDay(TimePeriod.Evening);
	}

	public void SetNight()
	{
		SetTimeOfDay(TimePeriod.Night);
	}

	public void SetDay()
	{
		SetTimeOfDay(TimePeriod.Day);
	}

	public void SetEveningQueued()
	{
		m_queuedTimeOfDay = TimePeriod.Evening;
	}

	public void SetNightQueued()
	{
		m_queuedTimeOfDay = TimePeriod.Night;
	}

	public void SetDayQueued()
	{
		m_queuedTimeOfDay = TimePeriod.Day;
	}

	public void ApplyQueuedChange()
	{
		if (m_queuedTimeOfDay != m_currentTimeOfDay)
		{
			SetTimeOfDay(m_queuedTimeOfDay);
		}
	}

	public bool IsNightTime()
	{
		return CurrentTimeOfDay != TimePeriod.Day;
	}

	[DebugCommand("time_of_day", "Sets the current time of day for the game", "time 0-2", typeof(int), false)]
	public static void SetTimeOfDay(int timeOfDay)
	{
		GlobalReferences.Instance.TimeOfDay.SetTimeOfDay((TimePeriod)timeOfDay);
	}
}
