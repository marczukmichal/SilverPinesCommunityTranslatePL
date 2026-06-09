using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

[AddComponentMenu("Gameplay/Progression Variable State Events")]
public class ProgressionVariableEvents : MonoBehaviour
{
	public enum IntCheckType
	{
		Equal,
		GreaterThan,
		GreaterThanOrEqualTo
	}

	[Serializable]
	private struct IntEvent
	{
		public int m_value;

		public IntCheckType m_intCheckType;

		[FormerlySerializedAs("m_onSetEvent")]
		public UnityEvent m_onTrueEvent;

		[FormerlySerializedAs("m_onNotSetEvent")]
		public UnityEvent m_onFalseEvent;
	}

	[Header("Booleans")]
	[SerializeField]
	private ProgressionVariable m_variable;

	[Tooltip("If this array is set then this will only count as true if all of these are true")]
	[SerializeField]
	private ProgressionVariable[] m_variableArray;

	[Header("Unity Events")]
	[SerializeField]
	private UnityEvent m_onTrueEvents;

	[SerializeField]
	private UnityEvent m_onFalseEvents;

	[Header("Toggle GameObjects")]
	[SerializeField]
	private List<GameObject> m_enableGameObjects;

	[SerializeField]
	private List<GameObject> m_disableGameObjects;

	[Header("Integers")]
	[SerializeField]
	private ProgressionVariableInt m_intVariable;

	[Header("Advanced")]
	[Tooltip("If listener disabled, then these events will not be applied actively when the value changes, only when the value loaded as persistent change on scene load.")]
	[SerializeField]
	private bool m_disableListener;

	[SerializeField]
	private IntEvent[] m_intEvents;

	private void OnEnable()
	{
		StartCoroutine(DoDelayedCheck());
		if (m_disableListener)
		{
			return;
		}
		if ((bool)m_variable)
		{
			m_variable.RegisterListener(OnValueChanged);
		}
		if (m_intVariable != null)
		{
			m_intVariable.RegisterListener(OnValueChanged);
		}
		if (m_variableArray == null)
		{
			return;
		}
		ProgressionVariable[] variableArray = m_variableArray;
		foreach (ProgressionVariable progressionVariable in variableArray)
		{
			if (!(progressionVariable == null))
			{
				progressionVariable.RegisterListener(OnValueChanged);
			}
		}
	}

	private void OnDisable()
	{
		if (m_disableListener)
		{
			return;
		}
		if ((bool)m_variable)
		{
			m_variable.UnregisterListener(OnValueChanged);
		}
		if (m_intVariable != null)
		{
			m_intVariable.UnregisterListener(OnValueChanged);
		}
		if (m_variableArray == null)
		{
			return;
		}
		ProgressionVariable[] variableArray = m_variableArray;
		foreach (ProgressionVariable progressionVariable in variableArray)
		{
			if (!(progressionVariable == null))
			{
				progressionVariable.UnregisterListener(OnValueChanged);
			}
		}
	}

	private IEnumerator DoDelayedCheck()
	{
		yield return new WaitForEndOfFrame();
		if (m_variable != null || m_variableArray.Length != 0)
		{
			OnValueChanged(_: false);
		}
		if (m_intVariable != null)
		{
			OnValueChanged(m_intVariable.Value);
		}
	}

	private void OnValueChanged(bool _)
	{
		bool flag = true;
		if ((bool)m_variable && !m_variable.Value)
		{
			flag = false;
		}
		if (m_variableArray != null)
		{
			ProgressionVariable[] variableArray = m_variableArray;
			foreach (ProgressionVariable progressionVariable in variableArray)
			{
				if (!(progressionVariable == null) && !progressionVariable.Value)
				{
					flag = false;
					break;
				}
			}
		}
		if (flag)
		{
			m_onTrueEvents.Invoke();
		}
		else
		{
			m_onFalseEvents.Invoke();
		}
		if (m_enableGameObjects != null)
		{
			foreach (GameObject enableGameObject in m_enableGameObjects)
			{
				enableGameObject.SetActive(flag);
			}
		}
		if (m_disableGameObjects == null)
		{
			return;
		}
		foreach (GameObject disableGameObject in m_disableGameObjects)
		{
			disableGameObject.SetActive(!flag);
		}
	}

	private void OnValueChanged(int newValue)
	{
		IntEvent[] intEvents = m_intEvents;
		for (int i = 0; i < intEvents.Length; i++)
		{
			IntEvent intEvent = intEvents[i];
			bool flag = false;
			switch (intEvent.m_intCheckType)
			{
			case IntCheckType.Equal:
				flag = newValue == intEvent.m_value;
				break;
			case IntCheckType.GreaterThan:
				flag = newValue > intEvent.m_value;
				break;
			case IntCheckType.GreaterThanOrEqualTo:
				flag = newValue >= intEvent.m_value;
				break;
			}
			if (flag)
			{
				intEvent.m_onTrueEvent.Invoke();
			}
			else
			{
				intEvent.m_onFalseEvent.Invoke();
			}
		}
	}
}
