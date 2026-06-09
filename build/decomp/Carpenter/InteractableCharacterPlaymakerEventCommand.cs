using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class InteractableCharacterPlaymakerEventCommand : BaseInteractableCommand
{
	[SerializeField]
	private string m_event;

	[SerializeField]
	private bool m_waitForCompletion = true;

	public override IEnumerator DoInteraction(BaseInteractable interactable, BaseInteractor interactor, InteractionResult result)
	{
		CharacterInteractor characterInteractor = interactor as CharacterInteractor;
		if (characterInteractor != null)
		{
			characterInteractor.InteractableCommandEventToSend = m_event;
			characterInteractor.InteractableCommandRequestFlag = CharacterInteractor.InteractableCommandFlag.RequestEvent;
			characterInteractor.GetComponent<PlayMakerFSM>().SendEvent(m_event);
			if (m_waitForCompletion)
			{
				yield return new WaitUntil(() => characterInteractor.InteractableCommandRequestFlag == CharacterInteractor.InteractableCommandFlag.EventDone);
			}
			characterInteractor.InteractableCommandRequestFlag = CharacterInteractor.InteractableCommandFlag.None;
		}
		else
		{
			Debug.LogError("Tried to use InteractableCharacterPlaymakerEventCommand on a non-character interactor!");
		}
	}
}
