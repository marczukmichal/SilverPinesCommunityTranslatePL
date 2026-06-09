using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "Inventory", menuName = "Items/Player Inventory")]
public class Inventory : ScriptableObject
{
	public enum CombineItemResult
	{
		None,
		FailedNotEnoughSpace,
		NewItem,
		WeaponReloaded,
		WeaponUpgraded,
		MeleeRepaired,
		ChargedPoweredItem,
		AddedToKeyRing,
		StacksCombined,
		ItemDefinitionChanged,
		AddedItemUses
	}

	[SerializeField]
	protected Vector2Int m_baseInventorySize;

	[SerializeField]
	protected AddItemGameEventChannel m_addItemEventChannel;

	[SerializeField]
	protected RemoveItemInstanceGameEventChannel m_removeItemInstanceEventChannel;

	[SerializeField]
	protected ItemInstanceGameEventChannel m_useItemEventChannel;

	[SerializeField]
	protected ItemInstanceGameEventChannel m_onItemRemovedEventChannel;

	[SerializeField]
	protected ItemInstanceGameEventChannel m_onItemInstanceStackAmountChangedEventChannel;

	[SerializeField]
	protected DynamicHintGameEventChannel m_dynamicHintEventChannel;

	[SerializeField]
	protected GameObjectAnchor m_playerAnchor;

	protected List<ItemInstance> m_items;

	public UnityAction<ItemInstance> OnItemAddedToInventory;

	public virtual Vector2Int InventorySize => m_baseInventorySize;

	public virtual IReadOnlyCollection<ItemInstance> ItemList => m_items.AsReadOnly();

	public bool CanPlaceItemInPosition(ItemDefinition itemDefinition, Vector2Int position, bool rotated, List<ItemInstance> ignoreList = null)
	{
		bool flag = true;
		Vector2Int itemSize = ItemInstance.GetItemSize(itemDefinition.ItemSize, rotated);
		for (int i = 0; i < itemSize.x; i++)
		{
			for (int j = 0; j < itemSize.y; j++)
			{
				Vector2Int position2 = position;
				position2.x += i;
				position2.y += j;
				if (!IsSlotAvailable(position2, ignoreList))
				{
					flag = false;
					break;
				}
			}
			if (!flag)
			{
				break;
			}
		}
		return flag;
	}

	public bool IsItemAtPositionWithinInventoryBounds(ItemDefinition itemDefinition, Vector2Int position, bool rotated)
	{
		bool flag = true;
		Vector2Int itemSize = ItemInstance.GetItemSize(itemDefinition.ItemSize, rotated);
		for (int i = 0; i < itemSize.x; i++)
		{
			for (int j = 0; j < itemSize.y; j++)
			{
				Vector2Int position2 = position;
				position2.x += i;
				position2.y += j;
				if (!IsPositionWithinInventoryBounds(position2))
				{
					flag = false;
					break;
				}
			}
			if (!flag)
			{
				break;
			}
		}
		return flag;
	}

	public virtual bool IsSlotLocked(Vector2Int position)
	{
		return false;
	}

	public virtual int GetAvailableLastRowSlots()
	{
		return -1;
	}

	public bool IsPositionWithinInventoryBounds(Vector2Int position)
	{
		if (position.x < 0 || position.y < 0 || position.x >= InventorySize.x || position.y >= InventorySize.y)
		{
			return false;
		}
		if (IsSlotLocked(position))
		{
			return false;
		}
		return true;
	}

	public bool IsSlotAvailable(Vector2Int position, List<ItemInstance> ignoreList = null)
	{
		if (!IsPositionWithinInventoryBounds(position))
		{
			return false;
		}
		ItemInstance itemAtPosition = GetItemAtPosition(position);
		if (itemAtPosition != null)
		{
			bool flag = false;
			if (ignoreList != null && ignoreList.Contains(itemAtPosition))
			{
				flag = true;
			}
			if (!flag)
			{
				return false;
			}
		}
		return true;
	}

	protected virtual void OnEnable()
	{
		if ((bool)m_addItemEventChannel)
		{
			m_addItemEventChannel.Register(AddItemToInventory);
		}
		if ((bool)m_removeItemInstanceEventChannel)
		{
			m_removeItemInstanceEventChannel.Register(RemoveItemFromInventory);
		}
		if ((bool)m_useItemEventChannel)
		{
			m_useItemEventChannel.Register(TryUseItemFromInventory);
		}
	}

	protected virtual void OnDisable()
	{
		if ((bool)m_addItemEventChannel)
		{
			m_addItemEventChannel.Unregister(AddItemToInventory);
		}
		if ((bool)m_removeItemInstanceEventChannel)
		{
			m_removeItemInstanceEventChannel.Unregister(RemoveItemFromInventory);
		}
		if ((bool)m_useItemEventChannel)
		{
			m_useItemEventChannel.Unregister(TryUseItemFromInventory);
		}
	}

