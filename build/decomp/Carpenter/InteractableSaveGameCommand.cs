using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class InteractableSaveGameCommand : BaseInteractableCommand
{
	public PhoneBook m_phoneBook;

	public PhoneEvent m_phoneEvent;

	public override IEnumerator DoInteraction(BaseInteractable interactable, BaseInteractor interactor, InteractionResult result)
	{
		if (m_phoneBook != null && m_phoneEvent != null)
		{
			m_phoneBook.SetPhoneSaveEvent(m_phoneEvent);
		}
		GlobalReferences.Instance.EventChannels.SaveLoad.ShowSaveGamePanel.Raise();
		yield return new WaitForSecondsRealtime(0.1f);
		yield return new WaitUntil(() => GlobalReferences.Instance.GameMenuState.IsInMenu(GameMenuState.GameMenu.GameSaving));
	}
}
