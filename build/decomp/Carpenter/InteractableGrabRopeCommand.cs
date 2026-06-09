using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class InteractableGrabRopeCommand : BaseInteractableCommand
{
	[SerializeField]
	private ClimbableRope m_rope;

	public override IEnumerator DoInteraction(BaseInteractable interactable, BaseInteractor interactor, InteractionResult result)
	{
		CharacterTraversalUtils component = interactor.GetComponent<CharacterTraversalUtils>();
		component.AttachToRope(m_rope);
		interactor.GetComponent<PlayMakerFSM>().SendEvent("Rope/Grab");
		component.AttachToRope(m_rope);
		yield return null;
	}
}