	public virtual void Clear()
	{
		m_items = new List<ItemInstance>();
	}

	public ItemInstance GetItemAtPosition(Vector2Int position)
	{
		for (int i = 0; i < m_items.Count; i++)
		{
			if (m_items[i].OccupiesPosition(position))
			{
				return m_items[i];
			}
		}
		return null;
	}

	public virtual ItemInstance GetItemOfType(ItemDefinition type)
	{
		foreach (ItemInstance item in m_items)
		{
			if (item.ItemDefinition == type)
			{
				return item;
			}
		}
		return null;
	}

	public virtual bool HasItem(ItemInstance itemInstance)
	{
		return m_items.Contains(itemInstance);
	}

	public virtual ItemInstance GetItemOfClass<T>() where T : ItemInstance
	{
		foreach (ItemInstance item in m_items)
		{
			if (item is T)
			{
				return item;
			}
		}
		return null;
	}

	public int GetAvailableAmmoAmount(AmmunitionItemDefinition ammoType)
	{
		int num = 0;
		foreach (ItemInstance item in m_items)
		{
			if (item.ItemDefinition == ammoType)
			{
				num += item.StackSize;
			}
		}
		return num;
	}

	public bool CanItemBeCombinedWithAnything(ItemInstance item)
	{
		for (int i = 0; i < m_items.Count; i++)
		{
			if (m_items[i] != item && (m_items[i].ItemDefinition.CanBeCombinedWith(item.ItemDefinition) || item.ItemDefinition.CanBeCombinedWith(m_items[i].ItemDefinition)))
			{
				return true;
			}
		}
		return false;
	}

	public bool CanItemBeCombined(ItemInstance itemA, ItemInstance itemB)
	{
		if (itemA == null || itemB == null)
		{
			return false;
		}
		if (itemA == itemB)
		{
			return false;
		}
		return itemA.ItemDefinition.CanBeCombinedWith(itemB.ItemDefinition);
	}

	public bool CanItemBeMovedTo(ItemInstance item, Vector2Int newPosition)
	{
		for (int i = 0; i < item.ItemDefinition.ItemSize.x; i++)
		{
			for (int j = 0; j < item.ItemDefinition.ItemSize.y; j++)
			{
				Vector2Int position = new Vector2Int(newPosition.x + i, newPosition.y + j);
				ItemInstance itemAtPosition = GetItemAtPosition(position);
				if (itemAtPosition != null && itemAtPosition != item)
				{
					return false;
				}
			}
		}
		return true;
	}

	public virtual bool CanAddItemToInventory(ItemPickup itemPickup)
	{
		return CanAddItemToInventory(itemPickup.ItemDefinition, itemPickup.ItemAmount);
	}

	public virtual bool CanAddItemToInventory(ItemDefinition itemDefinition, int amount)
	{
		if (itemDefinition.IsKey)
		{
			foreach (ItemInstance item in m_items)
			{
				if (itemDefinition.IsKey && item.ItemDefinition is KeyRingItemDefinition)
				{
					return true;
				}
			}
		}
		if (itemDefinition is InventoryUpgradeItemDefinition)
		{
			return true;
		}
		if (itemDefinition is ArtifactItemDefinition)
		{
			return true;
		}
		int num = amount;
		if (itemDefinition.MaxStackSize > 1)
		{
			foreach (ItemInstance item2 in m_items)
			{
				if (item2.ItemDefinition == itemDefinition)
				{
					int num2 = Mathf.Min(item2.ItemDefinition.MaxStackSize - item2.StackSize, num);
					if (num2 > 0)
					{
						num -= num2;
					}
				}
			}
		}
		if (num == 0)
		{
			return true;
		}
		Vector2Int position = new Vector2Int(0, 0);
		position.y = 0;
		while (position.y < InventorySize.y)
		{
			position.x = 0;
			while (position.x < InventorySize.x)
			{
				if (CanPlaceItemInPosition(itemDefinition, position, rotated: false))
				{
					return true;
				}
				position.x++;
			}
			position.y++;
		}
		position.y = 0;
		while (position.y < InventorySize.y)
		{
			position.x = 0;
			while (position.x < InventorySize.x)
			{
				if (CanPlaceItemInPosition(itemDefinition, position, rotated: true))
				{
					return true;
				}
				position.x++;
			}
			position.y++;
		}
		return false;
	}

	private void AddItemToInventory(AddItemEventData addItemEvent)
	{
		AddItemToInventory(addItemEvent.m_itemDefinition, addItemEvent.m_itemAmount, addItemEvent.m_position, addItemEvent.m_rotation, addItemEvent.m_autoEquip, out var _, autoAddShortcut: false, addItemEvent.m_amountInItemOverride);
	}

