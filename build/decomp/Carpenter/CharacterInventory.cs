using System;
using UnityEngine;
using UnityEngine.Events;

public class CharacterInventory : MonoBehaviour
{
	public enum ComplexInventoryActionsMode
	{
		None,
		All,
		SecondaryOnly
	}

	[SerializeField]
	private PlayerMainInventory m_inventory;

	[SerializeField]
	private PlayerMainInventory m_tempInventory;

	private bool m_useTempInventory;

	private ItemInstance m_queuedUseItemInstance;

	private float m_queuedItemClearTimer;

	private ItemInstance m_queuedDropItemInstance;

	private CharacterInputPlayer m_playerInput;

	private ComplexInventoryActionsMode m_complexInventoryActionsAllowed;

	private bool m_isShowingItemWheel;

	private bool m_isRequestingShowItemWheel;

	private bool m_weaponSwappingAllowed = true;

	private bool m_isShowingQuickItemPanel;

	private bool m_isRequestingQuickItemPanel;

	private bool m_instantQuickWeaponSwitch = true;

	public UnityAction OnPerformedQuickWeaponSwitch;

	private float m_lastQuicKWeaponSwitchTime;

	private SecondaryWeaponItemInstance m_usedSecondaryItem;

	public PlayerMainInventory Inventory
	{
		get
		{
			if (m_useTempInventory)
			{
				return m_tempInventory;
			}
			return m_inventory;
		}
	}

	public bool UsingTempInventory => m_useTempInventory;

	public PlayerMainInventory ArtifactInventory => m_inventory;

	public ItemInstance QueuedUseItemInstance => m_queuedUseItemInstance;

	public ItemInstance QueuedDropItemInstance => m_queuedDropItemInstance;

	public ComplexInventoryActionsMode ComplexInventoryActionsAllowed
	{
		get
		{
			return m_complexInventoryActionsAllowed;
		}
		set
		{
			if (m_complexInventoryActionsAllowed != value)
			{
				m_complexInventoryActionsAllowed = value;
				GlobalReferences.Instance.EventChannels.Inventory.AllowComplexInventoryActionsChanged.Raise(m_complexInventoryActionsAllowed != ComplexInventoryActionsMode.None);
			}
		}
	}

	public bool IsShowingItemWheel => m_isShowingItemWheel;

	public bool WeaponSwappingAllowed
	{
		get
		{
			return m_weaponSwappingAllowed;
		}
		set
		{
			m_weaponSwappingAllowed = value;
		}
	}

	public bool IsShowingQuickItemPanel => m_isShowingQuickItemPanel;

	public bool InstantQuickWeaponSwitch
	{
		get
		{
			return m_instantQuickWeaponSwitch;
		}
		set
		{
			m_instantQuickWeaponSwitch = value;
		}
	}

	public bool UsedQuickWeaponSwitchRecently => Time.time < m_lastQuicKWeaponSwitchTime + 1f;

	public SecondaryWeaponItemInstance UsedSecondaryItem
	{
		get
		{
			return m_usedSecondaryItem;
		}
		set
		{
			m_usedSecondaryItem = value;
		}
	}

	public PlayerMainInventory GetTempInventory()
	{
		return m_tempInventory;
	}

	public void EnableTempInventory()
	{
		m_tempInventory.Clear();
		m_useTempInventory = true;
	}

	public void DisableTempInventory()
	{
		m_useTempInventory = false;
	}

	private void Awake()
	{
		m_lastQuicKWeaponSwitchTime = -1000f;
		m_playerInput = GetComponent<CharacterInputPlayer>();
		CharacterInputPlayer playerInput = m_playerInput;
		playerInput.OnRequestItemWheel = (UnityAction)Delegate.Combine(playerInput.OnRequestItemWheel, new UnityAction(RequestOpenItemWheel));
		CharacterInputPlayer playerInput2 = m_playerInput;
		playerInput2.OnRequestCloseItemWheel = (UnityAction)Delegate.Combine(playerInput2.OnRequestCloseItemWheel, new UnityAction(RequestCloseItemWheel));
		CharacterInputPlayer playerInput3 = m_playerInput;
		playerInput3.OnRequestQuickItemPanel = (UnityAction<bool>)Delegate.Combine(playerInput3.OnRequestQuickItemPanel, new UnityAction<bool>(ShowQuickItemPanel));
		CharacterInputPlayer playerInput4 = m_playerInput;
		playerInput4.OnCycleNextWeapon = (UnityAction)Delegate.Combine(playerInput4.OnCycleNextWeapon, new UnityAction(CycleNextWeapon));
		CharacterInputPlayer playerInput5 = m_playerInput;
		playerInput5.OnCyclePreviousWeapon = (UnityAction)Delegate.Combine(playerInput5.OnCyclePreviousWeapon, new UnityAction(CyclePreviousWeapon));
	}

