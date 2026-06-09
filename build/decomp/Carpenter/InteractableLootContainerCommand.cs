using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Serialization;

[DisallowMultipleComponent]
public class InteractableLootContainerCommand : BaseInteractableCommand, IPersistentComponent, IItemPickup
{
	public enum LootContainerState
	{
		Closed,
		Open
	}

	[Serializable]
	private class PersistentData
	{
		public bool m_generatedLoot;

		public Loot m_loot;
	}

	[SerializeField]
	private PlayMakerFSM m_playmakerFSM;

	[Header("Loot Settings")]
	[SerializeField]
	private LootTableSettings m_lootTable;

	[Header("Force Item spawn")]
	[FormerlySerializedAs("m_itemToSpawn")]
	[SerializeField]
	private AssetReference m_forcedItemToSpawn;

	[SerializeField]
	private int m_forcedItemAmount = 1;

	[Header("Notification")]
	[SerializeField]
	private bool m_suppressNotification;

	private AsyncOperationHandle<ItemDefinition> m_loadedAssetHandle;

	private bool m_menuClosed;

	private LootContainerState m_containerState;

	private PersistentDataObject m_persistentDataObject;

	private PersistentData m_persistentData;

	public LootTableSettings LootTable => m_lootTable;

	public int AmountInItem => -1;

	public LootContainerState State => m_containerState;

	public LootType LootType
	{
		get
		{
			if (m_persistentData != null && m_persistentData.m_generatedLoot)
			{
				return m_persistentData.m_loot.m_lootItemType;
			}
			return LootType.None;
		}
	}

	public ItemDefinition ItemDefinition
	{
		get
		{
			if (!m_loadedAssetHandle.IsValid())
			{
				return null;
			}
			return m_loadedAssetHandle.Result;
		}
	}

	public int ItemAmount
	{
		get
		{
			if (m_persistentData != null)
			{
				return m_persistentData.m_loot.m_amount;
			}
			return 0;
		}
		set
		{
			if (m_persistentData != null)
			{
				m_persistentData.m_loot.m_amount = value;
			}
			else
			{
				Debug.LogError("Can't change loot amount in LootContainerInteractable as it has no persistent data!");
			}
		}
	}

	public bool SuppressNotification => m_suppressNotification;

	private void SetContainerState(LootContainerState newState, bool instant = false)
	{
		m_containerState = newState;
		if (m_playmakerFSM != null)
		{
			switch (newState)
			{
			case LootContainerState.Open:
				m_playmakerFSM.SendEvent(instant ? "Open_Instant" : "Open");
				break;
			case LootContainerState.Closed:
				m_playmakerFSM.SendEvent(instant ? "Close_Instant" : "Close");
				break;
			}
		}
	}

	public override IEnumerator DoInteraction(BaseInteractable interactable, BaseInteractor interactor, InteractionResult result)
	{
		m_menuClosed = false;
		if (!m_persistentData.m_generatedLoot)
		{
			GenerateLoot();
		}
		SetContainerState(LootContainerState.Open);
		AppearsOnMap aom = interactable.GetComponent<AppearsOnMap>();
		yield return new WaitForSeconds(0.25f);
		if (LootType == LootType.ItemDefinition || LootType == LootType.Money)
		{
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			CharacterInventory component = interactor.GetComponent<CharacterInventory>();
			if (aom != null)
			{
				aom.SetDiscovered();
				aom.SetItemRevealed();
			}
			if (component != null)
			{
				if (LootType == LootType.Money)
				{
					flag = !component.Inventory.HasSeenMoney();
				}
				else if (LootType == LootType.ItemDefinition)
				{
					flag = !component.Inventory.HasSeenItemType(ItemDefinition);
					flag2 = ItemDefinition.IsImportantItem;
				}
				flag3 = LootType == LootType.ItemDefinition && !component.Inventory.CanAddItemToInventory(ItemDefinition, ItemAmount);
			}
			if (flag2 || flag || flag3)
			{
				GlobalReferences.Instance.Anchors.Inventory.ItemPickupInteractAnchor.Set(this);
				GlobalReferences.Instance.GameMenuState.SetInMenu(GameMenuState.GameMenu.ItemPickup);
				GameMenuState gameMenuState = GlobalReferences.Instance.GameMenuState;
				gameMenuState.OnGameMenuChanged = (UnityAction<GameMenuState.GameMenu>)Delegate.Combine(gameMenuState.OnGameMenuChanged, new UnityAction<GameMenuState.GameMenu>(OnGameMenuChanged));
				yield return new WaitUntil(() => m_menuClosed);
			}
			else if (LootType == LootType.Money)
			{
				GlobalReferences.Instance.EventChannels.Inventory.AddPlayerMoney.Raise(ItemAmount);
				ItemAmount = 0;
			}
			else if (LootType == LootType.ItemDefinition)
			{
				component.Inventory.PickupItem(this);
			}
		}
		if (m_persistentData.m_loot.m_amount > 0)
		{
			result.m_cancel = true;
		}
		else
		{
			if (LootType == LootType.Money)
			{
				CharacterInventory component2 = interactor.GetComponent<CharacterInventory>();
				if (component2 != null)
				{
					component2.Inventory.SetHasPickedUpMoney();
				}
			}
			if (aom != null)
			{
				aom.SetCleared();
			}
			if (LootType == LootType.ItemDefinition && ItemDefinition is InventoryUpgradeItemDefinition)
			{
				yield return new WaitForEndOfFrame();
				GlobalReferences.Instance.EventChannels.InGameMenu.ShowInventoryMenuTab.Raise();
			}
		}
		OnExit();
	}

