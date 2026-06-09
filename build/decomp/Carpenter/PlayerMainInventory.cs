using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "PlayerMainInventory", menuName = "Items/Player Main Inventory")]
public class PlayerMainInventory : Inventory
{
	[Serializable]
	public struct SpecialItemSlot
	{
		[SerializeField]
		public ItemDefinition m_itemType;

		[SerializeField]
		public Vector2Int m_virtualPosition;
	}

	[Header("Player Artifact Inventory")]
	[SerializeField]
	private List<ArtifactItemInstance> m_artifactItems;

	[SerializeField]
	private List<ItemInstance> m_equippedArtifacts;

	[SerializeField]
	private int m_maxEquippedArtifactsCount;

	[Header("Event Channels")]
	[SerializeField]
	private IntGameEventChannel m_setMaxEquippedArtifactsEventChannel;

	[SerializeField]
	private VoidGameEventChannel m_upgradeMaxArtifactCountEventChannel;

	[Header("Healing Item Defintions")]
	[SerializeField]
	private List<ItemDefinition> m_healingItems;

	[SerializeField]
	protected List<SpecialItemSlot> m_specialItemSlots;

	private ItemInstance m_tempUseItem;

	[SerializeField]
	private int m_maxUpgradeInventorySlots;

	[Header("Player Character Inventory")]
	private ItemInstance m_queuedPrimaryItem;

	private ItemInstance m_equippedPrimaryItem;

	private ItemInstance m_equippedMeleeItem;

	[SerializeField]
	private List<ItemInstance> m_inventoryUpgrades;

	private ItemInstance m_newInventoryUpgrade;

	private List<ItemInstance> m_specialItems;

	private List<ItemInstance> m_cacheList = new List<ItemInstance>();

	[SerializeField]
	private List<ItemDefinition> m_itemShortcuts;

	[SerializeField]
	private int m_money;

	private bool m_pickedUpMoney;

	[Header("Event Channels")]
	[SerializeField]
	private ItemInstanceGameEventChannel m_inventoryShortcutUsedEventChannel;

	[SerializeField]
	private IntGameEventChannel m_addMoneyEventChannel;

	[SerializeField]
	private IntGameEventChannel m_onMoneyChangedEventChannel;

	[SerializeField]
	private ItemInstanceGameEventChannel m_requestEquipItemEventChannel;

	private List<ItemDefinition> m_seenItemTypes;

	public List<ArtifactItemInstance> ArtifactItems => m_artifactItems;

	public ItemInstance TempUseItem => m_tempUseItem;

	public IEnumerable<SpecialItemSlot> SpecialItemSlots => m_specialItemSlots.AsReadOnly();

	public override Vector2Int InventorySize
	{
		get
		{
			Vector2Int baseInventorySize = m_baseInventorySize;
			int num = 0;
			foreach (ItemInstance inventoryUpgrade in m_inventoryUpgrades)
			{
				InventoryUpgradeItemDefinition inventoryUpgradeItemDefinition = inventoryUpgrade.ItemDefinition as InventoryUpgradeItemDefinition;
				if (inventoryUpgradeItemDefinition != null)
				{
					num += inventoryUpgradeItemDefinition.AddedSlots;
				}
			}
			if (num > m_maxUpgradeInventorySlots)
			{
				num = m_maxUpgradeInventorySlots;
			}
			if (num > 0)
			{
				int num2 = Mathf.CeilToInt((float)num / (float)m_baseInventorySize.x);
				baseInventorySize.y += num2;
			}
			return baseInventorySize;
		}
	}

	public ItemInstance QueuedPrimaryItem => m_queuedPrimaryItem;

	public ItemInstance EquippedPrimaryItem => m_equippedPrimaryItem;

	public ProjectileWeaponItemInstance EquippedRangedItem
	{
		get
		{
			if (m_equippedPrimaryItem is ProjectileWeaponItemInstance)
			{
				return m_equippedPrimaryItem as ProjectileWeaponItemInstance;
			}
			return null;
		}
		set
		{
			if (m_equippedPrimaryItem != value)
			{
				EquipRangedItem(value);
			}
		}
	}

	public ItemInstance EquippedMeleeItem
	{
		get
		{
			return m_equippedMeleeItem;
		}
		set
		{
			if (m_equippedMeleeItem != value)
			{
				EquipMeleeItem(value);
			}
		}
	}

	public SecondaryWeaponItemInstance EquippedSecondaryItem
	{
		get
		{
			if (m_equippedPrimaryItem is SecondaryWeaponItemInstance)
			{
				return m_equippedPrimaryItem as SecondaryWeaponItemInstance;
			}
			return null;
		}
	}

	public override IReadOnlyCollection<ItemInstance> ItemList
	{
		get
		{
			m_cacheList.Clear();
			foreach (ItemInstance item in m_items)
			{
				m_cacheList.Add(item);
			}
			foreach (ItemInstance specialItem in m_specialItems)
			{
				m_cacheList.Add(specialItem);
			}
			return m_cacheList.AsReadOnly();
		}
	}

	public int Money => m_money;

	public IReadOnlyCollection<ItemInstance> EquippedArtifacts => m_equippedArtifacts.AsReadOnly();

	public int MaxEquippedArtifactsCount => m_maxEquippedArtifactsCount;

	public ArtifactItemInstance GetArtifactOfType(ArtifactItemDefinition definition)
	{
		foreach (ArtifactItemInstance artifactItem in m_artifactItems)
		{
			if (artifactItem.ItemDefinition == definition)
			{
				return artifactItem;
			}
		}
		return null;
	}

	public void ClearTempUseItem()
	{
		m_tempUseItem = null;
	}

	public void UseTempQuickItem()
	{
		UseItem(m_tempUseItem);
		m_tempUseItem = null;
	}

