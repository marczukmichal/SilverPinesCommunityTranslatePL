using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(PersistentDataIdentifier))]
[DisallowMultipleComponent]
[ShowInDesignerInspector]
[AddComponentMenu("Gameplay/Persistent Scene State Events")]
public class PersistentSceneTriggerEvents : MonoBehaviour, IPersistentComponent
{
	[Serializable]
	private class PersistentData
	{
		public bool m_flagSet;
	}

	[Header("Flag Set Events")]
	[Tooltip("Will trigger when state is set to true, either on trigger or reload")]
	[SerializeField]
	private UnityEvent m_onSet;

	[Tooltip("Will trigger when state is set to true, only on trigger event")]
	[SerializeField]
	private UnityEvent m_onSetTransition;

	[Tooltip("Will trigger when state is set to true, only on reloading the scene")]
	[SerializeField]
	private UnityEvent m_onSetReload;

	[Header("Flag Not Set Events")]
	[Tooltip("Will trigger when state is set to false, either on trigger or reload")]
	[SerializeField]
	private UnityEvent m_onNotSet;

	[Tooltip("Will trigger when state is set to false, only on trigger event")]
	[SerializeField]
	private UnityEvent m_onNotSetTransition;

	[Tooltip("Will trigger when state is set to false, only on reloading the scene")]
	[SerializeField]
	private UnityEvent m_onNotSetReload;

	[Header("Toggle GameObjects")]
	[SerializeField]
	private List<GameObject> m_enableGameObjects;

	[SerializeField]
	private List<GameObject> m_disableGameObjects;

	[Header("Initial State")]
	[SerializeField]
	private bool m_initialValue;

	private bool m_isSet;

	private PersistentDataObject m_persistentDataObject;

	private PersistentData m_persistentData;

	public bool IsSet => m_isSet;

	public void SetState(bool set)
	{
		if (m_persistentData == null)
		{
			PersistentDataIdentifier component = GetComponent<PersistentDataIdentifier>();
			if (component != null)
			{
				component.UpdateFromDatastore();
			}
		}
		if (m_persistentData == null)
		{
			Debug.LogError("Missing PersistentDataIdentifier for PersistentSceneTriggerEvents on " + base.gameObject.name);
		}
		if (m_isSet != set)
		{
			if (GameDebugCommands.PROG_VAR_LOG_CHANGES)
			{
				Debug.Log("<color=yellow>PersistentSceneTriggerEvents " + base.name + " is now " + set + "</color>");
			}
			m_isSet = set;
			m_persistentData.m_flagSet = set;
			if (set)
			{
				m_onSetTransition.Invoke();
				m_onSet.Invoke();
			}
			else
			{
				m_onNotSetTransition.Invoke();
				m_onNotSet.Invoke();
			}
			ToggleGameObjects();
		}
	}

	private void ToggleGameObjects()
	{
		if (m_enableGameObjects != null)
		{
			foreach (GameObject enableGameObject in m_enableGameObjects)
			{
				if (!(enableGameObject == null))
				{
					enableGameObject.SetActive(m_isSet);
				}
			}
		}
		if (m_disableGameObjects == null)
		{
			return;
		}
		foreach (GameObject disableGameObject in m_disableGameObjects)
		{
			if (!(disableGameObject == null))
			{
				disableGameObject.SetActive(!m_isSet);
			}
		}
	}

	public void ReceiveDataStoreEntry(PersistentDataObject dataEntry)
	{
		m_persistentDataObject = dataEntry;
		m_persistentData = ((m_persistentDataObject.Data != null) ? (m_persistentDataObject.Data as PersistentData) : null);
		if (m_persistentData != null)
		{
			m_isSet = m_persistentData.m_flagSet;
		}
		else
		{
			m_persistentData = new PersistentData();
			m_persistentDataObject.Data = m_persistentData;
			m_isSet = (m_persistentData.m_flagSet = m_initialValue);
		}
		if (m_isSet)
		{
			m_onSetReload.Invoke();
			m_onSet.Invoke();
		}
		else
		{
			m_onNotSetReload.Invoke();
			m_onNotSet.Invoke();
		}
		ToggleGameObjects();
	}

	public bool RequiresPersistentData()
	{
		return true;
	}
}
