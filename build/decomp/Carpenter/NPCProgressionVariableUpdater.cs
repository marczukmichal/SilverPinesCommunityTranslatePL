using UnityEngine;
using UnityEngine.Events;

public class NPCProgressionVariableUpdater : MonoBehaviour
{
	public enum Mode
	{
		OnEnable,
		OnDisable,
		Script
	}

	[SerializeField]
	private ProgressionVariableInt m_npcIntVariable;

	[SerializeField]
	private int m_targetValue;

	[SerializeField]
	private UnityEvent m_event;

	[SerializeField]
	private Mode m_mode;

	private void OnEnable()
	{
		if (m_mode == Mode.OnEnable)
		{
			UpdateVariable();
		}
	}

	private void OnDisable()
	{
		if (m_mode == Mode.OnDisable)
		{
			UpdateVariable();
		}
	}

	public void UpdateVariable()
	{
		if (m_npcIntVariable != null && m_npcIntVariable.Value < m_targetValue)
		{
			m_npcIntVariable.Value = m_targetValue;
			m_event.Invoke();
		}
	}
}
