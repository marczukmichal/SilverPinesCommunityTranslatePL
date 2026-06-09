using System.Collections;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
	[SerializeField]
	private float m_skipAutoplaySpeed = 0.15f;

	private int m_currentLineIndex;

	private Dialogue m_activeDialogue;

	private Coroutine m_skipCoroutine;

	private void OnEnable()
	{
		GlobalReferences.Instance.EventChannels.Dialogue.TriggerDialogue.Register(SetDialogueActive);
		GlobalReferences.Instance.EventChannels.Dialogue.ProgressDialogue.Register(ProgressDialogueEvent);
		GlobalReferences.Instance.EventChannels.Dialogue.SkipDialogue.Register(SkipDialogue);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.Dialogue.TriggerDialogue.Unregister(SetDialogueActive);
		GlobalReferences.Instance.EventChannels.Dialogue.ProgressDialogue.Unregister(ProgressDialogueEvent);
		GlobalReferences.Instance.EventChannels.Dialogue.SkipDialogue.Unregister(SkipDialogue);
	}

	private void SetDialogueActive(Dialogue dialogue)
	{
		Debug.Log("Set dialogue active " + dialogue.name);
		m_activeDialogue = dialogue;
		m_currentLineIndex = 0;
		GlobalReferences.Instance.EventChannels.Player.TogglePlayerInput.Raise(value: false);
		GlobalReferences.Instance.EventChannels.Dialogue.DialogueEnterExit.Raise(value: true);
		MusicSettings musicSettings = dialogue.GetMusicSettings();
		if (musicSettings != null)
		{
			GlobalReferences.Instance.EventChannels.Audio.PlayMusicOverride.Raise(musicSettings);
		}
		ShowDialogueLine(dialogue.GetLine(m_currentLineIndex), instant: false);
		GlobalReferences.Instance.GameMenuState.SetInMenu(GameMenuState.GameMenu.Dialogue);
	}

	private void ShowDialogueLine(DialogueLine line, bool instant)
	{
		DialogueEventData dialogueEventData = new DialogueEventData();
		dialogueEventData.m_show = true;
		dialogueEventData.m_dialogue = m_activeDialogue;
		dialogueEventData.m_dialogueLine = line;
		dialogueEventData.m_instant = instant;
		dialogueEventData.m_speakerGameObject = CharacterIdentifier.FindGameObjectForSpeakerSettings(line.SpeakerSettings);
		GlobalReferences.Instance.EventChannels.Dialogue.Dialogue.Raise(dialogueEventData);
	}

	private void EndDialogue()
	{
		GlobalReferences.Instance.EventChannels.Player.TogglePlayerInput.Raise(value: true);
		GlobalReferences.Instance.EventChannels.Dialogue.DialogueEnterExit.Raise(value: false);
		GlobalReferences.Instance.EventChannels.Audio.PlayMusicOverride.Raise(null);
		DialogueEventData dialogueEventData = new DialogueEventData();
		dialogueEventData.m_show = false;
		GlobalReferences.Instance.EventChannels.Dialogue.Dialogue.Raise(dialogueEventData);
		m_activeDialogue = null;
		if (m_skipCoroutine != null)
		{
			StopCoroutine(m_skipCoroutine);
			m_skipCoroutine = null;
		}
		GlobalReferences.Instance.GameMenuState.ClearInMenu(GameMenuState.GameMenu.Dialogue);
	}

	private void ProgressDialogueEvent()
	{
		ProgressDialogue(instant: false);
	}

	private void ProgressDialogue(bool instant)
	{
		if (m_currentLineIndex + 1 < m_activeDialogue.LineCount)
		{
			m_currentLineIndex++;
			ShowDialogueLine(m_activeDialogue.GetLine(m_currentLineIndex), instant);
		}
		else
		{
			EndDialogue();
		}
	}

	private void SkipDialogue()
	{
		if ((bool)m_activeDialogue && m_skipCoroutine == null)
		{
			m_skipCoroutine = StartCoroutine(SkipCoroutine());
		}
	}

	private IEnumerator SkipCoroutine()
	{
		while (m_activeDialogue != null)
		{
			yield return new WaitForSeconds(m_skipAutoplaySpeed);
			ProgressDialogue(instant: true);
		}
		m_skipCoroutine = null;
	}
}
