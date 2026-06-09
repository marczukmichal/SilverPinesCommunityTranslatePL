using System;
using HutongGames.PlayMaker;
using UnityEngine;

[DisallowMultipleComponent]
[ShowInDesignerInspector]
public class PlaymakerFSMPersistent : MonoBehaviour, IPersistentComponent
{
	[Serializable]
	public struct StartStateOption
	{
		[SerializeField]
		public string m_startingEvent;

		[SerializeField]
		public Sprite m_spriteToSet;
	}

	[Serializable]
	private class PersistentData
	{
		public bool m_hasClearedInitialEvent;

		public string m_fsmState;
	}

	[SerializeField]
	private PlayMakerFSM m_playmakerFSM;

	[SerializeField]
	private string m_startingEvent;

	[SerializeField]
	private bool m_autoRestoreState;

	private PersistentDataObject m_persistentDataObject;

	private PersistentData m_persistentData;

	public string StartingEvent => m_startingEvent;

	private void Reset()
	{
		m_playmakerFSM = GetComponent<PlayMakerFSM>();
	}

	private void OnEnable()
	{
		if (m_playmakerFSM != null)
		{
			Fsm fsm = m_playmakerFSM.Fsm;
			fsm.StateChanged = (Action<FsmState>)Delegate.Combine(fsm.StateChanged, new Action<FsmState>(OnStateChanged));
		}
	}

	private void OnDisable()
	{
		if (m_playmakerFSM != null)
		{
			Fsm fsm = m_playmakerFSM.Fsm;
			fsm.StateChanged = (Action<FsmState>)Delegate.Remove(fsm.StateChanged, new Action<FsmState>(OnStateChanged));
		}
	}

	private void OnStateChanged(FsmState state)
	{
		if (m_persistentData != null)
		{
			m_persistentData.m_fsmState = state.Name;
		}
	}

	public void ClearStartingEvent()
	{
		m_startingEvent = "";
		if (m_persistentData != null)
		{
			m_persistentData.m_hasClearedInitialEvent = true;
		}
	}

	public bool RequiresPersistentData()
	{
		return true;
	}

	public void ReceiveDataStoreEntry(PersistentDataObject dataEntry)
	{
		m_persistentDataObject = dataEntry;
		m_persistentData = ((m_persistentDataObject.Data != null) ? (m_persistentDataObject.Data as PersistentData) : null);
		if (m_persistentData != null)
		{
			if (m_persistentData.m_hasClearedInitialEvent)
			{
				m_startingEvent = "";
			}
			if (m_autoRestoreState && !string.IsNullOrEmpty(m_persistentData.m_fsmState))
			{
				m_playmakerFSM.SetState(m_persistentData.m_fsmState);
			}
		}
		else
		{
			m_persistentData = new PersistentData();
			m_persistentDataObject.Data = m_persistentData;
		}
	}
}
