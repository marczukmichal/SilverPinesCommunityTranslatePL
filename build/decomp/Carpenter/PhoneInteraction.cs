using System.Collections;
using UnityEngine;

public class PhoneInteraction
{
	private bool m_finishedDialogue;

	private bool m_shouldExit;

	private PhoneNumber m_numberData;

	private PhoneBook m_phoneBook;

	private PhoneEvent m_phoneEvent;

	public PhoneInteraction(PhoneNumber numberData, PhoneEvent phoneEvent, PhoneBook phoneBook)
	{
		m_numberData = numberData;
		m_phoneBook = phoneBook;
		m_phoneEvent = phoneEvent;
	}

	public IEnumerator CallNumber(Transform speakerTransform, bool ignoreSave = false)
	{
		if (m_phoneBook != null && m_numberData != null)
		{
			m_phoneBook.RecordCalledNumber(m_numberData);
		}
		Dialogue dialogue;
		if (m_phoneEvent == null)
		{
			dialogue = m_phoneBook.InvalidCallDialogue;
		}
		else
		{
			dialogue = m_phoneEvent.m_dialogue;
			m_phoneEvent.m_onCalledEvent.Invoke();
			m_phoneBook.RecordHeardPhoneEvent(m_phoneEvent);
		}
		if (dialogue != null)
		{
			yield return PlayDialogue(dialogue, speakerTransform);
		}
		else
		{
			Debug.LogError("No comic or dialogue set for phone conversation " + m_numberData.ToString());
			OnComicFinished(inDialogue: false);
		}
		yield return new WaitUntil(() => m_shouldExit);
		Cleanup();
	}

	private IEnumerator PlayDialogue(Dialogue dialogue, Transform speakerTransform)
	{
		GlobalReferences.Instance.EventChannels.Gameplay.PhoneDialogue.Raise(value: true);
		m_finishedDialogue = false;
		GlobalReferences.Instance.EventChannels.Dialogue.TriggerDialogue.Raise(dialogue);
		GlobalReferences.Instance.EventChannels.Dialogue.DialogueEnterExit.Register(OnDialogueStateChanged);
		yield return new WaitUntil(() => m_finishedDialogue);
		GlobalReferences.Instance.EventChannels.Dialogue.DialogueEnterExit.Unregister(OnDialogueStateChanged);
		m_shouldExit = true;
		GlobalReferences.Instance.EventChannels.Gameplay.PhoneDialogue.Raise(value: false);
	}

	private void OnDialogueStateChanged(bool inDialogue)
	{
		if (!inDialogue)
		{
			m_finishedDialogue = true;
		}
	}

	public void Cleanup()
	{
		GlobalReferences.Instance.EventChannels.Dialogue.DialogueEnterExit.Unregister(OnDialogueStateChanged);
	}

	private void OnComicFinished(bool inDialogue)
	{
		if (!inDialogue)
		{
			GlobalReferences.Instance.EventChannels.Comic.ToggleComicViewActive.Unregister(OnComicFinished);
			m_shouldExit = true;
		}
	}
}
