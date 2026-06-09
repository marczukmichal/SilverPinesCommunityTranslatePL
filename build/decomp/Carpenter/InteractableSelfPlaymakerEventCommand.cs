using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class InteractableSelfPlaymakerEventCommand : BaseInteractableCommand
{
	[SerializeField]
	private PlayMakerFSM m_playmakerFSM;

	[SerializeField]
	private string m_event;

	[SerializeField]
	private bool m_waitForCompletion = true;

	public override IEnumerator DoInteraction(BaseInteractable baseInteractable, BaseInteractor interactor, InteractionResult result)
	{
		if (!(baseInteractable is Interactable))
		{
			Debug.LogError("Tried to use InteractableSelfPlaymakerEventCommand with a non Interactable component!" + baseInteractable.name + " This is not supported!");
			yield return null;
		}
		Interactable interactable = baseInteractable as Interactable;
		interactable.FSMEventDone = false;
		m_playmakerFSM.SendEvent(m_event);
		if (m_waitForCompletion)
		{
			yield return new WaitUntil(() => interactable.FSMEventDone);
		}
	}
}