	private bool GetPositionForItem(ItemDefinition itemDefinition, ref Vector2Int position, ref bool rotation, ref int amountRemaining)
	{
		if (itemDefinition.MaxStackSize > 1)
		{
			foreach (ItemInstance item in m_items)
			{
				if (item.ItemDefinition == itemDefinition)
				{
					int num = Mathf.Min(item.ItemDefinition.MaxStackSize - item.StackSize, amountRemaining);
					if (num > 0)
					{
						item.StackSize += num;
						amountRemaining -= num;
						m_onItemInstanceStackAmountChangedEventChannel.Raise(item);
					}
				}
			}
		}
		if (amountRemaining == 0)
		{
			return true;
		}
		position = new Vector2Int(0, 0);
		bool flag = false;
		if (!flag)
		{
			position.y = 0;
			while (position.y < InventorySize.y)
			{
				position.x = 0;
				while (position.x < InventorySize.x)
				{
					if (CanPlaceItemInPosition(itemDefinition, position, rotation))
					{
						flag = true;
						break;
					}
					position.x++;
				}
				if (flag)
				{
					break;
				}
				position.y++;
			}
		}
		if (!flag)
		{
			position.y = 0;
			while (position.y < InventorySize.y)
			{
				position.x = 0;
				while (position.x < InventorySize.x)
				{
					if (CanPlaceItemInPosition(itemDefinition, position, !rotation))
					{
						flag = true;
						break;
					}
					position.x++;
				}
				if (flag)
				{
					rotation = !rotation;
					break;
				}
				position.y++;
			}
		}
		return flag;
	}

	public virtual ItemInstance AddItemToInventory(ItemDefinition itemDefinition, int amount, Vector2Int position, bool rotated, bool autoEquip, out int amountRemaining, bool autoAddShortcut = false, int amountInItemOverride = -1)
	{
		amountRemaining = amount;
		if (position.x < 0 || position.y < 0)
		{
			if (!GetPositionForItem(itemDefinition, ref position, ref rotated, ref amountRemaining))
			{
				if (itemDefinition.IsKey)
				{
					foreach (ItemInstance item in m_items)
					{
						if (itemDefinition.IsKey && item.ItemDefinition is KeyRingItemDefinition)
						{
							KeyRingItemInstance obj = item as KeyRingItemInstance;
							ItemInstance itemInstance = new KeyRingItemInstance(itemDefinition, amountRemaining, position, rotated);
							obj.AddItem(itemInstance);
							GlobalReferences.Instance.EventChannels.Inventory.KeyAutoAddedToKeyRing.Raise(itemInstance);
							amountRemaining = 0;
							return itemInstance;
						}
					}
				}
				Debug.LogError("Failed to add new item " + itemDefinition.ItemName + " as there is no space in the inventory! Check that there is space in the inventory before trying to add a new item!");
				return null;
			}
		}
		else
		{
			ItemInstance itemAtPosition = GetItemAtPosition(position);
			if (itemAtPosition != null)
			{
				int num = Mathf.Min(itemAtPosition.ItemDefinition.MaxStackSize - itemAtPosition.StackSize, amountRemaining);
				if (num > 0)
				{
					itemAtPosition.StackSize += num;
					amountRemaining -= num;
				}
				return itemAtPosition;
			}
		}
		if (amountRemaining == 0)
		{
			return GetItemAtPosition(position);
		}
		ItemInstance itemInstance2 = CreateItemInstance(itemDefinition, position, rotated, amountRemaining, amountInItemOverride);
		if (itemInstance2 != null)
		{
			m_items.Add(itemInstance2);
			amountRemaining = 0;
			if (m_dynamicHintEventChannel != null)
			{
				m_dynamicHintEventChannel.Raise(DynamicHintEvent.AddItemToInventory);
				if (itemInstance2.CanUse(this, checkConditions: false))
				{
					m_dynamicHintEventChannel.Raise(DynamicHintEvent.QuickItemSecondUsableItem);
				}
			}
			if (autoEquip)
			{
				AutoEquipItem(itemInstance2);
			}
			if (autoAddShortcut && itemInstance2.ItemDefinition.CanHaveShortcutSet(this))
			{
				AutoAddShortcutForItem(itemInstance2);
			}
		}
		OnItemAddedToInventory?.Invoke(itemInstance2);
		return itemInstance2;
	}

