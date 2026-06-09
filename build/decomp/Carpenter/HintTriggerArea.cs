using System;
using UnityEngine;
using UnityEngine.Events;

public class HintTriggerArea : MonoBehaviour, IPersistentComponent
{
	public enum ProgressionVariableBehavior
	{
		EnableIfSet,
		DisableIfSet
	}

	[Serializable]
	private class PersistentData
	{
		public bool m_hintResolved;
	}

	[SerializeField]
	private HintInfo m_hintInfo;

	[Header("Trigger Area")]
	[SerializeField]
	private PlayerBoundsTrigger m_triggerArea;

	[SerializeField]
	private float m_timeInAreaToTrigger;

	[Header("Timing & Restrictions")]
	[SerializeField]
	private ProgressionVariable m_progressionVariable;

	[SerializeField]
	private ProgressionVariableBehavior m_progVarBehavior;

	[SerializeField]
	private bool m_onlyOnce;

	private bool m_playerInArea;

	private bool m_hintActive;

	private float m_timeInArea;

	private bool m_hintResolved;

	private PersistentDataObject m_persistentDataObject;

	private PersistentData m_persistentData;

	private void Reset()
	{
		m_triggerArea.Size = new Vector2(5f, 5f);
		m_triggerArea.Offset = Vector2.zero;
	}

	private void OnEnable()
	{
		PlayerBoundsTrigger triggerArea = m_triggerArea;
		triggerArea.OnEnter = (UnityAction)Delegate.Combine(triggerArea.OnEnter, new UnityAction(OnPlayerEnter));
		PlayerBoundsTrigger triggerArea2 = m_triggerArea;
		triggerArea2.OnExit = (UnityAction)Delegate.Combine(triggerArea2.OnExit, new UnityAction(OnPlayerExit));
	}

	private void OnDisable()
	{
		PlayerBoundsTrigger triggerArea = m_triggerArea;
		triggerArea.OnEnter = (UnityAction)Delegate.Remove(triggerArea.OnEnter, new UnityAction(OnPlayerEnter));
		PlayerBoundsTrigger triggerArea2 = m_triggerArea;
		triggerArea2.OnExit = (UnityAction)Delegate.Remove(triggerArea2.OnExit, new UnityAction(OnPlayerExit));
		if (m_playerInArea)
		{
			OnPlayerExit();
		}
	}

	private void OnPlayerEnter()
	{
		m_playerInArea = true;
	}

	private void OnPlayerExit()
	{
		m_playerInArea = false;
		if (m_hintActive)
		{
			CancelHint();
		}
	}

	public void Update()
	{
		m_triggerArea.Update(base.transform);
		if (m_hintResolved)
		{
			return;
		}
		if (m_progressionVariable != null)
		{
			bool flag = false;
			if (m_progVarBehavior == ProgressionVariableBehavior.EnableIfSet && !m_progressionVariable.Value)
			{
				flag = true;
			}
			if (m_progVarBehavior == ProgressionVariableBehavior.DisableIfSet && m_progressionVariable.Value)
			{
				flag = true;
			}
			if (flag)
			{
				if (m_hintActive)
				{
					CancelHint();
				}
				return;
			}
		}
		if (m_playerInArea && !m_hintActive)
		{
			m_timeInArea += Time.deltaTime;
			if (m_timeInArea >= m_timeInAreaToTrigger)
			{
				TriggerHint();
			}
		}
		else
		{
			m_timeInArea = 0f;
		}
	}

	public void MarkResolved()
	{
		m_hintResolved = true;
		if (m_persistentData != null)
		{
			m_persistentData.m_hintResolved = true;
		}
	}

	private void TriggerHint()
	{
		GlobalReferences.Instance.Anchors.Hints.ActiveAreaHintInfoAnchor.Set(m_hintInfo);
		m_hintActive = true;
		if (m_onlyOnce)
		{
			MarkResolved();
		}
	}

	public void CancelHint()
	{
		GlobalReferences.Instance.Anchors.Hints.ActiveAreaHintInfoAnchor.Set(null);
		m_hintActive = false;
	}

	public void ReceiveDataStoreEntry(PersistentDataObject dataEntry)
	{
		m_persistentDataObject = dataEntry;
		m_persistentData = ((m_persistentDataObject.Data != null) ? (m_persistentDataObject.Data as PersistentData) : null);
		if (m_persistentData != null)
		{
			if (m_persistentData.m_hintResolved)
			{
				m_hintResolved = true;
			}
		}
		else
		{
			m_persistentData = new PersistentData();
			m_persistentDataObject.Data = m_persistentData;
		}
	}

	public bool RequiresPersistentData()
	{
		return true;
	}
}