	public override int GetAvailableLastRowSlots()
	{
		Vector2Int baseInventorySize = m_baseInventorySize;
		int num = 0;
		foreach (ItemInstance inventoryUpgrade in m_inventoryUpgrades)
		{
			InventoryUpgradeItemDefinition inventoryUpgradeItemDefinition = inventoryUpgrade.ItemDefinition as InventoryUpgradeItemDefinition;
			if (inventoryUpgradeItemDefinition != null)
			{
				num += inventoryUpgradeItemDefinition.AddedSlots;
			}
		}
		if (num > m_maxUpgradeInventorySlots)
		{
			num = m_maxUpgradeInventorySlots;
		}
		int num2 = baseInventorySize.x;
		if (num > 0)
		{
			int num3 = Mathf.CeilToInt((float)num / (float)m_baseInventorySize.x);
			baseInventorySize.y += num3;
			num2 = num % m_baseInventorySize.x;
			if (num2 == 0)
			{
				return -1;
			}
		}
		return num2;
	}

	public override bool IsSlotLocked(Vector2Int position)
	{
		Vector2Int inventorySize = InventorySize;
		int availableLastRowSlots = GetAvailableLastRowSlots();
		if (availableLastRowSlots != -1 && position.y == inventorySize.y - 1)
		{
			return position.x >= availableLastRowSlots;
		}
		return false;
	}

	public ProjectileWeaponItemInstance GetEquippedRangedItemIncludingQueued()
	{
		if (QueuedPrimaryItem != null)
		{
			return QueuedPrimaryItem as ProjectileWeaponItemInstance;
		}
		return EquippedRangedItem;
	}

	public bool IsItemEquipped(ItemInstance itemInstance)
	{
		if (EquippedMeleeItem != itemInstance)
		{
			return EquippedRangedItem == itemInstance;
		}
		return true;
	}

	public bool HasSeenItemType(ItemDefinition itemDefinition)
	{
		return m_seenItemTypes.Contains(itemDefinition);
	}

	public ItemInstance GetNewInventoryUpgrade()
	{
		return m_newInventoryUpgrade;
	}

	public void ClearNewInventoryUpgrades()
	{
		m_newInventoryUpgrade = null;
	}

	public override void Clear()
	{
		base.Clear();
		m_money = 0;
		m_pickedUpMoney = false;
		m_equippedPrimaryItem = null;
		m_equippedMeleeItem = null;
		m_artifactItems = new List<ArtifactItemInstance>();
		m_itemShortcuts = new List<ItemDefinition>();
		m_seenItemTypes = new List<ItemDefinition>();
		m_specialItems = new List<ItemInstance>();
		m_inventoryUpgrades = new List<ItemInstance>();
		m_newInventoryUpgrade = null;
		m_equippedArtifacts = new List<ItemInstance>();
		m_maxEquippedArtifactsCount = 0;
	}

	protected override bool ContainsItem(ItemInstance item)
	{
		if (m_specialItems.Contains(item))
		{
			return true;
		}
		return base.ContainsItem(item);
	}

	public ItemInstance GetRangedWeaponForAim()
	{
		if (m_equippedPrimaryItem == null)
		{
			ItemInstance itemOfClass = GetItemOfClass<ProjectileWeaponItemInstance>();
			if (itemOfClass != null)
			{
				EquipRangedItem(itemOfClass);
			}
		}
		return m_equippedPrimaryItem;
	}

	public ItemInstance GetMeleeWeaponForAttack()
	{
		if (m_equippedMeleeItem == null)
		{
			ItemInstance itemOfClass = GetItemOfClass<MeleeWeaponItemInstance>();
			if (itemOfClass != null)
			{
				EquipMeleeItem(itemOfClass, toggle: false);
			}
		}
		return m_equippedMeleeItem;
	}

	public void ToggleItemOnShortcutItemWheel(ItemDefinition itemDefinition)
	{
		if (m_itemShortcuts.Contains(itemDefinition))
		{
			m_itemShortcuts.Remove(itemDefinition);
		}
		else
		{
			m_itemShortcuts.Add(itemDefinition);
		}
	}

	public void PerformShortcut(ItemDefinition itemDefinition)
	{
		ItemInstance itemInstance = null;
		if (itemDefinition != null)
		{
			ItemInstance itemOfType = GetItemOfType(itemDefinition);
			if (itemOfType != null)
			{
				if (itemOfType.CanBeActivated())
				{
					itemOfType.Activated = !itemOfType.Activated;
				}
				else if (itemOfType.CanUse(this, checkConditions: true))
				{
					if (itemOfType.ItemDefinition.UsingItemHasState)
					{
						GlobalReferences.Instance.Anchors.Inventory.QueuedUseItemInstanceAnchor.Set(itemOfType);
					}
					else
					{
						TryUseItemFromInventory(itemOfType);
					}
				}
				else if (itemOfType.CanEquip(this) && !IsItemEquipped(itemOfType))
				{
					itemInstance = itemOfType;
					RequestEquipItem(itemOfType);
				}
			}
		}
		if (itemInstance != null)
		{
			m_inventoryShortcutUsedEventChannel.Raise(itemInstance);
		}
	}

	public bool HasShortcutSet(ItemDefinition itemDefinition)
	{
		return false;
	}

	public bool CanAfford(int cost)
	{
		return m_money >= cost;
	}

	public void AddMoney(int money)
	{
		if (money > 0)
		{
			m_money += money;
			if (m_onMoneyChangedEventChannel != null)
			{
				m_onMoneyChangedEventChannel.Raise(m_money);
			}
		}
	}

	public void RemoveMoney(int money)
	{
		if (money > 0)
		{
			m_money -= money;
			if (m_onMoneyChangedEventChannel != null)
			{
				m_onMoneyChangedEventChannel.Raise(m_money);
			}
		}
	}

	public bool HasSeenMoney()
	{
		return m_pickedUpMoney;
	}