	private void OnEnable()
	{
		GlobalReferences.Instance.Anchors.Inventory.QueuedUseItemInstanceAnchor.Set(null);
		LevelMetadata item = GlobalReferences.Instance.Anchors.Generic.ActiveLevelMetadata.Item;
		if (item != null && item.UseTemporaryInventory)
		{
			EnableTempInventory();
		}
		GlobalReferences.Instance.Anchors.Inventory.QueuedUseItemInstanceAnchor.Register(OnQueuedUseItemRequest);
		GlobalReferences.Instance.EventChannels.Inventory.SelectedFromItemWheel.Register(OnSelectedItemFromWheel);
		GlobalReferences.Instance.EventChannels.Inventory.SelectedItemFromQuickItemPanel.Register(OnSelectedItemFromQuickItemPanel);
		GlobalReferences.Instance.EventChannels.Inventory.RequestDropItemInstance.Register(OnRequestDropItem);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.Anchors.Inventory.QueuedUseItemInstanceAnchor.Unregister(OnQueuedUseItemRequest);
		GlobalReferences.Instance.EventChannels.Inventory.SelectedFromItemWheel.Unregister(OnSelectedItemFromWheel);
		GlobalReferences.Instance.EventChannels.Inventory.SelectedItemFromQuickItemPanel.Unregister(OnSelectedItemFromQuickItemPanel);
		GlobalReferences.Instance.EventChannels.Inventory.RequestDropItemInstance.Unregister(OnRequestDropItem);
	}

	private void OnQueuedUseItemRequest(ItemInstance itemInstance)
	{
		if (itemInstance != null)
		{
			if (m_queuedUseItemInstance != null)
			{
				Debug.LogError("Queing a new item to be used but the current one hasn't been cleared!");
			}
			GlobalReferences.Instance.GameMenuState.ClearInMenu(GameMenuState.GameMenu.InGameMenu);
			m_queuedItemClearTimer = 1f;
			m_playerInput.ClearCrouchToggle();
			m_queuedUseItemInstance = itemInstance;
		}
	}

	public void UseQueuedItem()
	{
		if (m_queuedUseItemInstance != null)
		{
			if (m_inventory.TempUseItem == m_queuedUseItemInstance)
			{
				m_inventory.UseTempQuickItem();
			}
			else
			{
				GlobalReferences.Instance.EventChannels.Inventory.UseItem.Raise(m_queuedUseItemInstance);
			}
			ClearQueuedItem();
		}
	}

	public void ClearQueuedItem()
	{
		if (m_inventory.TempUseItem != null && m_inventory.TempUseItem == m_queuedUseItemInstance)
		{
			SpawnDroppedItem(m_inventory.TempUseItem);
			m_inventory.ClearTempUseItem();
		}
		m_queuedUseItemInstance = null;
		m_queuedDropItemInstance = null;
		GlobalReferences.Instance.Anchors.Inventory.QueuedUseItemInstanceAnchor.Set(null);
	}

	private bool ShouldDrainPower()
	{
		return !GlobalReferences.Instance.GameMenuState.IsInAnyMenu();
	}

	private void CheckPoweredItemChanges(PoweredItemInstance poweredItem, float previousChargedAmount)
	{
		if (poweredItem.ChargeAmount <= 0f)
		{
			poweredItem.Activated = false;
			GlobalReferences.Instance.EventChannels.Inventory.BatteryDepletedItem.Raise(poweredItem);
			return;
		}
		if (previousChargedAmount > GameUtils.Constants.s_lowBatteryPoint && poweredItem.ChargeAmount <= GameUtils.Constants.s_lowBatteryPoint)
		{
			GlobalReferences.Instance.EventChannels.Inventory.BatteryLowItem.Raise(poweredItem);
			return;
		}
		int s_batteryPipCount = GameUtils.Constants.s_batteryPipCount;
		for (int i = 1; i < s_batteryPipCount; i++)
		{
			float num = (float)i / (float)s_batteryPipCount * poweredItem.PoweredItemDefinition.MaxCharge;
			if (previousChargedAmount >= num && poweredItem.ChargeAmount < num)
			{
				GlobalReferences.Instance.EventChannels.Inventory.BatteryChargeLevelNotifyItem.Raise(poweredItem);
			}
		}
	}

	private void Update()
	{
		if (m_queuedUseItemInstance == null)
		{
			m_queuedItemClearTimer -= Time.deltaTime;
			if (m_queuedItemClearTimer <= 0f)
			{
				m_queuedUseItemInstance = null;
			}
		}
		if (ShouldDrainPower())
		{
			foreach (ItemInstance item in Inventory.ItemList)
			{
				if (item.Activated && item is PoweredItemInstance)
				{
					PoweredItemInstance poweredItemInstance = (PoweredItemInstance)item;
					float chargeAmount = poweredItemInstance.ChargeAmount;
					poweredItemInstance.ChargeAmount -= poweredItemInstance.PoweredItemDefinition.ActivatedChargeDrainRate * Time.deltaTime;
					CheckPoweredItemChanges(poweredItemInstance, chargeAmount);
				}
			}
		}
		bool flag = m_complexInventoryActionsAllowed != 0 && !GlobalReferences.Instance.GameMenuState.IsInAnyMenu();
		if (m_isShowingItemWheel && !flag)
		{
			m_isShowingItemWheel = false;
			GlobalReferences.Instance.EventChannels.Inventory.ShowItemWheel.Raise(value: false);
		}
		else if (m_isRequestingShowItemWheel && flag && !m_isShowingItemWheel)
		{
			m_isShowingItemWheel = true;
			GlobalReferences.Instance.EventChannels.Inventory.ShowItemWheel.Raise(value: true);
		}
		if (m_isShowingQuickItemPanel && !flag)
		{
			m_isShowingQuickItemPanel = false;
		}
		else if (m_isRequestingQuickItemPanel && flag && !m_isShowingItemWheel)
		{
			m_isShowingQuickItemPanel = true;
		}
	}

