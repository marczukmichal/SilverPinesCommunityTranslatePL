using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class InteractableRevealMapCommand : BaseInteractableCommand
{
	[SerializeField]
	private MapRevealPickup m_mapRevealPickup;

	public override IEnumerator DoInteraction(BaseInteractable interactable, BaseInteractor interactor, InteractionResult result)
	{
		if (m_mapRevealPickup == null)
		{
			Debug.LogWarning("InteractableRevealMapCommand is missing a map reveal pickup, don't do anything");
			yield return null;
		}
		GlobalReferences.Instance.MapDynamicData.AddMapRevealPickup(m_mapRevealPickup);
		yield return null;
	}
}
