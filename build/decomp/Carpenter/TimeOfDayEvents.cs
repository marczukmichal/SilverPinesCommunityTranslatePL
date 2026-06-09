using UnityEngine;
using UnityEngine.Events;

[ExecuteAlways]
public class TimeOfDayEvents : MonoBehaviour
{
	[SerializeField]
	private UnityEvent m_onDayTime;

	[SerializeField]
	private UnityEvent m_onNightTime;

	private bool m_isNight;

	private void Start()
	{
		m_isNight = IsCurrentlyNight();
		if (m_isNight)
		{
			m_onNightTime?.Invoke();
		}
		else
		{
			m_onDayTime?.Invoke();
		}
	}

	private bool IsCurrentlyNight()
	{
		return GlobalReferences.Instance.TimeOfDay.IsNightTime();
	}

	private void Update()
	{
		bool flag = IsCurrentlyNight();
		if (flag != m_isNight)
		{
			m_isNight = flag;
			if (m_isNight)
			{
				m_onNightTime?.Invoke();
			}
			else
			{
				m_onDayTime?.Invoke();
			}
		}
	}
}
