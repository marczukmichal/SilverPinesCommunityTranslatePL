using System;
using System.Collections;

[Serializable]
public class InteractableSetMinigameCompleteCommand : BaseInteractableCommand
{
	public override IEnumerator DoInteraction(BaseInteractable baseInteractable, BaseInteractor interactor, InteractionResult result)
	{
		baseInteractable.gameObject.GetComponentInParent<MinigameScene>().SetMinigameCompleted();
		yield return null;
	}
}
