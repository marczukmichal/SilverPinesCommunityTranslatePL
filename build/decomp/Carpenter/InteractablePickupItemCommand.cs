using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class InteractablePickupItemCommand : BaseInteractableCommand
{
	[SerializeField]
	private ItemPickup m_itemPickup;

	private bool m_menuClosed;

	public override IEnumerator DoInteraction(BaseInteractable interactable, BaseInteractor interactor, InteractionResult result)
	{
		if (m_itemPickup == null)
		{
			Debug.LogWarning("InteractablePickupItemCommand is missing an item pickup, don't do anything");
			yield return null;
		}
		m_menuClosed = false;
		if (m_itemPickup.ItemPickedup)
		{
			yield break;
		}
		CharacterInventory component = interactor.GetComponent<CharacterInventory>();
		PlayerMainInventory playerMainInventory = ((!(component != null)) ? GlobalReferences.Instance.MainInventory : component.Inventory);
		bool flag = !playerMainInventory.HasSeenItemType(m_itemPickup.ItemDefinition);
		bool isImportantItem = m_itemPickup.ItemDefinition.IsImportantItem;
		bool flag2 = !playerMainInventory.CanAddItemToInventory(m_itemPickup);
		AppearsOnMap component2 = interactable.GetComponent<AppearsOnMap>();
		if (component2 != null)
		{
			component2.SetItemRevealed();
		}
		if (isImportantItem || flag || flag2)
		{
			GlobalReferences.Instance.Anchors.Inventory.ItemPickupInteractAnchor.Set(m_itemPickup);
			GlobalReferences.Instance.GameMenuState.SetInMenu(GameMenuState.GameMenu.ItemPickup);
			GameMenuState gameMenuState = GlobalReferences.Instance.GameMenuState;
			gameMenuState.OnGameMenuChanged = (UnityAction<GameMenuState.GameMenu>)Delegate.Combine(gameMenuState.OnGameMenuChanged, new UnityAction<GameMenuState.GameMenu>(OnGameMenuChanged));
			yield return new WaitUntil(() => m_menuClosed);
		}
		else
		{
			playerMainInventory.PickupItem(m_itemPickup);
		}
		if (m_itemPickup.ItemDefinition is InventoryUpgradeItemDefinition)
		{
			yield return new WaitForEndOfFrame();
			GlobalReferences.Instance.EventChannels.InGameMenu.ShowInventoryMenuTab.Raise();
		}
		OnExit();
		if (!m_itemPickup.ItemPickedup)
		{
			result.m_cancel = true;
		}
	}

	private void OnGameMenuChanged(GameMenuState.GameMenu menu)
	{
		if (!menu.HasFlag(GameMenuState.GameMenu.InGameMenu))
		{
			m_menuClosed = true;
		}
	}

	private void OnExit()
	{
		GlobalReferences.Instance.Anchors.Inventory.ItemPickupInteractAnchor.Set(null);
		GameMenuState gameMenuState = GlobalReferences.Instance.GameMenuState;
		gameMenuState.OnGameMenuChanged = (UnityAction<GameMenuState.GameMenu>)Delegate.Remove(gameMenuState.OnGameMenuChanged, new UnityAction<GameMenuState.GameMenu>(OnGameMenuChanged));
		GlobalReferences.Instance.GameMenuState.ClearInMenu(GameMenuState.GameMenu.ItemPickup);
	}

	public override void Cancel()
	{
		base.Cancel();
		OnExit();
	}
}
