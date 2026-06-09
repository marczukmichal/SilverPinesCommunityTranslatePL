using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

[AddComponentMenu("/CircuitManager")]
public class CircuitManager : MonoBehaviour, IPersistentComponent
{
	[Serializable]
	private struct CircuitSetup
	{
		public int m_id;

		[Header("Starting State")]
		[FormerlySerializedAs("m_variable")]
		public ProgressionVariable m_startingStateVariable;

		[FormerlySerializedAs("m_startingState")]
		public bool m_startingStateConstant;
	}

	[Serializable]
	private class PersistentData
	{
		public List<int> m_keys;

		public List<bool> m_states;
	}

	[Header("Anchor")]
	[SerializeField]
	private CircuitManagerAnchor m_anchor;

	[SerializeField]
	private CircuitSetup[] m_startingStates;

	[Header("Power Override")]
	[SerializeField]
	private ProgressionVariable m_scenePowerVariable;

	private Dictionary<int, bool> m_circuitStates;

	public UnityAction<int, bool> OnCircuitChanged;

	public UnityAction<bool> OnScenePowerStateChanged;

	private PersistentDataObject m_persistentDataObject;

	private PersistentData m_persistentData;

	private void Awake()
	{
		m_circuitStates = new Dictionary<int, bool>();
		CircuitSetup[] startingStates = m_startingStates;
		for (int i = 0; i < startingStates.Length; i++)
		{
			CircuitSetup circuitSetup = startingStates[i];
			if (circuitSetup.m_startingStateVariable != null)
			{
				circuitSetup.m_startingStateVariable.RegisterListener(OnStartingStateVariableChanged);
				SetState(circuitSetup.m_id, circuitSetup.m_startingStateVariable.Value, persistent: false);
			}
			else
			{
				SetState(circuitSetup.m_id, circuitSetup.m_startingStateConstant, persistent: false);
			}
		}
		m_anchor.Set(this);
		TimeOfDay timeOfDay = GlobalReferences.Instance.TimeOfDay;
		timeOfDay.m_onTimeOfDayChanged = (UnityAction<TimeOfDay.TimePeriod>)Delegate.Combine(timeOfDay.m_onTimeOfDayChanged, new UnityAction<TimeOfDay.TimePeriod>(OnTimeOfDayChanged));
		if (m_scenePowerVariable != null)
		{
			m_scenePowerVariable.RegisterListener(OnPowerVariableChanged);
		}
	}

	private void OnDestroy()
	{
		CircuitSetup[] startingStates = m_startingStates;
		for (int i = 0; i < startingStates.Length; i++)
		{
			CircuitSetup circuitSetup = startingStates[i];
			if (circuitSetup.m_startingStateVariable != null)
			{
				circuitSetup.m_startingStateVariable.UnregisterListener(OnStartingStateVariableChanged);
			}
		}
		m_anchor.Set(null);
		TimeOfDay timeOfDay = GlobalReferences.Instance.TimeOfDay;
		timeOfDay.m_onTimeOfDayChanged = (UnityAction<TimeOfDay.TimePeriod>)Delegate.Remove(timeOfDay.m_onTimeOfDayChanged, new UnityAction<TimeOfDay.TimePeriod>(OnTimeOfDayChanged));
		if (m_scenePowerVariable != null)
		{
			m_scenePowerVariable.UnregisterListener(OnPowerVariableChanged);
		}
	}

	private void OnStartingStateVariableChanged(bool obj)
	{
		CircuitSetup[] startingStates = m_startingStates;
		for (int i = 0; i < startingStates.Length; i++)
		{
			CircuitSetup circuitSetup = startingStates[i];
			if (circuitSetup.m_startingStateVariable != null)
			{
				bool state = GetState(circuitSetup.m_id);
				bool value = circuitSetup.m_startingStateVariable.Value;
				if (state != value)
				{
					SetState(circuitSetup.m_id, value, persistent: false);
				}
			}
		}
	}