	public void SetHasPickedUpMoney()
	{
		m_pickedUpMoney = true;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if ((bool)m_addMoneyEventChannel)
		{
			m_addMoneyEventChannel.Register(AddMoney);
		}
		if ((bool)m_requestEquipItemEventChannel)
		{
			m_requestEquipItemEventChannel.Register(RequestEquipItem);
		}
		if ((bool)m_setMaxEquippedArtifactsEventChannel)
		{
			m_setMaxEquippedArtifactsEventChannel.Register(SetMaxEquippedArtifactsCount);
		}
		if ((bool)m_upgradeMaxArtifactCountEventChannel)
		{
			m_upgradeMaxArtifactCountEventChannel.Register(UnlockNewArtifactSlot);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if ((bool)m_addMoneyEventChannel)
		{
			m_addMoneyEventChannel.Unregister(AddMoney);
		}
		if ((bool)m_requestEquipItemEventChannel)
		{
			m_requestEquipItemEventChannel.Unregister(RequestEquipItem);
		}
		if ((bool)m_setMaxEquippedArtifactsEventChannel)
		{
			m_setMaxEquippedArtifactsEventChannel.Unregister(SetMaxEquippedArtifactsCount);
		}
		if ((bool)m_upgradeMaxArtifactCountEventChannel)
		{
			m_upgradeMaxArtifactCountEventChannel.Unregister(UnlockNewArtifactSlot);
		}
	}

	private void RequestEquipItem(ItemInstance item)
	{
		if (item is ProjectileWeaponItemInstance)
		{
			EquipRangedItem(item);
		}
		else if (item is MeleeWeaponItemInstance)
		{
			EquipMeleeItem(item);
		}
		else if (item is ArtifactItemInstance)
		{
			EquipArtifactItem(item);
		}
	}

	public void EquipRangedItem(ItemInstance item, bool toggle = true)
	{
		if (item != null && !(item.ItemDefinition is ProjectileWeaponItemDefinition) && !(item.ItemDefinition is SecondaryWeaponItemDefinition))
		{
			return;
		}
		ItemInstance item2 = item;
		if (item == null)
		{
			item2 = m_equippedPrimaryItem;
		}
		if (m_equippedPrimaryItem == item && toggle)
		{
			m_equippedPrimaryItem = null;
		}
		else
		{
			m_equippedPrimaryItem = item;
		}
		if (m_equippedPrimaryItem != null && item.ItemDefinition is ProjectileWeaponItemDefinition projectileWeaponItemDefinition && m_dynamicHintEventChannel != null)
		{
			m_dynamicHintEventChannel.Raise(DynamicHintEvent.EquippedRangedWeapon);
			foreach (ItemInstance item3 in m_items)
			{
				if (item3.ItemDefinition is AmmunitionItemDefinition ammunition && projectileWeaponItemDefinition.AcceptsAmmunition(ammunition))
				{
					m_dynamicHintEventChannel.Raise(DynamicHintEvent.PickedUpAmmoReload);
				}
			}
		}
		m_queuedPrimaryItem = null;
		GlobalReferences.Instance.EventChannels.Inventory.EquippedRangedItemChanged.Raise(new ItemEquippedEventData(item2, m_equippedPrimaryItem != null));
	}

	public List<ItemInstance> GetCyclableWeapons(out int currentIndex)
	{
		currentIndex = 0;
		List<ItemInstance> list = new List<ItemInstance>();
		List<ItemDefinition> list2 = new List<ItemDefinition>();
		ItemInstance itemInstance = ((QueuedPrimaryItem != null) ? QueuedPrimaryItem : m_equippedPrimaryItem);
		foreach (ItemInstance item in m_items)
		{
			if (!list2.Contains(item.ItemDefinition) && item is ProjectileWeaponItemInstance projectileWeaponItemInstance)
			{
				if (projectileWeaponItemInstance == itemInstance)
				{
					currentIndex = list.Count;
				}
				list.Add(projectileWeaponItemInstance);
				list2.Add(projectileWeaponItemInstance.ItemDefinition);
			}
		}
		return list;
	}

	public bool CycleNextProjectileWeapon(bool next, bool instant)
	{
		ItemInstance nextWeaponToCycleTo = GetNextWeaponToCycleTo(next);
		if (nextWeaponToCycleTo != null)
		{
			if (instant)
			{
				RequestEquipItem(nextWeaponToCycleTo);
			}
			else
			{
				m_queuedPrimaryItem = nextWeaponToCycleTo;
			}
			GlobalReferences.Instance.EventChannels.Inventory.WeaponCycled.Raise();
			return true;
		}
		return false;
	}

	public void SwapToQueuedWeapon()
	{
		if (m_queuedPrimaryItem != EquippedPrimaryItem)
		{
			RequestEquipItem(m_queuedPrimaryItem);
		}
		else
		{
			m_queuedPrimaryItem = null;
		}
	}

	public ItemInstance GetNextWeaponToCycleTo(bool next)
	{
		int currentIndex;
		List<ItemInstance> cyclableWeapons = GetCyclableWeapons(out currentIndex);
		if (cyclableWeapons.Count > 1)
		{
			if (next)
			{
				currentIndex++;
				currentIndex %= cyclableWeapons.Count;
			}
			else
			{
				currentIndex--;
				if (currentIndex < 0)
				{
					currentIndex = cyclableWeapons.Count - 1;
				}
			}
			return cyclableWeapons[currentIndex];
		}
		return null;
	}

	public bool CanCycleProjectileWeapon()
	{
		int currentIndex;
		return GetCyclableWeapons(out currentIndex).Count > 1;
	}

	private void EquipRangedItemOfType(ItemDefinition type)
	{
		foreach (ItemInstance item in m_items)
		{
			if (item.ItemDefinition == type)
			{
				EquipRangedItem(item);
				break;
			}
		}
	}

	public void EquipMeleeItem(ItemInstance item, bool toggle = true)
	{
		if (item != null && !(item.ItemDefinition is MeleeWeaponItemDefinition))
		{
			Debug.LogError("Tried to equip item " + item.ItemDefinition.ItemName + "as melee weapon but it isn't a melee weapon!");
			return;
		}
		ItemInstance item2 = item;
		if (item == null)
		{
			item2 = m_equippedMeleeItem;
		}
		if (m_equippedMeleeItem == item && toggle)
		{
			m_equippedMeleeItem = null;
		}
		else
		{
			m_equippedMeleeItem = item;
		}
		if (m_equippedMeleeItem != null && m_dynamicHintEventChannel != null)
		{
			m_dynamicHintEventChannel.Raise(DynamicHintEvent.EquippedMeleeWeapon);
		}
		GlobalReferences.Instance.EventChannels.Inventory.EquippedMeleeItemChanged.Raise(new ItemEquippedEventData(item2, m_equippedMeleeItem != null));
	}

	public override ItemInstance AddItemToInventory(ItemDefinition itemDefinition, int amount, Vector2Int position, bool rotated, bool autoEquip, out int amountRemaining, bool autoAddShortcut = false, int amountInItemOverride = -1)
	{
		amountRemaining = amount;
		if (itemDefinition is ArtifactItemDefinition)
		{
			bool flag = m_artifactItems.Count > 0;
			amountRemaining = 0;
			foreach (ArtifactItemInstance artifactItem in m_artifactItems)
			{
				if (artifactItem.ItemDefinition == itemDefinition)
				{
					Debug.LogWarning("Tried to add an artifact item that we already have. Ignoring...");
					return artifactItem;
				}
			}
			ArtifactItemInstance artifactItemInstance = (ArtifactItemInstance)Inventory.CreateItemInstance(itemDefinition, position, rotated, amountRemaining, amountInItemOverride);
			m_artifactItems.Add(artifactItemInstance);
			GlobalReferences.Instance.EventChannels.Inventory.ArtifactItemCollected.Raise(artifactItemInstance);
			if (!flag)
			{
				GlobalReferences.Instance.EventChannels.Inventory.OnFirstArtifactPickedUp.Raise();
			}
			GlobalReferences.Instance.Achievements.FindAllMementos.UnlockIfValueReached(m_artifactItems.Count);
			return artifactItemInstance;
		}
		if (itemDefinition is InventoryUpgradeItemDefinition)
		{
			ItemInstance itemInstance = new ItemInstance(itemDefinition, amountRemaining);
			amountRemaining = 0;
			m_inventoryUpgrades.Add(itemInstance);
			int num = 0;
			foreach (ItemInstance inventoryUpgrade in m_inventoryUpgrades)
			{
				InventoryUpgradeItemDefinition inventoryUpgradeItemDefinition = inventoryUpgrade.ItemDefinition as InventoryUpgradeItemDefinition;
				if (inventoryUpgradeItemDefinition != null)
				{
					num += inventoryUpgradeItemDefinition.AddedSlots;
				}
			}
			if (num > m_maxUpgradeInventorySlots)
			{
				Debug.LogError("Too many inventory upgrades! Player now has more added slots than is specified in maxUpgradeInventorySlots");
			}
			if (num >= m_maxUpgradeInventorySlots)
			{
				GlobalReferences.Instance.Achievements.MaxInventoryUpgrades.UnlockAchievement();
			}
			return itemInstance;
		}
		foreach (SpecialItemSlot specialItemSlot in m_specialItemSlots)
		{
			if (!(specialItemSlot.m_itemType == itemDefinition))
			{
				continue;
			}
			foreach (ItemInstance specialItem in m_specialItems)
			{
				if (specialItem.ItemDefinition == itemDefinition)
				{
					Debug.LogError("Tried to add a special item that we already have " + itemDefinition.name + " - ignoring...");
					return specialItem;
				}
			}
			ItemInstance itemInstance2 = Inventory.CreateItemInstance(itemDefinition, specialItemSlot.m_virtualPosition, rotated: false, amountRemaining, 0);
			amountRemaining = 0;
			m_specialItems.Add(itemInstance2);
			return itemInstance2;
		}
		if (!m_seenItemTypes.Contains(itemDefinition))
		{
			m_seenItemTypes.Add(itemDefinition);
		}
		if (itemDefinition.UseEffects != null && itemDefinition.UseEffects.Count > 0)
		{
			foreach (ItemEffect useEffect in itemDefinition.UseEffects)
			{
				if (useEffect is RepairMeleeItemEffect)
				{
					CheckForMeleeWeaponDamagedHintTrigger(fromRepairItem: true);
					break;
				}
			}
		}
		if (itemDefinition is AmmunitionItemDefinition ammunition)
		{
			ProjectileWeaponItemInstance equippedRangedItem = EquippedRangedItem;
			if (equippedRangedItem != null && (equippedRangedItem.ItemDefinition as ProjectileWeaponItemDefinition).AcceptsAmmunition(ammunition))
			{
				m_dynamicHintEventChannel.Raise(DynamicHintEvent.PickedUpAmmoReload);
			}
		}
		GlobalReferences.Instance.Variables.Mechanics.PV_InventoryDisabled.SetValue(value: false);
		return base.AddItemToInventory(itemDefinition, amount, position, rotated, autoEquip, out amountRemaining, autoAddShortcut, amountInItemOverride);
	}

	public void AutoReloadEquippedRangedItem()
	{
		if (m_equippedPrimaryItem != null && m_equippedPrimaryItem is ProjectileWeaponItemInstance weaponItem)
		{
			AutoReloadWeapon(weaponItem);
		}
	}

	public void AutoReloadWeapon(ProjectileWeaponItemInstance weaponItem)
	{
		int num = Mathf.Min(weaponItem.GetEmptyAmmoCapacity(), weaponItem.GetAutoReloadAmount());
		AmmunitionItemDefinition loadedAmmoType = weaponItem.LoadedAmmoType;
		int num2 = m_items.Count - 1;
		while (num2 >= 0 && num > 0)
		{
			if (m_items[num2].ItemDefinition == loadedAmmoType)
			{
				num = ReloadWeaponWithAmmo(weaponItem, m_items[num2], num);
			}
			num2--;
		}
	}

	public bool CanReloadEquippedItem()
	{
		if (m_equippedPrimaryItem != null && m_equippedPrimaryItem is ProjectileWeaponItemInstance projectileWeaponItemInstance)
		{
			int emptyAmmoCapacity = projectileWeaponItemInstance.GetEmptyAmmoCapacity();
			if (emptyAmmoCapacity <= 0)
			{
				return false;
			}
			AmmunitionItemDefinition loadedAmmoType = projectileWeaponItemInstance.LoadedAmmoType;
			int num = 0;
			int num2 = m_items.Count - 1;
			while (num2 >= 0 && emptyAmmoCapacity > 0)
			{
				ItemInstance itemInstance = m_items[num2];
				if (itemInstance.ItemDefinition == loadedAmmoType)
				{
					num += itemInstance.StackSize;
				}
				num2--;
			}
			return num > 0;
		}
		return false;
	}

	public void ConsumeSecondaryWeaponItem()
	{
		ItemInstance equippedSecondaryItem = EquippedSecondaryItem;
		int currentIndex;
		List<ItemInstance> cyclableWeapons = GetCyclableWeapons(out currentIndex);
		if (equippedSecondaryItem != null)
		{
			RemoveItem(equippedSecondaryItem, 1);
		}
		if (EquippedSecondaryItem == null && cyclableWeapons.Count >= 2)
		{
			currentIndex++;
			currentIndex %= cyclableWeapons.Count;
			RequestEquipItem(cyclableWeapons[currentIndex]);
		}
	}

	public void ConsumeItem(ItemInstance itemInstance)
	{
		RemoveItem(itemInstance, 1);
	}

	public bool ApplyDurabilityLossToEquippedItem(float amount, MeleeWeaponItemDefinition itemType)
	{
		bool flag = false;
		if (m_equippedMeleeItem != null && m_equippedMeleeItem.ItemDefinition == itemType)
		{
			amount *= GameDifficultyManager.CurrentDifficulty.DurabilityLossScalar;
			if (m_equippedMeleeItem is MeleeWeaponItemInstance meleeWeaponItemInstance)
			{
				int durabilityPercent = meleeWeaponItemInstance.DurabilityPercent;
				flag = meleeWeaponItemInstance.RemoveDurability(amount);
				if (flag)
				{
					m_dynamicHintEventChannel.Raise(DynamicHintEvent.MeleeWeaponBroke);
					GlobalReferences.Instance.EventChannels.Inventory.MeleeWeaponItemBroken.Raise(meleeWeaponItemInstance);
				}
				else if (meleeWeaponItemInstance.DurabilityPercent <= 25 && durabilityPercent > 25)
				{
					GlobalReferences.Instance.EventChannels.Inventory.MeleeWeaponItemDurabilityLow.Raise(meleeWeaponItemInstance);
				}
				CheckForMeleeWeaponDamagedHintTrigger(fromRepairItem: false);
			}
		}
		return flag;
	}

	private void CheckForMeleeWeaponDamagedHintTrigger(bool fromRepairItem)
	{
		bool flag = false;
		bool flag2 = fromRepairItem;
		foreach (ItemInstance item in m_items)
		{
			if (!flag && item is MeleeWeaponItemInstance meleeWeaponItemInstance && meleeWeaponItemInstance.DurabilityProportion <= 0.6f)
			{
				flag = true;
			}
			if (!flag2 && item.ItemDefinition.UseEffects != null)
			{
				foreach (ItemEffect useEffect in item.ItemDefinition.UseEffects)
				{
					if (useEffect is RepairMeleeItemEffect)
					{
						flag2 = true;
						break;
					}
				}
			}
			if (flag && flag2)
			{
				break;
			}
		}
		if (flag && flag2)
		{
			m_dynamicHintEventChannel.Raise(DynamicHintEvent.CanRepairMelee);
		}
	}

	public bool HasAnyHealingItem()
	{
		foreach (ItemInstance item in m_items)
		{
			if (m_healingItems.Contains(item.ItemDefinition))
			{
				return true;
			}
		}
		return false;
	}

	protected override void ForceUnequipItem(ItemInstance item)
	{
		base.ForceUnequipItem(item);
		if (m_equippedMeleeItem == item)
		{
			EquipMeleeItem(null);
		}
		else if (m_equippedPrimaryItem == item)
		{
			EquipRangedItem(null);
		}
	}

	protected override void TryLoadWeapon(ProjectileWeaponItemInstance weaponItem, ItemInstance item)
	{
		if (GlobalReferences.Instance.Anchors.Inventory.ItemBoxInteractableAnchor.Item != null)
		{
			base.TryLoadWeapon(weaponItem, item);
		}
		else
		{
			if (!(item.ItemDefinition is AmmunitionItemDefinition) || !weaponItem.ItemDefinition.CanBeCombinedWith(item.ItemDefinition))
			{
				return;
			}
			if (m_equippedPrimaryItem != weaponItem)
			{
				EquipRangedItem(weaponItem, toggle: false);
			}
			if (weaponItem.LoadedAmmoType != item.ItemDefinition)
			{
				if (!UnloadWeapon(weaponItem))
				{
					Debug.LogError("Failed to unload weapon in TryLoadWeapon! This should not happen, no inventory space?");
				}
				weaponItem.SwitchAmmoType(item.ItemDefinition as AmmunitionItemDefinition, 0);
			}
			GlobalReferences.Instance.EventChannels.Inventory.RequestCharacterReloadWeapon.Raise();
		}
	}

	protected override void TryRepairItem(MeleeWeaponItemInstance meleeWeapon, ItemInstance repairItem)
	{
		if (GlobalReferences.Instance.Anchors.Inventory.ItemBoxInteractableAnchor.Item != null)
		{
			base.TryRepairItem(meleeWeapon, repairItem);
			return;
		}
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
			EquipMeleeItem(meleeWeapon, toggle: false);
			GlobalReferences.Instance.Anchors.Inventory.QueuedUseItemInstanceAnchor.Set(repairItem);
		}
		else
		{
			Debug.LogError("Tried to repair with a repair item" + repairItem.ItemName + " that doesn't have a repair use effect!");
		}
	}

