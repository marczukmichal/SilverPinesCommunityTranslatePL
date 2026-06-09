using System;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Objectives and Leads/Objective - Active")]
public class ActiveObjective : ScriptableObject
{
	[SerializeField]
	[ReadOnly]
	private MapObjectiveState m_activeObjectives;

	private UnityAction<MapObjectiveState> m_onActiveObjectiveChanged;

	private MapObjectiveState m_dirtyFlag;

	public MapObjectiveState ActiveObjectives => m_activeObjectives;

	public MapObjectiveState DirtyFlag => m_dirtyFlag;

	public void ClearDirtyFlag()
	{
		m_dirtyFlag = MapObjectiveState.None;
	}

	public void RegisterListener(UnityAction<MapObjectiveState> listener)
	{
		m_onActiveObjectiveChanged = (UnityAction<MapObjectiveState>)Delegate.Combine(m_onActiveObjectiveChanged, listener);
	}

	public void UnregisterListener(UnityAction<MapObjectiveState> listener)
	{
		m_onActiveObjectiveChanged = (UnityAction<MapObjectiveState>)Delegate.Remove(m_onActiveObjectiveChanged, listener);
	}

	public void Clear()
	{
		m_activeObjectives = MapObjectiveState.None;
	}

	public void ReadFromPersistentData(MapObjectiveState objectiveState)
	{
		m_activeObjectives = objectiveState;
		m_dirtyFlag = MapObjectiveState.None;
	}

	public void SetObjective(MapObjectiveState objective)
	{
		MapObjectiveState activeObjectives = m_activeObjectives;
		m_activeObjectives |= objective;
		if (activeObjectives != m_activeObjectives)
		{
			m_dirtyFlag = objective;
			m_onActiveObjectiveChanged?.Invoke(objective);
		}
	}

	public void ClearObjective(MapObjectiveState objective)
	{
		MapObjectiveState activeObjectives = m_activeObjectives;
		m_activeObjectives &= ~objective;
		if (activeObjectives != m_activeObjectives)
		{
			m_dirtyFlag = MapObjectiveState.None;
			m_onActiveObjectiveChanged?.Invoke(objective);
		}
	}

	public bool IsObjectiveActive(MapObjectiveState objective)
	{
		return m_activeObjectives.HasFlag(objective);
	}
}