	private void OnGameMenuChanged(GameMenuState.GameMenu menu)
	{
		if (!menu.HasFlag(GameMenuState.GameMenu.ItemPickup))
		{
			m_menuClosed = true;
		}
	}

	private void GenerateLoot()
	{
		m_persistentData.m_generatedLoot = true;
		Loot loot = default(Loot);
		if (m_forcedItemToSpawn.HasAsset())
		{
			loot.m_lootItemType = LootType.ItemDefinition;
			loot.m_amount = m_forcedItemAmount;
			loot.m_itemDefinition = m_forcedItemToSpawn;
		}
		else if (!m_lootTable.GetLootFromLootTable(ref loot))
		{
			Debug.Log("Error generating loot, nothing selected from loot table");
		}
		m_persistentData.m_loot = loot;
		if (m_persistentData.m_loot.m_lootItemType == LootType.ItemDefinition && m_persistentData.m_loot.m_itemDefinition.HasAsset())
		{
			LoadItemDefinitionAsset();
		}
	}

	private void LoadItemDefinitionAsset()
	{
		m_loadedAssetHandle = Addressables.LoadAssetAsync<ItemDefinition>(m_persistentData.m_loot.m_itemDefinition);
		m_loadedAssetHandle.WaitForCompletion();
	}

	public override void Cleanup()
	{
		if (m_loadedAssetHandle.IsValid())
		{
			Addressables.Release(m_loadedAssetHandle);
		}
	}

	private void OnExit()
	{
		GlobalReferences.Instance.Anchors.Inventory.ItemPickupInteractAnchor.Set(null);
		GameMenuState gameMenuState = GlobalReferences.Instance.GameMenuState;
		gameMenuState.OnGameMenuChanged = (UnityAction<GameMenuState.GameMenu>)Delegate.Remove(gameMenuState.OnGameMenuChanged, new UnityAction<GameMenuState.GameMenu>(OnGameMenuChanged));
		GlobalReferences.Instance.GameMenuState.ClearInMenu(GameMenuState.GameMenu.ItemPickup);
		if (m_persistentData.m_loot.m_amount > 0)
		{
			SetContainerState(LootContainerState.Closed);
		}
	}

	public override void Cancel()
	{
		base.Cancel();
		OnExit();
	}

	public override void Reset()
	{
		base.Reset();
		if (m_persistentData != null)
		{
			GenerateLoot();
		}
		SetContainerState(LootContainerState.Closed, instant: true);
	}

	public override bool RequiresPersistentData()
	{
		return true;
	}

	public void ReceiveDataStoreEntry(PersistentDataObject dataEntry)
	{
		m_persistentDataObject = dataEntry;
		m_persistentData = ((m_persistentDataObject.Data != null) ? (m_persistentDataObject.Data as PersistentData) : null);
		if (m_persistentData != null)
		{
			if (m_persistentData.m_generatedLoot && m_persistentData.m_loot.m_amount == 0)
			{
				SetContainerState(LootContainerState.Open, instant: true);
			}
			else
			{
				SetContainerState(LootContainerState.Closed, instant: true);
			}
			if (m_persistentData.m_generatedLoot && m_persistentData.m_loot.m_amount > 0 && m_persistentData.m_loot.m_lootItemType == LootType.ItemDefinition)
			{
				LoadItemDefinitionAsset();
			}
		}
		else
		{
			m_persistentData = new PersistentData();
			m_persistentDataObject.Data = m_persistentData;
		}
	}
}