	public ItemInstance PickupThrownMeleeWeapon(ThrownMeleeWeapon meleeWeapon)
	{
		EquipMeleeItem(meleeWeapon.WeaponItemInstance);
		return meleeWeapon.WeaponItemInstance;
	}

	protected override void OnItemRemoved(ItemInstance itemInstance)
	{
		bool flag = false;
		if (m_equippedPrimaryItem == itemInstance)
		{
			EquipRangedItem(null);
			flag = true;
		}
		if (m_equippedMeleeItem == itemInstance)
		{
			EquipMeleeItem(null);
		}
		if (itemInstance.ItemDefinition.CanBeActivated())
		{
			itemInstance.Activated = false;
		}
		if (flag)
		{
			EquipRangedItemOfType(itemInstance.ItemDefinition);
		}
		if (m_equippedArtifacts.Contains(itemInstance))
		{
			EquipArtifactItem(itemInstance);
		}
	}

	protected override void AutoEquipItem(ItemInstance newItem)
	{
		base.AutoEquipItem(newItem);
		if (!newItem.CanEquip(this))
		{
			return;
		}
		if (newItem is ProjectileWeaponItemInstance)
		{
			if (m_equippedPrimaryItem == null)
			{
				EquipRangedItem(newItem);
			}
		}
		else if (newItem is MeleeWeaponItemInstance && m_equippedMeleeItem == null)
		{
			EquipMeleeItem(newItem);
		}
	}