	private void OnTimeOfDayChanged(TimeOfDay.TimePeriod arg0)
	{
		OnCircuitChanged?.Invoke(0, GetState(0));
	}

	public void SetState(int id, bool state, bool persistent)
	{
		if (m_circuitStates.ContainsKey(id))
		{
			m_circuitStates[id] = state;
			if (m_persistentData != null && persistent)
			{
				int index = m_persistentData.m_keys.IndexOf(id);
				m_persistentData.m_states[index] = state;
			}
		}
		else
		{
			m_circuitStates.Add(id, state);
			if (m_persistentData != null && persistent)
			{
				m_persistentData.m_keys.Add(id);
				m_persistentData.m_states.Add(state);
			}
		}
		OnCircuitChanged?.Invoke(id, state);
	}

	private void OnPowerVariableChanged(bool newPowerState)
	{
		OnScenePowerStateChanged?.Invoke(newPowerState);
	}

	public void ToggleState(int id)
	{
		SetState(id, !GetState(id), persistent: true);
	}

	public void EnablePowerState(int id)
	{
		SetState(id, state: true, persistent: true);
	}

	public void DisablePowerState(int id)
	{
		SetState(id, state: false, persistent: true);
	}

	public bool GetState(int id)
	{
		if (m_circuitStates.ContainsKey(id))
		{
			return m_circuitStates[id];
		}
		bool flag = false;
		LevelMetadata levelMetadata = null;
		LevelMetadataReference component = GetComponent<LevelMetadataReference>();
		levelMetadata = ((!(component != null)) ? GlobalReferences.Instance.Anchors.Generic.ActiveLevelMetadata.Item : component.Metadata);
		if (levelMetadata != null && levelMetadata.SpacialType == LevelMetadata.LevelSpacialType.Exterior && !GlobalReferences.Instance.TimeOfDay.IsNightTime())
		{
			flag = true;
		}
		if (!flag)
		{
			return true;
		}
		return false;
	}

	private ProgressionVariable GetVariableForState(int index)
	{
		CircuitSetup[] startingStates = m_startingStates;
		for (int i = 0; i < startingStates.Length; i++)
		{
			CircuitSetup circuitSetup = startingStates[i];
			if (circuitSetup.m_startingStateVariable != null && circuitSetup.m_id == index)
			{
				return circuitSetup.m_startingStateVariable;
			}
		}
		return null;
	}

	public bool IsScenePowered()
	{
		if (m_scenePowerVariable != null)
		{
			return m_scenePowerVariable.Value;
		}
		return true;
	}

	public void ReceiveDataStoreEntry(PersistentDataObject dataEntry)
	{
		m_persistentDataObject = dataEntry;
		m_persistentData = ((m_persistentDataObject.Data != null) ? (m_persistentDataObject.Data as PersistentData) : null);
		if (m_persistentData != null)
		{
			m_circuitStates = new Dictionary<int, bool>();
			for (int i = 0; i < m_persistentData.m_keys.Count; i++)
			{
				ProgressionVariable variableForState = GetVariableForState(m_persistentData.m_keys[i]);
				if (variableForState != null)
				{
					SetState(m_persistentData.m_keys[i], variableForState.Value, persistent: false);
				}
				else
				{
					SetState(m_persistentData.m_keys[i], m_persistentData.m_states[i], persistent: false);
				}
			}
			return;
		}
		m_persistentData = new PersistentData();
		m_persistentDataObject.Data = m_persistentData;
		m_persistentData.m_keys = new List<int>();
		m_persistentData.m_states = new List<bool>();
		foreach (KeyValuePair<int, bool> circuitState in m_circuitStates)
		{
			m_persistentData.m_keys.Add(circuitState.Key);
			m_persistentData.m_states.Add(circuitState.Value);
		}
	}

	public bool RequiresPersistentData()
	{
		return true;
	}
}