	protected static ItemInstance CreateItemInstance(ItemDefinition itemDefinition, Vector2Int position, bool rotated, int amountRemaining, int amountInItemOverride)
	{
		if (itemDefinition is ProjectileWeaponItemDefinition)
		{
			int stackSize = ((amountInItemOverride != -1) ? amountInItemOverride : amountRemaining);
			return new ProjectileWeaponItemInstance(itemDefinition as ProjectileWeaponItemDefinition, stackSize, position, rotated);
		}
		if (itemDefinition is MeleeWeaponItemDefinition meleeWeaponItemDefinition)
		{
			int durability = ((amountInItemOverride != -1) ? amountInItemOverride : meleeWeaponItemDefinition.MaxDurability);
			return new MeleeWeaponItemInstance(itemDefinition as MeleeWeaponItemDefinition, amountRemaining, position, rotated, durability);
		}
		if (itemDefinition is SecondaryWeaponItemDefinition)
		{
			return new SecondaryWeaponItemInstance(itemDefinition as SecondaryWeaponItemDefinition, amountRemaining, position, rotated);
		}
		if (itemDefinition is ArtifactItemDefinition)
		{
			return new ArtifactItemInstance(itemDefinition as ArtifactItemDefinition, amountRemaining, position, rotated);
		}
		if (itemDefinition is KeyRingItemDefinition)
		{
			return new KeyRingItemInstance(itemDefinition, amountRemaining, position, rotated);
		}
		if (itemDefinition is PoweredItemDefinition)
		{
			return new PoweredItemInstance(itemDefinition as PoweredItemDefinition, amountRemaining, position, rotated);
		}
		if (itemDefinition is RefillableItemDefinition)
		{
			int stackSize2 = ((amountInItemOverride != -1) ? amountInItemOverride : amountRemaining);
			return new RefillableItemInstance(itemDefinition as RefillableItemDefinition, stackSize2, position, rotated);
		}
		return new ItemInstance(itemDefinition, amountRemaining, position, rotated);
	}

	protected virtual void AutoEquipItem(ItemInstance item)
	{
	}

	protected virtual void AutoAddShortcutForItem(ItemInstance item)
	{
	}

	public bool ReceiveItemFromTransfer(ItemInstance transferredItem, Vector2Int position, out int amountTransferred)
	{
		amountTransferred = 0;
		if (position.x < 0 || position.y < 0)
		{
			int amountRemaining = transferredItem.StackSize;
			int stackSize = transferredItem.StackSize;
			bool rotation = transferredItem.ItemRotated;
			bool positionForItem = GetPositionForItem(transferredItem.ItemDefinition, ref position, ref rotation, ref amountRemaining);
			amountTransferred = stackSize - amountRemaining;
			transferredItem.StackSize = amountRemaining;
			transferredItem.ItemRotated = rotation;
			if (amountRemaining == 0)
			{
				return true;
			}
			if (!positionForItem)
			{
				Debug.LogWarning("Failed to fully transfer item " + transferredItem.ItemDefinition.ItemName + " as there is no space in the inventory!");
				return false;
			}
		}
		ItemInstance itemAtPosition = GetItemAtPosition(position);
		if (itemAtPosition != null)
		{
			int num = Mathf.Min(itemAtPosition.ItemDefinition.MaxStackSize - itemAtPosition.StackSize, transferredItem.StackSize);
			if (num > 0)
			{
				itemAtPosition.StackSize += num;
				transferredItem.StackSize -= num;
			}
			amountTransferred += num;
		}
		else
		{
			transferredItem.ItemGridPosition = position;
			m_items.Add(transferredItem);
			amountTransferred = transferredItem.StackSize;
		}
		return true;
	}

	private void RemoveItemFromInventory(RemoveItemInstanceEventData removeItemEvent)
	{
		if (m_items.Contains(removeItemEvent.m_itemInstance))
		{
			RemoveItem(removeItemEvent.m_itemInstance, removeItemEvent.m_itemAmount);
		}
	}

	protected void RemoveItem(ItemInstance itemInstance, int amount)
	{
		if (!m_items.Contains(itemInstance))
		{
			m_removeItemInstanceEventChannel.Raise(new RemoveItemInstanceEventData
			{
				m_itemInstance = itemInstance,
				m_itemAmount = amount
			});
			return;
		}
		foreach (ItemInstance item in m_items)
		{
			if (item is KeyRingItemInstance)
			{
				(item as KeyRingItemInstance).RemoveItem(itemInstance);
			}
		}
		if (itemInstance.StackSize > amount)
		{
			itemInstance.StackSize -= amount;
			return;
		}
		m_items.Remove(itemInstance);
		if (m_onItemRemovedEventChannel != null)
		{
			m_onItemRemovedEventChannel.Raise(itemInstance);
		}
		OnItemRemoved(itemInstance);
	}

	protected virtual void OnItemRemoved(ItemInstance item)
	{
	}

	protected void TryApplyWeaponUpgrade(ProjectileWeaponItemInstance weaponItem, ItemInstance item)
	{
		if (weaponItem.AttachUpgrade(item))
		{
			RemoveItem(item, 1);
			item.ItemGridPosition = new Vector2Int(-1, -1);
		}
	}

	public bool RequestUnloadItem(ItemInstance item)
	{
		if (item is ProjectileWeaponItemInstance)
		{
			return UnloadWeapon(item as ProjectileWeaponItemInstance);
		}
		return false;
	}