	protected override void AutoAddShortcutForItem(ItemInstance newItem)
	{
		if (newItem.ItemDefinition.CanHaveShortcutSet(this) && !m_itemShortcuts.Contains(newItem.ItemDefinition))
		{
			m_itemShortcuts.Add(newItem.ItemDefinition);
		}
	}

	public void PickupItem(IItemPickup item)
	{
		int itemAmount = item.ItemAmount;
		AddItemEventData addItemEventData = new AddItemEventData();
		addItemEventData.m_itemDefinition = item.ItemDefinition;
		addItemEventData.m_supressNotification = item.SuppressNotification;
		int amountRemaining;
		ItemInstance newInventoryUpgrade = AddItemToInventory(item.ItemDefinition, item.ItemAmount, new Vector2Int(-1, -1), rotated: false, autoEquip: false, out amountRemaining, autoAddShortcut: false, item.AmountInItem);
		item.ItemAmount = amountRemaining;
		addItemEventData.m_itemAmount = itemAmount - amountRemaining;
		if (addItemEventData.m_itemAmount > 0)
		{
			AudioEvent onAddedToInventory = item.ItemDefinition.Audio.OnAddedToInventory;
			if (onAddedToInventory != null)
			{
				AudioEvent.Play2D(onAddedToInventory);
			}
			GlobalReferences.Instance.EventChannels.Inventory.ItemPickedUp.Raise(addItemEventData);
		}
		if (item.ItemDefinition is InventoryUpgradeItemDefinition)
		{
			m_newInventoryUpgrade = newInventoryUpgrade;
		}
	}

