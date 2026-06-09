using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class InteractablePickupCollectableCommand : BaseInteractableCommand
{
	[SerializeField]
	private CollectableDefinition m_collectableDefinition;

	public override IEnumerator DoInteraction(BaseInteractable interactable, BaseInteractor interactor, InteractionResult result)
	{
		GlobalReferences.Instance.EventChannels.Collectables.PickupCollectableDefinition.Raise(m_collectableDefinition);
		yield return null;
	}
}
