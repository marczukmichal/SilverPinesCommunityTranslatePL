using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class InteractablePhoneCoinSlotCommand : BaseInteractableCommand
{
	[SerializeField]
	private PhoneMinigame m_phone;

	public override IEnumerator DoInteraction(BaseInteractable interactable, BaseInteractor interactor, InteractionResult result)
	{
		yield return m_phone.DoPhoneCoinSlotCommand(interactable, interactor, result);
	}
}