	public void QuickUseItem(IItemPickup item, bool useItemFSMState)
	{
		ItemInstance itemInstance = new ItemInstance(item.ItemDefinition, item.ItemAmount);
		item.ItemAmount = 0;
		GlobalReferences.Instance.EventChannels.Inventory.QuickUseOnPickupItem.Raise(itemInstance);
		if (useItemFSMState && itemInstance.ItemDefinition.UsingItemHasState)
		{
			m_tempUseItem = itemInstance;
			GlobalReferences.Instance.Anchors.Inventory.QueuedUseItemInstanceAnchor.Set(itemInstance);
		}
		else
		{
			UseItem(itemInstance);
		}
	}

	protected override void ReplaceWithUsedItem(ItemInstance itemInstance)
	{
		if (m_tempUseItem == itemInstance)
		{
			ItemDefinition useResultChangeItemDefinition = itemInstance.ItemDefinition.UseResultChangeItemDefinition;
			if (CanAddItemToInventory(useResultChangeItemDefinition, 1))
			{
				AddItemToInventory(useResultChangeItemDefinition, 1, new Vector2Int(-1, -1), rotated: false, autoEquip: false, out var _);
			}
			else if (useResultChangeItemDefinition.IsDroppable)
			{
				GameObject item = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item;
				if (item != null)
				{
					ItemInstance itemInstance2 = Inventory.CreateItemInstance(useResultChangeItemDefinition, new Vector2Int(-1, -1), rotated: false, 1, 0);
					CharacterInventory component = item.GetComponent<CharacterInventory>();
					if (component != null)
					{
						Debug.Log("Spawning dropped item due to no space in inventory " + itemInstance2.ItemName);
						component.SpawnDroppedItem(itemInstance2);
					}
				}
			}
			else
			{
				Debug.LogError("Tried to create new Item " + useResultChangeItemDefinition.name + " from using item " + itemInstance.ItemName + " but there is no inventory space and the new item isn't droppable!");
			}
		}
		else
		{
			base.ReplaceWithUsedItem(itemInstance);
		}
	}

	public bool ValidateItemDrop(ItemInstance item)
	{
		if (m_items.Contains(item))
		{
			return true;
		}
		return false;
	}