	private void RequestOpenItemWheel()
	{
		m_isRequestingShowItemWheel = true;
	}

	private void RequestCloseItemWheel()
	{
		m_isRequestingShowItemWheel = false;
		if (m_isShowingItemWheel)
		{
			m_isShowingItemWheel = false;
			GlobalReferences.Instance.EventChannels.Inventory.ShowItemWheel.Raise(value: false);
		}
	}

	private void ShowQuickItemPanel(bool showing)
	{
		m_isRequestingQuickItemPanel = showing;
		if (!showing && m_isShowingQuickItemPanel)
		{
			m_isShowingQuickItemPanel = false;
		}
	}

	private void OnSelectedItemFromWheel(ItemInstance itemInstance)
	{
		m_inventory.PerformShortcut(itemInstance.ItemDefinition);
	}

	private void OnSelectedItemFromQuickItemPanel(ItemDefinition itemDefinition)
	{
		bool flag = ComplexInventoryActionsAllowed == ComplexInventoryActionsMode.All;
		if (ComplexInventoryActionsAllowed == ComplexInventoryActionsMode.SecondaryOnly && itemDefinition is SecondaryWeaponItemDefinition)
		{
			flag = true;
		}
		if (string.IsNullOrEmpty(itemDefinition.UseFSMEvent) || flag)
		{
			m_inventory.PerformShortcut(itemDefinition);
		}
	}

	private void OnRequestDropItem(ItemInstance itemInstance)
	{
		m_queuedDropItemInstance = itemInstance;
	}

	public void DropQueuedItem()
	{
		ItemInstance queuedDropItemInstance = m_queuedDropItemInstance;
		if (m_inventory.ValidateItemDrop(queuedDropItemInstance))
		{
			SpawnDroppedItem(queuedDropItemInstance);
		}
		ClearQueuedItem();
	}

	public void SpawnDroppedItem(ItemInstance itemInstance)
	{
		int amountToDrop = itemInstance.StackSize;
		Vector3 position = base.transform.position;
		CharacterDirection component = GetComponent<CharacterDirection>();
		position.x += component.GetForwardVector().x * UnityEngine.Random.Range(0.15f, 0.25f);
		position.z += UnityEngine.Random.Range(0.05f, 0.1f);
		RemoveItemInstanceEventData value = new RemoveItemInstanceEventData
		{
			m_itemInstance = itemInstance,
			m_itemAmount = amountToDrop
		};
		GlobalReferences.Instance.EventChannels.Inventory.RequestRemoveItem.Raise(value);
		DynamicallySpawnObjectEventData eventData = new DynamicallySpawnObjectEventData(itemInstance.ItemDefinition.DroppedItemPrefab, persistent: true, position);
		ref UnityAction<GameObject> onSpawnedAction = ref eventData.m_onSpawnedAction;
		onSpawnedAction = (UnityAction<GameObject>)Delegate.Combine(onSpawnedAction, (UnityAction<GameObject>)delegate(GameObject spawnedObject)
		{
			ItemPickup component2 = spawnedObject.GetComponent<ItemPickup>();
			if (component2 != null)
			{
				component2.SetItemAmount(amountToDrop);
			}
		});
		DynamicallySpawnedObject.Spawn(eventData);
	}

	private void CyclePreviousWeapon()
	{
		if (WeaponSwappingAllowed && Inventory.CycleNextProjectileWeapon(next: false, InstantQuickWeaponSwitch))
		{
			OnPerformedQuickWeaponSwitch?.Invoke();
			m_lastQuicKWeaponSwitchTime = Time.time;
		}
	}

	private void CycleNextWeapon()
	{
		if (WeaponSwappingAllowed && Inventory.CycleNextProjectileWeapon(next: true, InstantQuickWeaponSwitch))
		{
			OnPerformedQuickWeaponSwitch?.Invoke();
			m_lastQuicKWeaponSwitchTime = Time.time;
		}
	}

	public bool HasQueuedWeaponSwap()
	{
		if (Inventory.QueuedPrimaryItem != null && Inventory.QueuedPrimaryItem != Inventory.EquippedPrimaryItem)
		{
			return true;
		}
		return false;
	}

	public void PerformWeaponSwap()
	{
		Inventory.SwapToQueuedWeapon();
	}
}
