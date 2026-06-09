using System;
using System.Collections;

[Serializable]
public class InteractableDisableSelfCommand : BaseInteractableCommand
{
	public override IEnumerator DoInteraction(BaseInteractable interactable, BaseInteractor interactor, InteractionResult result)
	{
		yield return null;
		result.m_disableGameObject = true;
	}
}