	public override bool IsItemMovable(ItemInstance itemInstance)
	{
		if (m_specialItems.Contains(itemInstance))
		{
			return false;
		}
		return base.IsItemMovable(itemInstance);
	}

	public bool HasEquippableRangedWeapon()
	{
		return GetItemOfClass<ProjectileWeaponItemInstance>() != null;
	}

	private int GetFreeArtifactSlots()
	{
		int num = 0;
		for (int i = 0; i < m_maxEquippedArtifactsCount; i++)
		{
			if (i >= m_equippedArtifacts.Count || m_equippedArtifacts[i] == null)
			{
				num++;
			}
		}
		return num;
	}

	public int GetEquippedArtifactCount()
	{
		int num = 0;
		foreach (ItemInstance equippedArtifact in m_equippedArtifacts)
		{
			if (equippedArtifact != null)
			{
				num++;
			}
		}
		return num;
	}

	public bool CanEquipArtifact(ItemInstance itemInstance)
	{
		if (itemInstance != null)
		{
			if (GetFreeArtifactSlots() <= 0)
			{
				return m_equippedArtifacts.Contains(itemInstance);
			}
			return true;
		}
		return false;
	}

	public bool IsArtifactEquipped(ItemInstance artifact)
	{
		foreach (ItemInstance equippedArtifact in m_equippedArtifacts)
		{
			if (equippedArtifact == artifact)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsArtifactEquipped(ItemDefinition artifactDefinition)
	{
		foreach (ItemInstance equippedArtifact in m_equippedArtifacts)
		{
			if (equippedArtifact != null && equippedArtifact.ItemDefinition == artifactDefinition)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasActiveArtifactEffect(ArtifactEffectDefinition definition)
	{
		float floatValue;
		int integerValue;
		return HasActiveArtifactEffect(definition, out floatValue, out integerValue);
	}

	public bool HasActiveArtifactEffect(ArtifactEffectDefinition definition, out int intValue)
	{
		float floatValue;
		return HasActiveArtifactEffect(definition, out floatValue, out intValue);
	}

	public bool HasActiveArtifactEffect(ArtifactEffectDefinition definition, out float floatValue)
	{
		int integerValue;
		return HasActiveArtifactEffect(definition, out floatValue, out integerValue);
	}

	public bool HasActiveArtifactEffect(ArtifactEffectDefinition definition, out float floatValue, out int integerValue)
	{
		List<ArtifactEffectInstance> list = new List<ArtifactEffectInstance>();
		foreach (ItemInstance equippedArtifact in m_equippedArtifacts)
		{
			if (equippedArtifact != null)
			{
				ArtifactEffectInstance matchingArtifactEffect = (equippedArtifact as ArtifactItemInstance).ArtifactDefinition.GetMatchingArtifactEffect(definition);
				if (matchingArtifactEffect != null)
				{
					list.Add(matchingArtifactEffect);
				}
			}
		}
		if (list.Count > 0)
		{
			floatValue = list[0].FloatValue;
			integerValue = list[0].IntegerValue;
			for (int i = 1; i < list.Count; i++)
			{
				switch (list[i].EffectDefinition.StackBehavior)
				{
				case ArtifactEffectDefinition.EffectStackBehavior.TakeHighest:
					floatValue = Mathf.Max(list[i].FloatValue, floatValue);
					integerValue = Mathf.Max(list[i].IntegerValue, integerValue);
					break;
				case ArtifactEffectDefinition.EffectStackBehavior.Additive:
					floatValue += list[i].FloatValue;
					integerValue += list[i].IntegerValue;
					break;
				}
			}
			return true;
		}
		floatValue = 0f;
		integerValue = 0;
		return false;
	}

	public void EquipArtifactItem(ItemInstance item)
	{
		if (item != null && !(item.ItemDefinition is ArtifactItemDefinition))
		{
			Debug.LogError("Tried to equip item " + item.ItemDefinition.ItemName + "as an artifact but it isn't an artifact!");
		}
		else if (m_equippedArtifacts.Contains(item))
		{
			m_equippedArtifacts[m_equippedArtifacts.IndexOf(item)] = null;
			GlobalReferences.Instance.EventChannels.Inventory.EquippedArtifactsChanged.Raise(new ItemEquippedEventData(item, equipped: false));
		}
		else if (GetFreeArtifactSlots() > 0)
		{
			bool flag = false;
			for (int i = 0; i < m_equippedArtifacts.Count; i++)
			{
				if (m_equippedArtifacts[i] == null)
				{
					flag = true;
					m_equippedArtifacts[i] = item;
					break;
				}
			}
			if (!flag)
			{
				m_equippedArtifacts.Add(item);
			}
			GlobalReferences.Instance.EventChannels.Inventory.EquippedArtifactsChanged.Raise(new ItemEquippedEventData(item, equipped: true));
		}
		else
		{
			Debug.LogError("Too many equipped artifacts! Failed to equip artifact " + item.ItemDefinition.name);
		}
	}

	public void EquipArtifactItemInSlot(ItemInstance item, int slot)
	{
		if (slot >= m_maxEquippedArtifactsCount)
		{
			return;
		}
		while (m_equippedArtifacts.Count <= slot)
		{
			m_equippedArtifacts.Add(null);
		}
		if (m_equippedArtifacts[slot] != item)
		{
			if (m_equippedArtifacts[slot] != null)
			{
				ItemInstance item2 = m_equippedArtifacts[slot];
				m_equippedArtifacts[slot] = null;
				GlobalReferences.Instance.EventChannels.Inventory.EquippedArtifactsChanged.Raise(new ItemEquippedEventData(item2, equipped: false));
			}
			if (IsArtifactEquipped(item))
			{
				EquipArtifactItem(item);
			}
			m_equippedArtifacts[slot] = item;
			GlobalReferences.Instance.EventChannels.Inventory.EquippedArtifactsChanged.Raise(new ItemEquippedEventData(item, equipped: true));
		}
	}

	public void PowerUpArtifactOfType(ArtifactItemDefinition artifact)
	{
		ItemInstance itemOfType = GetItemOfType(artifact);
		if (itemOfType != null && itemOfType is ArtifactItemInstance artifactItemInstance)
		{
			artifactItemInstance.PowerUp();
		}
	}

	private void SetMaxEquippedArtifactsCount(int artifactCount)
	{
		m_maxEquippedArtifactsCount = artifactCount;
		if (m_maxEquippedArtifactsCount == 2)
		{
			GlobalReferences.Instance.Achievements.UnlockFirstMementoSlots.UnlockAchievement();
		}
		GlobalReferences.Instance.Achievements.UnlockAllMementoSlots.UnlockIfValueReached(m_maxEquippedArtifactsCount);
	}

	private void UnlockNewArtifactSlot()
	{
		SetMaxEquippedArtifactsCount(m_maxEquippedArtifactsCount + 1);
	}

	public override ItemInstance GetItemOfType(ItemDefinition type)
	{
		foreach (ItemInstance specialItem in m_specialItems)
		{
			if (specialItem.ItemDefinition == type)
			{
				return specialItem;
			}
		}
		return base.GetItemOfType(type);
	}

	public override ItemInstance GetItemOfClass<T>()
	{
		foreach (ItemInstance specialItem in m_specialItems)
		{
			if (specialItem is T)
			{
				return specialItem;
			}
		}
		return base.GetItemOfClass<T>();
	}

	public override bool CanAddItemToInventory(ItemDefinition itemDefinition, int amount)
	{
		foreach (SpecialItemSlot specialItemSlot in m_specialItemSlots)
		{
			if (!(specialItemSlot.m_itemType == itemDefinition))
			{
				continue;
			}
			foreach (ItemInstance specialItem in m_specialItems)
			{
				if (specialItem.ItemDefinition == itemDefinition)
				{
					return false;
				}
			}
			return true;
		}
		return base.CanAddItemToInventory(itemDefinition, amount);
	}

	public int GetUsableItemCount()
	{
		int num = 0;
		foreach (ItemInstance item in ItemList)
		{
			if (item != null && !item.ItemDefinition.HideFromQuickItem && (item.CanUse(this, checkConditions: false) || item.CanBeActivated()))
			{
				num++;
			}
		}
		return num;
	}

	public override PersistentDataInventory GetPersistentData()
	{
		List<AssetReferenceT<ItemDefinition>> list = new List<AssetReferenceT<ItemDefinition>>();
		foreach (ItemDefinition itemShortcut in m_itemShortcuts)
		{
			list.Add(itemShortcut.AssetReference);
		}
		List<AssetReferenceT<ItemDefinition>> list2 = new List<AssetReferenceT<ItemDefinition>>();
		foreach (ItemDefinition seenItemType in m_seenItemTypes)
		{
			list2.Add(seenItemType.AssetReference);
		}
		return new PersistentDataMainInventory
		{
			m_equippedRangedItem = m_equippedPrimaryItem,
			m_equippedMeleeItem = m_equippedMeleeItem,
			m_artifactItems = new List<ArtifactItemInstance>(m_artifactItems),
			m_equippedArtifacts = new List<ItemInstance>(m_equippedArtifacts),
			m_maxEquippedArtifactsCount = m_maxEquippedArtifactsCount,
			m_items = new List<ItemInstance>(m_items),
			m_inventoryUpgrades = new List<ItemInstance>(m_inventoryUpgrades),
			m_specialItems = new List<ItemInstance>(m_specialItems),
			m_shortcutItemDefinitions = list,
			m_seenItemTypes = list2,
			m_money = m_money,
			m_pickedUpMoney = m_pickedUpMoney
		};
	}

	public void ReadFromPersistentData(PersistentDataMainInventory data)
	{
		m_items = new List<ItemInstance>(data.m_items);
		if (data.m_equippedRangedItem != null && data.m_equippedRangedItem.IsValid())
		{
			m_equippedPrimaryItem = data.m_equippedRangedItem;
		}
		else
		{
			m_equippedPrimaryItem = null;
		}
		if (data.m_equippedMeleeItem != null && data.m_equippedMeleeItem.IsValid())
		{
			m_equippedMeleeItem = data.m_equippedMeleeItem;
		}
		else
		{
			m_equippedMeleeItem = null;
		}
		MeleeWeaponItemInstance meleeWeaponItemInstance = m_equippedMeleeItem as MeleeWeaponItemInstance;
		_ = meleeWeaponItemInstance?.DurabilityProportion;
		GlobalReferences.Instance.EventChannels.Generic.MeleeWeaponRefresh.Raise(new MeleeWeaponDurabilityChangedData
		{
			m_meleeWeapon = meleeWeaponItemInstance
		});
		m_artifactItems = new List<ArtifactItemInstance>(data.m_artifactItems);
		m_equippedArtifacts = new List<ItemInstance>(data.m_equippedArtifacts);
		m_maxEquippedArtifactsCount = data.m_maxEquippedArtifactsCount;
		m_inventoryUpgrades = new List<ItemInstance>(data.m_inventoryUpgrades);
		m_specialItems = new List<ItemInstance>(data.m_specialItems);
		m_money = data.m_money;
		m_pickedUpMoney = data.m_pickedUpMoney;
		m_itemShortcuts = new List<ItemDefinition>();
		foreach (AssetReferenceT<ItemDefinition> shortcutItemDefinition in data.m_shortcutItemDefinitions)
		{
			m_itemShortcuts.Add(AddressablesContentManager.Instance.GetAsset(shortcutItemDefinition));
		}
		m_seenItemTypes = new List<ItemDefinition>();
		foreach (AssetReferenceT<ItemDefinition> seenItemType in data.m_seenItemTypes)
		{
			m_seenItemTypes.Add(AddressablesContentManager.Instance.GetAsset(seenItemType));
		}
	}
}
