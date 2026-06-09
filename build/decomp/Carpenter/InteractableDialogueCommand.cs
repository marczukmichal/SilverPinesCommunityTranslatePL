using System;
using System.Collections;
using UnityEngine;

public class InteractableDialogueCommand : BaseInteractableCommand, IPersistentComponent
{
	[Serializable]
	private class PersistentData
	{
		public int m_dialogueIndex;
	}

	[SerializeField]
	private PlayableDialogue[] m_dialogues;

	[SerializeField]
	private EndLoopSettings m_endLoopSettings;

	[SerializeField]
	private bool m_disableTalkAnimationEvents;

	private bool m_dialogueFinished;

	private int m_dialogueIndex;

	private PlayMakerFSM m_fsm;

	private GameObject m_interactorGameObject;

	private PersistentDataObject m_persistentDataObject;

	private PersistentData m_persistentData;

	private int DialogueIndex
	{
		get
		{
			return m_dialogueIndex;
		}
		set
		{
			m_dialogueIndex = value;
			if (m_persistentData != null)
			{
				m_persistentData.m_dialogueIndex = m_dialogueIndex;
			}
		}
	}

	private void CharacterPlaymakerTalkEvent(GameObject player, bool start)
	{
		if (m_disableTalkAnimationEvents)
		{
			return;
		}
		if (m_fsm != null)
		{
			m_fsm.SendEvent(start ? "Talk/Start" : "Talk/Stop");
		}
		if (player != null)
		{
			PlayMakerFSM component = player.GetComponent<PlayMakerFSM>();
			if (component != null)
			{
				component.SendEvent(start ? "Talk/Start" : "Talk/Stop");
			}
		}
	}

	private PlayableDialogue GetDialogue()
	{
		int dialogueIndex = DialogueIndex;
		if (m_dialogues.Length == 0 && !m_endLoopSettings.HasEndLoopDialogue())
		{
			return null;
		}
		if (dialogueIndex < m_dialogues.Length)
		{
			return m_dialogues[dialogueIndex];
		}
		if (m_endLoopSettings != null && m_endLoopSettings.HasEndLoopDialogue())
		{
			int endLoopIndex = DialogueIndex - m_dialogues.Length;
			return m_endLoopSettings.GetDialogue(endLoopIndex);
		}
		return m_dialogues[m_dialogues.Length - 1];
	}

	public override IEnumerator DoInteraction(BaseInteractable interactable, BaseInteractor interactor, InteractionResult result)
	{
		m_dialogueFinished = false;
		m_fsm = interactable.gameObject.GetComponentInParent<PlayMakerFSM>();
		m_interactorGameObject = interactor.gameObject;
		PlayableDialogue dialogue = GetDialogue();
		if (dialogue != null && (bool)dialogue.m_dialogue)
		{
			dialogue.m_settings.m_onDialogueStarted.Invoke();
			GlobalReferences.Instance.EventChannels.Dialogue.TriggerDialogue.Raise(dialogue.m_dialogue);
			GlobalReferences.Instance.EventChannels.Dialogue.DialogueEnterExit.Register(OnDialogueStateChanged);
			if (DialogueIndex < m_dialogues.Length - 1 && !dialogue.m_settings.m_forceContinue)
			{
				result.m_cancel = true;
			}
			yield return new WaitForEndOfFrame();
			CharacterPlaymakerTalkEvent(m_interactorGameObject, start: true);
			yield return new WaitUntil(() => m_dialogueFinished);
			GlobalReferences.Instance.EventChannels.Dialogue.DialogueEnterExit.Unregister(OnDialogueStateChanged);
		}
		else
		{
			Debug.LogError("Tried to talk but character has no dialogue set! Check setup for character (" + interactable.gameObject.name + ") in scene (" + interactable.gameObject.scene.name + ")");
		}
	}

	private void OnDialogueStateChanged(bool state)
	{
		if (!state)
		{
			GetDialogue().m_settings.m_onDialogueFinished.Invoke();
			DialogueIndex++;
			GlobalReferences.Instance.EventChannels.Dialogue.DialogueEnterExit.Unregister(OnDialogueStateChanged);
			CharacterPlaymakerTalkEvent(m_interactorGameObject, start: false);
			m_dialogueFinished = true;
		}
	}

	public override bool RequiresPersistentData()
	{
		return true;
	}

	public void ReceiveDataStoreEntry(PersistentDataObject dataEntry)
	{
		m_persistentDataObject = dataEntry;
		m_persistentData = ((m_persistentDataObject.Data != null) ? (m_persistentDataObject.Data as PersistentData) : null);
		if (m_persistentData != null)
		{
			m_dialogueIndex = m_persistentData.m_dialogueIndex;
			return;
		}
		m_persistentData = new PersistentData();
		m_persistentDataObject.Data = m_persistentData;
	}
}
