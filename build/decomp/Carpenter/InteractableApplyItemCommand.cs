using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

[Serializable]
public class InteractableApplyItemCommand : BaseInteractableCommand, IApplyItem, IPersistentComponent
{
	public enum ApplyItemReuseMode
	{
		NotAllowed,
		SelectItem,
		AutoUseSameItem
	}

	[Serializable]
	private class PersistentData
	{
		public bool m_itemApplied;
	}

	[SerializeField]
	private ItemDefinition m_requiredItemType;

	[SerializeField]
	private bool m_consumeItem;

	[FormerlySerializedAs("m_applyItemInteractUIPrefab")]
	[SerializeField]
	private GameObject m_interactUIPrefab;

	[SerializeField]
	private bool m_alwaysContinue;

	[SerializeField]
	private ApplyItemReuseMode m_allowReuse;

	[Tooltip("If true then disable persistence and the item has to be reapplied")]
	[SerializeField]
	private bool m_alwaysRequired;

	private bool m_menuClosed;

	private ItemInstance m_usedItem;

	private bool m_itemApplied;

	private bool m_hasBeenUsedSuccesfully;

	private PersistentDataObject m_persistentDataObject;

	private PersistentData m_persistentData;

	public ItemDefinition ItemDefinition => m_requiredItemType;

	public bool ConsumeItem => m_consumeItem;

	public GameObject GetApplyItemInteractUIPrefab()
	{
		return m_interactUIPrefab;
	}

	public static InteractableApplyItemCommand CreateInstance(ItemDefinition itemDefinition, bool consumeItem, GameObject interactPrefab)
	{
		return new InteractableApplyItemCommand
		{
			m_requiredItemType = itemDefinition,
			m_consumeItem = consumeItem,
			m_interactUIPrefab = interactPrefab,
			m_alwaysContinue = false
		};
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
			m_itemApplied = m_persistentData.m_itemApplied;
			return;
		}
		m_persistentData = new PersistentData();
		m_persistentDataObject.Data = m_persistentData;
	}

	public override IEnumerator DoInteraction(BaseInteractable interactable, BaseInteractor interactor, InteractionResult result)
	{
		bool flag = false;
		if (m_allowReuse != 0)
		{
			if (m_hasBeenUsedSuccesfully && m_allowReuse == ApplyItemReuseMode.AutoUseSameItem)
			{
				flag = true;
			}
			m_itemApplied = false;
			m_hasBeenUsedSuccesfully = false;
		}
		if (GameDebugCommands.CHEAT_MASTER_KEY || m_itemApplied)
		{
			yield break;
		}
		m_menuClosed = false;
		m_usedItem = null;
		if (flag)
		{
			m_usedItem = GlobalReferences.Instance.MainInventory.GetItemOfType(m_requiredItemType);
		}
		if (m_usedItem == null)
		{
			GlobalReferences.Instance.Anchors.Inventory.ApplyItemInteractableAnchor.Set(this);
			GlobalReferences.Instance.EventChannels.Inventory.ApplyItemToInteractable.Register(OnTryApplyItem);
			GlobalReferences.Instance.EventChannels.Inventory.ShowApplyItemInteract.Raise(value: true);
			GameMenuState gameMenuState = GlobalReferences.Instance.GameMenuState;
			gameMenuState.OnGameMenuChanged = (UnityAction<GameMenuState.GameMenu>)Delegate.Combine(gameMenuState.OnGameMenuChanged, new UnityAction<GameMenuState.GameMenu>(OnGameMenuChanged));
		}
		else
		{
			m_hasBeenUsedSuccesfully = true;
		}
		yield return new WaitUntil(() => m_menuClosed || m_usedItem != null);
		if (m_usedItem == null)
		{
			if (!m_alwaysContinue)
			{
				result.m_cancel = true;
			}
		}
		else
		{
			if (m_interactUIPrefab != null)
			{
				float time = 1f;
				InventoryApplyItemInteract component = m_interactUIPrefab.GetComponent<InventoryApplyItemInteract>();
				if (component != null)
				{
					time = component.CloseDelay;
				}
				yield return new WaitForSecondsRealtime(time);
			}
			if (m_consumeItem)
			{
				GlobalReferences.Instance.EventChannels.Inventory.RequestRemoveItem.Raise(new RemoveItemInstanceEventData
				{
					m_itemInstance = m_usedItem,
					m_itemAmount = 1
				});
			}
			m_hasBeenUsedSuccesfully = true;
		}
		OnExit();
	}

	private void OnExit()
	{
		Unregister();
		GlobalReferences.Instance.GameMenuState.ClearInMenu(GameMenuState.GameMenu.InGameMenu);
	}

	public override void Cancel()
	{
		base.Cancel();
		OnExit();
	}

	private void Unregister()
	{
		GameMenuState gameMenuState = GlobalReferences.Instance.GameMenuState;
		gameMenuState.OnGameMenuChanged = (UnityAction<GameMenuState.GameMenu>)Delegate.Remove(gameMenuState.OnGameMenuChanged, new UnityAction<GameMenuState.GameMenu>(OnGameMenuChanged));
		GlobalReferences.Instance.EventChannels.Inventory.ApplyItemToInteractable.Unregister(OnTryApplyItem);
		GlobalReferences.Instance.Anchors.Inventory.ApplyItemInteractableAnchor.Set(null);
		GlobalReferences.Instance.EventChannels.Inventory.ShowApplyItemInteract.Raise(value: false);
	}

	public override void Cleanup()
	{
		base.Cleanup();
		Unregister();
	}

	private void OnGameMenuChanged(GameMenuState.GameMenu menu)
	{
		if (!menu.HasFlag(GameMenuState.GameMenu.ApplyInteract))
		{
			m_menuClosed = true;
		}
	}

	private void OnTryApplyItem(ItemInstance itemInstance)
	{
		TryApplyItemEventData tryApplyItemEventData = new TryApplyItemEventData
		{
			m_item = itemInstance,
			m_isKey = itemInstance.ItemDefinition.IsKey,
			m_success = false
		};
		if (CanApplyItem(itemInstance))
		{
			m_itemApplied = true;
			if (m_persistentData != null)
			{
				m_persistentData.m_itemApplied = true;
			}
			m_usedItem = itemInstance;
			tryApplyItemEventData.m_success = true;
		}
		else
		{
			Debug.LogWarning("Tried to apply an invalid item to an interact");
		}
		GlobalReferences.Instance.EventChannels.Generic.TryApplyItem.Raise(tryApplyItemEventData);
	}

	public bool CanApplyItem(ItemInstance itemInstance)
	{
		if (itemInstance != null)
		{
			return ItemDefinition == itemInstance.ItemDefinition;
		}
		return false;
	}

	public override void Reset()
	{
		base.Reset();
		m_itemApplied = false;
		if (m_persistentData != null)
		{
			m_persistentData.m_itemApplied = false;
		}
		m_usedItem = null;
	}
}
