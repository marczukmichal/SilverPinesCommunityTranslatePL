using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class InteractableWaitCommand : BaseInteractableCommand
{
	[SerializeField]
	private float m_waitTime = 1f;

	public override IEnumerator DoInteraction(BaseInteractable interactable, BaseInteractor interactor, InteractionResult result)
	{
		yield return new WaitForSeconds(m_waitTime);
	}
}
