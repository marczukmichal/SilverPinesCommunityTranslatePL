using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
[ShowInDesignerInspector]
public class InteractablePickupMoneyCommand : BaseInteractableCommand, IPersistentComponent, IItemPickup
{
	[Serializable]
	private class PersistentData
	{
		public bool m_pickedUp;
	}

	[SerializeField]
	private int m_moneyAmount;

	private PersistentDataObject m_persistentDataObject;

	private PersistentData m_persistentData;

	private bool m_menuClosed;

	public bool MoneyPickedUp => m_moneyAmount == 0;

	public ItemDefinition ItemDefinition => null;

	public int ItemAmount
	{
		get
		{
			return m_moneyAmount;
		}
		set
		{
			m_moneyAmount = value;
			if (m_moneyAmount == 0 && m_persistentData != null)
			{
				m_persistentData.m_pickedUp = true;
			}
		}
	}

	public int AmountInItem => 1;

	public LootType LootType => LootType.Money;

	public bool SuppressNotification => false;

	public override bool RequiresPersistentData()
	{
		return true;
	}

	public void ReceiveDataStoreEntry(PersistentDataObject dataEntry)
	{
		m_persistentDataObject = dataEntry;
		m_persistentData = ((m_persistentDataObject.Data != null) ? (m_persistentDataObject.Data as PersistentData) : null);
		if (m_persistentData != null && m_persistentData.m_pickedUp)
		{
			m_moneyAmount = 0;
		}
	}

	public override IEnumerator DoInteraction(BaseInteractable interactable, BaseInteractor interactor, InteractionResult result)
	{
		m_menuClosed = false;
		yield return new WaitForSeconds(0.25f);
		CharacterInventory inventory = interactor.GetComponent<CharacterInventory>();
		bool flag = false;
		if (inventory != null)
		{
			flag = !inventory.Inventory.HasSeenMoney();
		}
		if (flag)
		{
			GlobalReferences.Instance.Anchors.Inventory.ItemPickupInteractAnchor.Set(this);
			GlobalReferences.Instance.EventChannels.InGameMenu.ShowInventoryMenuTab.Raise();
			GameMenuState gameMenuState = GlobalReferences.Instance.GameMenuState;
			gameMenuState.OnGameMenuChanged = (UnityAction<GameMenuState.GameMenu>)Delegate.Combine(gameMenuState.OnGameMenuChanged, new UnityAction<GameMenuState.GameMenu>(OnGameMenuChanged));
			yield return new WaitUntil(() => m_menuClosed);
		}
		else
		{
			GlobalReferences.Instance.EventChannels.Inventory.AddPlayerMoney.Raise(m_moneyAmount);
			ItemAmount = 0;
		}
		if (!MoneyPickedUp)
		{
			result.m_cancel = true;
		}
		if (inventory != null)
		{
			inventory.Inventory.SetHasPickedUpMoney();
		}
		OnExit();
	}

	private void OnGameMenuChanged(GameMenuState.GameMenu menu)
	{
		if (!menu.HasFlag(GameMenuState.GameMenu.InGameMenu))
		{
			m_menuClosed = true;
		}
	}

	public override void Cancel()
	{
		base.Cancel();
		OnExit();
	}

	private void OnExit()
	{
		GlobalReferences.Instance.Anchors.Inventory.ItemPickupInteractAnchor.Set(null);
		GameMenuState gameMenuState = GlobalReferences.Instance.GameMenuState;
		gameMenuState.OnGameMenuChanged = (UnityAction<GameMenuState.GameMenu>)Delegate.Remove(gameMenuState.OnGameMenuChanged, new UnityAction<GameMenuState.GameMenu>(OnGameMenuChanged));
		GlobalReferences.Instance.GameMenuState.ClearInMenu(GameMenuState.GameMenu.InGameMenu);
	}
}
