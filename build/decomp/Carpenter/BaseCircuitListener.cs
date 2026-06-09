using System;
using UnityEngine;
using UnityEngine.Events;

public abstract class BaseCircuitListener : MonoBehaviour
{
	private enum CircuitState
	{
		Unitialised,
		Powered,
		Unpowered
	}

	[Header("Circuit Settings")]
	[SerializeField]
	protected CircuitType m_listenerType;

	[SerializeField]
	protected int m_index;

	[Header("Simple Circuit Settings")]
	[SerializeField]
	private SimpleCircuit m_simpleCircuit;

	private CircuitState m_circuitState;

	private void OnEnable()
	{
		CircuitManager item = GlobalReferences.Instance.Anchors.Gameplay.CircuitManagerAnchor.Item;
		if (item != null)
		{
			item.OnScenePowerStateChanged = (UnityAction<bool>)Delegate.Combine(item.OnScenePowerStateChanged, new UnityAction<bool>(OnScenePowerChanged));
		}
		else
		{
			GlobalReferences.Instance.Anchors.Gameplay.CircuitManagerAnchor.Register(OnCircuitControllerRegistered);
		}
		if (m_listenerType == CircuitType.Local)
		{
			SimpleCircuit simpleCircuit = m_simpleCircuit;
			simpleCircuit.OnCircuitChanged = (UnityAction<bool>)Delegate.Combine(simpleCircuit.OnCircuitChanged, new UnityAction<bool>(OnSceneCircuitChanged));
			OnSceneCircuitChanged(m_simpleCircuit.GetState());
		}
		else if (item != null)
		{
			item.OnCircuitChanged = (UnityAction<int, bool>)Delegate.Combine(item.OnCircuitChanged, new UnityAction<int, bool>(OnSceneCircuitChanged));
			OnSceneCircuitChanged(m_index, item.GetState(m_index));
		}
	}

	private void OnCircuitControllerRegistered(CircuitManager circuitManager)
	{
		GlobalReferences.Instance.Anchors.Gameplay.CircuitManagerAnchor.Unregister(OnCircuitControllerRegistered);
		if (!(circuitManager == null))
		{
			if (m_listenerType == CircuitType.Scene)
			{
				circuitManager.OnCircuitChanged = (UnityAction<int, bool>)Delegate.Combine(circuitManager.OnCircuitChanged, new UnityAction<int, bool>(OnSceneCircuitChanged));
				OnSceneCircuitChanged(m_index, circuitManager.GetState(m_index));
			}
			circuitManager.OnScenePowerStateChanged = (UnityAction<bool>)Delegate.Combine(circuitManager.OnScenePowerStateChanged, new UnityAction<bool>(OnScenePowerChanged));
			if (!circuitManager.IsScenePowered())
			{
				OnScenePowerChanged(newScenePower: false);
			}
		}
	}

	private void OnDisable()
	{
		CircuitManager item = GlobalReferences.Instance.Anchors.Gameplay.CircuitManagerAnchor.Item;
		if (item != null)
		{
			item.OnScenePowerStateChanged = (UnityAction<bool>)Delegate.Remove(item.OnScenePowerStateChanged, new UnityAction<bool>(OnScenePowerChanged));
		}
		if (m_listenerType == CircuitType.Local)
		{
			SimpleCircuit simpleCircuit = m_simpleCircuit;
			simpleCircuit.OnCircuitChanged = (UnityAction<bool>)Delegate.Remove(simpleCircuit.OnCircuitChanged, new UnityAction<bool>(OnSceneCircuitChanged));
		}
		else if (item != null)
		{
			item.OnCircuitChanged = (UnityAction<int, bool>)Delegate.Remove(item.OnCircuitChanged, new UnityAction<int, bool>(OnSceneCircuitChanged));
		}
		GlobalReferences.Instance.Anchors.Gameplay.CircuitManagerAnchor.Unregister(OnCircuitControllerRegistered);
	}

	private void OnSceneCircuitChanged(int circuitID, bool state)
	{
		if (m_index == circuitID)
		{
			SetCircuitState(state ? CircuitState.Powered : CircuitState.Unpowered);
		}
	}

	private void OnSceneCircuitChanged(bool state)
	{
		SetCircuitState(state ? CircuitState.Powered : CircuitState.Unpowered);
	}

	private void SetCircuitState(CircuitState state)
	{
		if (m_circuitState != state)
		{
			m_circuitState = state;
			DoStateUpdate();
		}
	}

	private void OnScenePowerChanged(bool newScenePower)
	{
		DoStateUpdate();
	}

	private void DoStateUpdate()
	{
		switch (m_circuitState)
		{
		case CircuitState.Powered:
		{
			CircuitManager item = GlobalReferences.Instance.Anchors.Gameplay.CircuitManagerAnchor.Item;
			bool flag = true;
			if (item != null)
			{
				flag = item.IsScenePowered();
			}
			if (flag)
			{
				OnPowered();
			}
			else
			{
				OnUnpowered();
			}
			break;
		}
		case CircuitState.Unpowered:
			OnUnpowered();
			break;
		}
	}

	protected abstract void OnPowered();

	protected abstract void OnUnpowered();
}