	protected bool UnloadWeapon(ProjectileWeaponItemInstance weaponItem)
	{
		AmmunitionItemDefinition loadedAmmoType = weaponItem.LoadedAmmoType;
		int ammoCount = weaponItem.AmmoCount;
		if (CanAddItemToInventory(loadedAmmoType, ammoCount))
		{
			ammoCount = weaponItem.UnloadAmmo();
			AddItemToInventory(new AddItemEventData
			{
				m_itemAmount = ammoCount,
				m_itemDefinition = loadedAmmoType
			});
			return true;
		}
		return false;
	}

	protected bool MoveItemStacks(ItemInstance sourceItem, ItemInstance targetItem)
	{
		int num = Mathf.Min(targetItem.GetEmptyCapacity(), sourceItem.StackSize);
		targetItem.StackSize += num;
		RemoveItem(sourceItem, num);
		return num > 0;
	}

	public bool CanUnloadItem(ItemInstance itemInstance)
	{
		if (itemInstance is ProjectileWeaponItemInstance)
		{
			return (itemInstance as ProjectileWeaponItemInstance).AmmoCount > 0;
		}
		return false;
	}

	protected void TryUseItemFromInventory(ItemInstance itemInstance)
	{
		if (itemInstance != null && itemInstance.StackSize > 0 && m_items.Contains(itemInstance))
		{
			UseItem(itemInstance);
		}
	}

	protected void UseItem(ItemInstance itemInstance)
	{
		foreach (ItemEffect useEffect in itemInstance.ItemDefinition.UseEffects)
		{
			useEffect.ApplyEffect(m_playerAnchor.Item, this);
		}
		if (itemInstance is RefillableItemInstance refillableItemInstance)
		{
			if (refillableItemInstance.RefillableItemDefinition.AutoConsumeUse)
			{
				refillableItemInstance.Uses--;
			}
			return;
		}
		switch (itemInstance.ItemDefinition.UseItemPostResult)
		{
		case ItemDefinition.UseItemResult.Consume:
			RemoveItem(itemInstance, 1);
			break;
		case ItemDefinition.UseItemResult.ChangeItemDefinition:
			itemInstance.ChangeItemDefinition(itemInstance.ItemDefinition.UseResultChangeItemDefinition);
			break;
		case ItemDefinition.UseItemResult.ReplaceWithItem:
			ReplaceWithUsedItem(itemInstance);
			break;
		case ItemDefinition.UseItemResult.Reusable:
			break;
		}
	}

	protected virtual void ReplaceWithUsedItem(ItemInstance itemInstance)
	{
		Vector2Int itemGridPosition = itemInstance.ItemGridPosition;
		ItemDefinition useResultChangeItemDefinition = itemInstance.ItemDefinition.UseResultChangeItemDefinition;
		bool itemRotated = itemInstance.ItemRotated;
		RemoveItem(itemInstance, itemInstance.StackSize);
		AddItemToInventory(useResultChangeItemDefinition, 1, itemGridPosition, itemRotated, autoEquip: false, out var _);
	}

	public int GetItemCount(ItemDefinition itemDefinition)
	{
		int num = 0;
		foreach (ItemInstance item in m_items)
		{
			if (item.ItemDefinition == itemDefinition)
			{
				num += item.StackSize;
			}
		}
		return num;
	}

	public void ConsumeItemOfType(ItemDefinition itemDefinition, int amount = 1)
	{
		int num = amount;
		int num2 = m_items.Count - 1;
		while (num2 >= 0 && num > 0)
		{
			ItemInstance itemInstance = m_items[num2];
			if (itemInstance.ItemDefinition == itemDefinition)
			{
				int num3 = Mathf.Min(num, itemInstance.StackSize);
				num -= num3;
				RemoveItem(itemInstance, num3);
			}
			num2--;
		}
	}

	public virtual bool IsItemMovable(ItemInstance itemInstance)
	{
		return true;
	}

	public static void TransferItemOwnership(Inventory fromInventory, Inventory targetInventory, ItemInstance itemInstance)
	{
		fromInventory.ReleaseItemOwnership(itemInstance);
		targetInventory.ReceiveItemOwnership(itemInstance);
	}

	private void ReleaseItemOwnership(ItemInstance itemInstance)
	{
		m_items.Remove(itemInstance);
		OnItemRemoved(itemInstance);
	}

	private void ReceiveItemOwnership(ItemInstance itemInstance)
	{
		if (!m_items.Contains(itemInstance))
		{
			m_items.Add(itemInstance);
		}
	}

	public void TransferItem(ItemInstance itemInstance, Inventory targetInventory, Vector2Int targetPosition)
	{
		int amountTransferred;
		bool flag = targetInventory.ReceiveItemFromTransfer(itemInstance, targetPosition, out amountTransferred);
		if (amountTransferred > 0 && flag)
		{
			RemoveItem(itemInstance, amountTransferred);
		}
	}

