using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class MinigameSewerPressureMain : MonoBehaviour, IMinigameBackInputHandler, IMinigameComponent
{
	[SerializeField]
	private MinigameSewerPressureValve[] m_valves;

	[SerializeField]
	private MinigameSewerPressureGauge m_gauge;

	[SerializeField]
	private float m_goalTotalPressure;

	[SerializeField]
	private float m_completionAnimationPressure;

	private bool m_completed;

	private float m_currentPressure;

	private MinigameSewerPressureEvents m_events;

	public UnityAction<float> m_pressureLevelChanged;

	private void Update()
	{
		if (!m_completed)
		{
			float num = 0f;
			MinigameSewerPressureValve[] valves = m_valves;
			foreach (MinigameSewerPressureValve minigameSewerPressureValve in valves)
			{
				num += minigameSewerPressureValve.Value;
			}
			m_currentPressure = num;
			if (num > m_goalTotalPressure)
			{
				m_completed = true;
				StartCoroutine(CompletionFlow());
			}
			m_pressureLevelChanged?.Invoke(m_currentPressure / m_goalTotalPressure);
		}
		m_gauge.SetValue(m_currentPressure);
	}

	private IEnumerator CompletionFlow()
	{
		while (m_currentPressure < m_completionAnimationPressure)
		{
			m_currentPressure += Time.deltaTime;
		}
		yield return new WaitForSeconds(2f);
		GetComponentInParent<MinigameScene>().SetMinigameCompleted();
	}

	public bool TryBackInput()
	{
		return m_completed;
	}

	public void Setup(GameObject parent)
	{
		MinigameSewerPressureEvents component = parent.GetComponent<MinigameSewerPressureEvents>();
		if (component != null)
		{
			m_events = component;
			component.RegisterEvents(this);
		}
	}

	private void OnDestroy()
	{
		if (m_events != null)
		{
			m_events.UnregisterEvents(this);
		}
	}
}
