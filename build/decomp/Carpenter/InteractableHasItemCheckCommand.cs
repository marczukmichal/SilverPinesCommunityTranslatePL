using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Localization;

[Serializable]
public class InteractableHasItemCheckCommand : BaseInteractableCommand
{
	[SerializeField]
	private ItemDefinition m_requiredItemType;

	[SerializeField]
	private LocalizedString m_needItemString;

	private InteractableExaminableCommand m_subCommand;

	public ItemDefinition ItemDefinition => m_requiredItemType;

	public override IEnumerator DoInteraction(BaseInteractable interactable, BaseInteractor interactor, InteractionResult result)
	{
		bool flag = false;
		CharacterInventory component = interactor.GetComponent<CharacterInventory>();
		if (component != null && component.Inventory.GetItemOfType(m_requiredItemType) != null)
		{
			flag = true;
		}
		if (!flag)
		{
			m_subCommand = InteractableExaminableCommand.CreateInstance(m_needItemString, isChoice: false);
			m_subCommand.SetupInteraction(interactable, interactor);
			yield return m_subCommand.DoInteraction(interactable, interactor, result);
			m_subCommand.FinishInteraction(interactable, interactor);
			result.m_cancel = true;
		}
	}

	public override void Cancel()
	{
		base.Cancel();
		if (m_subCommand != null)
		{
			m_subCommand.Cancel();
			m_subCommand = null;
		}
	}
}