	protected void AddItemToKeyRing(KeyRingItemInstance keyring, ItemInstance item)
	{
		keyring.AddItem(item);
		m_items.Remove(item);
	}

	public void AutoAddToKeyRing(ItemInstance item)
	{
		KeyRingItemInstance keyRingItemInstance = null;
		foreach (ItemInstance item2 in m_items)
		{
			if (item2 is KeyRingItemInstance)
			{
				keyRingItemInstance = item2 as KeyRingItemInstance;
				break;
			}
		}
		if (keyRingItemInstance != null)
		{
			AddItemToKeyRing(keyRingItemInstance, item);
		}
	}

	public int GetResourceScore(GameDifficultyResourceScoreType type)
	{
		int num = 0;
		foreach (ItemInstance item in m_items)
		{
			num += item.GetDifficultyResourceValue(type);
		}
		return num;
	}

	public int ReloadWeaponWithAmmo(ProjectileWeaponItemInstance weaponItem, ItemInstance ammo, int ammoToAddTotal)
	{
		int num = Mathf.Min(ammoToAddTotal, ammo.StackSize);
		weaponItem.AddAmmo(num);
		ammoToAddTotal -= num;
		RemoveItem(ammo, num);
		return ammoToAddTotal;
	}

	protected virtual void TryLoadWeapon(ProjectileWeaponItemInstance weaponItem, ItemInstance item)
	{
		if (!(item.ItemDefinition is AmmunitionItemDefinition) || !weaponItem.ItemDefinition.CanBeCombinedWith(item.ItemDefinition))
		{
			return;
		}
		if (weaponItem.LoadedAmmoType != item.ItemDefinition)
		{
			if (!UnloadWeapon(weaponItem))
			{
				Debug.LogError("Failed to unload weapon in TryLoadWeapon! This should not happen, no inventory space?");
			}
			weaponItem.SwitchAmmoType(item.ItemDefinition as AmmunitionItemDefinition, 0);
		}
		ReloadWeaponWithAmmo(weaponItem, item, weaponItem.GetEmptyAmmoCapacity());
	}

	protected virtual void TryRepairItem(MeleeWeaponItemInstance meleeWeapon, ItemInstance repairItem)
	{
		RepairMeleeItemEffect repairMeleeItemEffect = null;
		foreach (ItemEffect useEffect in repairItem.ItemDefinition.UseEffects)
		{
			if (useEffect is RepairMeleeItemEffect repairMeleeItemEffect2)
			{
				repairMeleeItemEffect = repairMeleeItemEffect2;
			}
		}
		if (repairMeleeItemEffect != null)
		{
			RepairProportion(meleeWeapon, repairMeleeItemEffect.RepairProportion);
			RemoveItem(repairItem, 1);
		}
		else
		{
			Debug.LogError("Tried to repair with a repair item" + repairItem.ItemName + " that doesn't have a repair use effect!");
		}
	}

	protected bool TryFillRefillableItem(RefillableItemInstance refillableItem, ItemInstance item)
	{
		int num = Mathf.Min(refillableItem.RefillableItemDefinition.MaxUses - refillableItem.Uses, item.StackSize);
		if (num > 0)
		{
			refillableItem.Uses += num;
			RemoveItem(item, num);
			return true;
		}
		return false;
	}

	protected bool ValidateNewItemPosition(ItemDefinition item, ref Vector2Int position, ref bool rotated, List<ItemInstance> consumedItemList)
	{
		if (CanPlaceItemInPosition(item, position, rotated, consumedItemList))
		{
			return true;
		}
		bool flag = false;
		if (!flag)
		{
			position.y = 0;
			while (position.y < InventorySize.y)
			{
				position.x = 0;
				while (position.x < InventorySize.x)
				{
					if (CanPlaceItemInPosition(item, position, rotated, consumedItemList))
					{
						flag = true;
						break;
					}
					position.x++;
				}
				if (flag)
				{
					break;
				}
				position.y++;
			}
		}
		if (!flag)
		{
			position.y = 0;
			while (position.y < InventorySize.y)
			{
				position.x = 0;
				while (position.x < InventorySize.x)
				{
					if (CanPlaceItemInPosition(item, position, !rotated, consumedItemList))
					{
						flag = true;
						break;
					}
					position.x++;
				}
				if (flag)
				{
					rotated = !rotated;
					break;
				}
				position.y++;
			}
		}
		return flag;
	}

	protected float GetRepairProportion(ItemInstance item)
	{
		return 0f;
	}

	protected void ChargeItem(PoweredItemInstance poweredItem, float chargeAmount)
	{
		poweredItem.ChargeAmount += chargeAmount;
	}

	public void RepairProportion(MeleeWeaponItemInstance meleeWeapon, float proportion)
	{
		float durabilityProportion = meleeWeapon.DurabilityProportion;
		meleeWeapon.RepairDurability(Mathf.RoundToInt((float)meleeWeapon.WeaponDefinition.MaxDurability * proportion));
		GlobalReferences.Instance.EventChannels.Inventory.MeleeWeaponItemRepaired.Raise(meleeWeapon);
		GlobalReferences.Instance.EventChannels.Generic.MeleeWeaponDurabilityChanged.Raise(new MeleeWeaponDurabilityChangedData
		{
			m_meleeWeapon = meleeWeapon,
			m_startingDurabilityProportion = durabilityProportion,
			m_endingDurabilityProportion = meleeWeapon.DurabilityProportion
		});
	}

	protected virtual void ForceUnequipItem(ItemInstance item)
	{
	}

	protected virtual bool ContainsItem(ItemInstance item)
	{
		return m_items.Contains(item);
	}

	public CombineItemResult TryCombineItems(ItemInstance sourceItem, ItemInstance targetItem, Inventory sourceItemParentInventory)
	{
		CombineItemResult combineItemResult = CombineItemResult.None;
		if (!ContainsItem(targetItem))
		{
			return combineItemResult;
		}
		bool consumeItemA;
		bool consumeItemB;
		ItemCombinationRule combinationResult = ItemDefinition.GetCombinationResult(sourceItem.ItemDefinition, targetItem.ItemDefinition, out consumeItemA, out consumeItemB);
		if (sourceItem is ProjectileWeaponItemInstance)
		{
			if (targetItem.ItemDefinition is ProjectileWeaponUpgradeDefinition)
			{
				TryApplyWeaponUpgrade(sourceItem as ProjectileWeaponItemInstance, targetItem);
				combineItemResult = CombineItemResult.WeaponUpgraded;
			}
			else if (targetItem.ItemDefinition is AmmunitionItemDefinition)
			{
				TryLoadWeapon(sourceItem as ProjectileWeaponItemInstance, targetItem);
				combineItemResult = CombineItemResult.WeaponReloaded;
			}
		}
		else if (targetItem is ProjectileWeaponItemInstance)
		{
			if (sourceItem.ItemDefinition is ProjectileWeaponUpgradeDefinition)
			{
				TryApplyWeaponUpgrade(targetItem as ProjectileWeaponItemInstance, sourceItem);
				combineItemResult = CombineItemResult.WeaponUpgraded;
			}
			else if (sourceItem.ItemDefinition is AmmunitionItemDefinition)
			{
				TryLoadWeapon(targetItem as ProjectileWeaponItemInstance, sourceItem);
				combineItemResult = CombineItemResult.WeaponReloaded;
			}
		}
		else if (sourceItem is KeyRingItemInstance)
		{
			KeyRingItemInstance keyRingItemInstance = (KeyRingItemInstance)sourceItem;
			if (keyRingItemInstance.CanAddItem(targetItem))
			{
				AddItemToKeyRing(keyRingItemInstance, targetItem);
				combineItemResult = CombineItemResult.AddedToKeyRing;
			}
		}
		else if (targetItem is KeyRingItemInstance)
		{
			KeyRingItemInstance keyRingItemInstance2 = (KeyRingItemInstance)targetItem;
			if (keyRingItemInstance2.CanAddItem(sourceItem))
			{
				AddItemToKeyRing(keyRingItemInstance2, sourceItem);
				combineItemResult = CombineItemResult.AddedToKeyRing;
			}
		}
		else if (sourceItem is RefillableItemInstance)
		{
			RefillableItemInstance refillableItemInstance = (RefillableItemInstance)sourceItem;
			if (refillableItemInstance.CanRefillFromItem(targetItem) && TryFillRefillableItem(refillableItemInstance, targetItem))
			{
				combineItemResult = CombineItemResult.AddedItemUses;
			}
		}
		else if (targetItem is RefillableItemInstance)
		{
			RefillableItemInstance refillableItemInstance2 = (RefillableItemInstance)targetItem;
			if (refillableItemInstance2.CanRefillFromItem(sourceItem) && TryFillRefillableItem(refillableItemInstance2, sourceItem))
			{
				combineItemResult = CombineItemResult.AddedItemUses;
			}
		}
		if (combineItemResult == CombineItemResult.None)
		{
			ItemInstance itemInstance = null;
			if (combinationResult != null)
			{
				Vector2Int position = new Vector2Int(-1, -1);
				bool rotated = false;
				Vector2Int position2 = new Vector2Int(-1, -1);
				bool rotated2 = false;
				bool flag = false;
				if (combinationResult.Type == ItemCombinationRule.CombineType.ProduceNewItem)
				{
					List<ItemInstance> list = new List<ItemInstance>();
					if (consumeItemB && targetItem.StackSize == 1)
					{
						list.Add(targetItem);
					}
					if (consumeItemA && sourceItem.StackSize == 1)
					{
						list.Add(sourceItem);
					}
					if (consumeItemB && targetItem.StackSize == 1)
					{
						position = targetItem.ItemGridPosition;
						rotated = targetItem.ItemRotated;
						position2 = sourceItem.ItemGridPosition;
						rotated2 = sourceItem.ItemRotated;
					}
					else if (consumeItemA && sourceItem.StackSize == 1)
					{
						position = sourceItem.ItemGridPosition;
						rotated = sourceItem.ItemRotated;
						position2 = targetItem.ItemGridPosition;
						rotated2 = targetItem.ItemRotated;
					}
					if (!ValidateNewItemPosition(combinationResult.ProducedItem, ref position, ref rotated, list))
					{
						Debug.LogError("New item from combination result will be an invalid position! Cancel item combination");
						flag = true;
						combineItemResult = CombineItemResult.FailedNotEnoughSpace;
					}
				}
				else if (combinationResult.Type == ItemCombinationRule.CombineType.ChangeItemDefinition)
				{
					if (targetItem.ItemDefinition == combinationResult.CombineWith)
					{
						itemInstance = targetItem;
					}
					else if (sourceItem.ItemDefinition == combinationResult.CombineWith)
					{
						itemInstance = sourceItem;
					}
					if (itemInstance != null)
					{
						List<ItemInstance> list2 = new List<ItemInstance>();
						if (consumeItemB && targetItem.StackSize == 1)
						{
							list2.Add(targetItem);
						}
						if (consumeItemA && sourceItem.StackSize == 1)
						{
							list2.Add(sourceItem);
						}
						position = itemInstance.ItemGridPosition;
						rotated = itemInstance.ItemRotated;
					}
				}
				if (!flag)
				{
					if (consumeItemA)
					{
						RemoveItem(sourceItem, 1);
					}
					if (consumeItemB)
					{
						RemoveItem(targetItem, 1);
					}
					int amountRemaining;
					if (combinationResult.SourceResult == ItemCombinationRule.CombineSourceResult.ReplaceWithItem)
					{
						sourceItemParentInventory.AddItemToInventory(combinationResult.ReplaceSourceWithItemDefinition, 1, position2, rotated2, autoEquip: false, out amountRemaining);
					}
					switch (combinationResult.Type)
					{
					case ItemCombinationRule.CombineType.ProduceNewItem:
					{
						bool num = GlobalReferences.Instance.MainInventory.HasSeenItemType(combinationResult.ProducedItem);
						bool isImportantItem = combinationResult.ProducedItem.IsImportantItem;
						ItemInstance value = AddItemToInventory(combinationResult.ProducedItem, combinationResult.ProducedAmount, position, rotated, autoEquip: false, out amountRemaining);
						combineItemResult = CombineItemResult.NewItem;
						if (!num || isImportantItem)
						{
							GlobalReferences.Instance.EventChannels.Inventory.NewItemCombined.Raise(value);
						}
						break;
					}
					case ItemCombinationRule.CombineType.RepairMelee:
						if (sourceItem is MeleeWeaponItemInstance)
						{
							TryRepairItem((MeleeWeaponItemInstance)sourceItem, targetItem);
						}
						else if (targetItem is MeleeWeaponItemInstance)
						{
							TryRepairItem((MeleeWeaponItemInstance)targetItem, sourceItem);
						}
						combineItemResult = CombineItemResult.MeleeRepaired;
						break;
					case ItemCombinationRule.CombineType.ChargePoweredItem:
						if (sourceItem is PoweredItemInstance)
						{
							ChargeItem((PoweredItemInstance)sourceItem, combinationResult.ProducedAmount);
						}
						else if (targetItem is PoweredItemInstance)
						{
							ChargeItem((PoweredItemInstance)targetItem, combinationResult.ProducedAmount);
						}
						combineItemResult = CombineItemResult.ChargedPoweredItem;
						break;
					case ItemCombinationRule.CombineType.ChangeItemDefinition:
						ForceUnequipItem(itemInstance);
						itemInstance.ChangeItemDefinition(combinationResult.ProducedItem);
						combineItemResult = CombineItemResult.ItemDefinitionChanged;
						break;
					case ItemCombinationRule.CombineType.ProduceArtifact:
						GlobalReferences.Instance.MainInventory.AddItemToInventory(combinationResult.ProducedItem, 1, new Vector2Int(-1, -1), rotated: false, autoEquip: false, out amountRemaining);
						combineItemResult = CombineItemResult.NewItem;
						break;
					}
				}
			}
			else if (sourceItem.ItemDefinition == targetItem.ItemDefinition && MoveItemStacks(sourceItem, targetItem))
			{
				combineItemResult = CombineItemResult.StacksCombined;
			}
		}
		return combineItemResult;
	}

	public virtual PersistentDataInventory GetPersistentData()
	{
		return new PersistentDataInventory
		{
			m_items = new List<ItemInstance>(m_items)
		};
	}

	public void ReadFromPersistentData(PersistentDataInventory data)
	{
		m_items = new List<ItemInstance>(data.m_items);
	}
}
