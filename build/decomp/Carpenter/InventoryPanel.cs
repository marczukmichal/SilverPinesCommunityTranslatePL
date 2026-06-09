using System.Collections;
using System.Collections.Generic;
using Shapes2D;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class InventoryPanel : MonoBehaviour, IMoveHandler, IEventSystemHandler, ISelectHandler, IDeselectHandler
{
	public enum Mode
	{
		Normal,
		Move,
		Combine,
		ViewOnly,
		Menu,
		KeyRingView
	}

	public enum TryPlaceItemResult
	{
		OK,
		Failed,
		PickUpNewItem
	}

	[SerializeField]
	private InventoryItemCell m_inventoryItemCellPrefab;

	[SerializeField]
	private RectTransform m_cursorSelectionHighlight;

	[SerializeField]
	private Inventory m_trackedInventory;

	[SerializeField]
	private Inventory m_otherInventory;

	[SerializeField]
	private bool m_isMainInventoryPanel;

	[SerializeField]
	private InventoryPanel m_otherInventoryPanel;

	[SerializeField]
	private float m_gridSpacing;

	[SerializeField]
	private float m_margin;

	[SerializeField]
	private float m_specialMargin;

	[SerializeField]
	private ItemPickupAnchor m_activeItemPickupAnchor;

	[SerializeField]
	private ApplyItemAnchor m_activeApplyItemInteractable;

	[SerializeField]
	private CanvasGroup m_panelCanvasGroup;

	[SerializeField]
	private CanvasGroup m_fullInventoryCanvasGroup;

	[SerializeField]
	private RectTransform m_actionsContainer;

	[SerializeField]
	private InputState m_inputState;

	[SerializeField]
	private GraphicRaycaster m_graphicRaycaster;

	[Header("Mode Components")]
	[SerializeField]
	private InventoryPanelMoveMode m_moveMode;

	[SerializeField]
	private InventoryPanelCombineMode m_combineMode;

	[SerializeField]
	private InventoryItemPickupPanel m_inventoryItemPickupPanel;

	[Header("Special Items")]
	[SerializeField]
	private SpecialItemCell[] m_specialItemCells;

	[Header("Background UI Elements")]
	[FormerlySerializedAs("m_gridBackgroound")]
	[SerializeField]
	private RectTransform m_gridBackground;

	[SerializeField]
	private RectTransform m_expandingBackground;

	[Header("Upgrade")]
	[SerializeField]
	private InventoryAddNewSlotEffect m_unlockFlashEffectTemplate;

	[SerializeField]
	private RectTransform m_blockedCellsGridOverlay;

	[Header("Event Channels")]
	[SerializeField]
	private RemoveItemInstanceGameEventChannel m_removeItemEventChannel;

	[SerializeField]
	private ItemInstanceGameEventChannel m_requestEquipItemEventChannel;

	[SerializeField]
	private ItemInstanceGameEventChannel m_useItemEventChannel;

	[SerializeField]
	private ItemInstanceGameEventChannel m_applyItemToInteractableEventChannel;

	[SerializeField]
	private ItemInstanceGameEventChannel m_onItemRemovedEventChannel;

	[SerializeField]
	private ItemInstanceGameEventChannel m_onItemActivatedChangedEventChannel;

	[SerializeField]
	private TransferItemInstanceGameEventChannel m_requestTransferItemEventChannel;

	[SerializeField]
	private ItemInstanceGameEventChannel m_onKeyRingEnabledEventChannel;

	[SerializeField]
	private ItemInstanceGameEventChannel m_onItemInstanceStackAmountChangedEventChannel;

	[Header("Other")]
	[SerializeField]
	private KeyRingItemDefinition m_keyRingItemDefinition;

	[Header("Audio Events")]
	[SerializeField]
	private AudioEvent m_audioMoveItemDone;

	[SerializeField]
	private AudioEvent m_audioStartMovingItem;

	[SerializeField]
	private AudioEvent m_audioEquipItem;

	[SerializeField]
	private AudioEvent m_audioUnequipItem;

	[SerializeField]
	private AudioEvent m_audioCombineItems;

	[SerializeField]
	private AudioEvent m_audioUnload;

	[SerializeField]
	private AudioEvent m_audioTranferItem;

	[SerializeField]
	private AudioEvent m_audioRotateItem;

	[SerializeField]
	private AudioEvent m_audioPlaceItemError;

	[Header("Localized Strings")]
	[SerializeField]
	private LocalizedString m_stringUnloadFailedNoSpace;

	private InventoryItemCell m_currentlySelectedCell;

	private InventoryItemCell m_currentlyHoveredCell;

	public UnityEvent<ItemInstance> m_onSelectedItemChangedEvent;

	public UnityAction<Mode> m_onModeChanged;

	private List<InventoryItemCell> m_itemCells;

	private Vector2 m_expandingBackgroundStartSize;

	private Vector2Int m_cursorCellPosition = new Vector2Int(-1, -1);

	private Vector2 m_calculatedGridOffset;

	private float m_lastClearTime;

	private float m_inputBlockedTimer;

	private bool m_invalidMouseClick;

	private float m_itemSelectTime;

	private static readonly float s_doubleTapTime = 0.25f;

	private Mode m_activeMode;

	public Inventory TrackedInventory => m_trackedInventory;

	public Vector2Int CursorCellPosition
	{
		get
		{
			return m_cursorCellPosition;
		}
		set
		{
			if (m_cursorCellPosition != value)
			{
				m_cursorCellPosition = value;
				SetHoveredItemCellFromCursorPosition();
			}
		}
	}

	public Mode ActiveMode
	{
		get
		{
			return m_activeMode;
		}
		set
		{
			if (m_activeMode == value)
			{
				return;
			}
			if (m_activeMode == Mode.Move)
			{
				m_moveMode.Deactivate();
				ClearItemBlockStatus();
				if (m_currentlySelectedCell != null)
				{
					m_currentlySelectedCell.gameObject.SetActive(value: true);
				}
			}
			if (m_activeMode == Mode.Combine)
			{
				m_combineMode.Deactivate();
			}
			switch (value)
			{
			case Mode.KeyRingView:
				SetInteractionsEnabled(enabled: false);
				if (m_fullInventoryCanvasGroup != null)
				{
					m_fullInventoryCanvasGroup.alpha = 0.3f;
					CanvasGroup fullInventoryCanvasGroup2 = m_fullInventoryCanvasGroup;
					bool blocksRaycasts = (m_fullInventoryCanvasGroup.interactable = false);
					fullInventoryCanvasGroup2.blocksRaycasts = blocksRaycasts;
				}
				break;
			case Mode.Move:
				if (m_fullInventoryCanvasGroup != null)
				{
					m_fullInventoryCanvasGroup.alpha = 1f;
					CanvasGroup fullInventoryCanvasGroup3 = m_fullInventoryCanvasGroup;
					bool blocksRaycasts = (m_fullInventoryCanvasGroup.interactable = false);
					fullInventoryCanvasGroup3.blocksRaycasts = blocksRaycasts;
				}
				StartMoveMode(m_currentlySelectedCell);
				break;
			default:
				if (m_fullInventoryCanvasGroup != null)
				{
					m_fullInventoryCanvasGroup.alpha = 1f;
					CanvasGroup fullInventoryCanvasGroup = m_fullInventoryCanvasGroup;
					bool blocksRaycasts = (m_fullInventoryCanvasGroup.interactable = true);
					fullInventoryCanvasGroup.blocksRaycasts = blocksRaycasts;
				}
				break;
			}
			switch (value)
			{
			case Mode.Normal:
				m_onSelectedItemChangedEvent.Invoke(null);
				break;
			case Mode.Combine:
				m_combineMode.SetActive(m_currentlySelectedCell.ItemInstance, this);
				break;
			}
			if (value != Mode.ViewOnly && value != Mode.Move)
			{
				ResetEventSystemSelection();
			}
			m_activeMode = value;
			m_onModeChanged?.Invoke(value);
		}
	}

	public void TempBlockInput()
	{
		m_inputBlockedTimer = 0.1f;
		m_invalidMouseClick = false;
	}

	private void SetHoveredItemCellFromCursorPosition()
	{
		InventoryItemCell itemCellAtPosition = GetItemCellAtPosition(m_cursorCellPosition);
		m_cursorSelectionHighlight.gameObject.SetActive(itemCellAtPosition == null);
		if (!(itemCellAtPosition != null))
		{
			m_cursorSelectionHighlight.sizeDelta = m_inventoryItemCellPrefab.Size;
			m_cursorSelectionHighlight.anchoredPosition = GetAnchorPositionForItemPosition(m_cursorCellPosition);
		}
		HoverItemCell(itemCellAtPosition);
	}

	private void SetItemMoveGhostHighlight(ItemInstance itemInstance, Vector2Int cellPosition, bool rotated)
	{
		HoverItemCell(null);
		Vector2 sizeDelta = itemInstance.GetItemSize(rotated) * m_inventoryItemCellPrefab.Size;
		m_cursorSelectionHighlight.gameObject.SetActive(value: true);
		m_cursorSelectionHighlight.sizeDelta = sizeDelta;
		m_cursorSelectionHighlight.anchoredPosition = GetAnchorPositionForItemPosition(cellPosition);
	}

	private void HoverItemCell(InventoryItemCell itemCell)
	{
		if (itemCell != m_currentlyHoveredCell)
		{
			if (m_currentlyHoveredCell != null)
			{
				m_currentlyHoveredCell.SetHovered(hovered: false);
			}
			m_currentlyHoveredCell = itemCell;
			if (m_currentlyHoveredCell != null)
			{
				m_currentlyHoveredCell.SetHovered(hovered: true);
			}
		}
	}

	public void Awake()
	{
		if (m_cursorSelectionHighlight != null)
		{
			m_cursorSelectionHighlight.gameObject.SetActive(value: false);
		}
		if (m_expandingBackground != null)
		{
			m_expandingBackgroundStartSize = m_expandingBackground.sizeDelta;
		}
		SpecialItemCell[] specialItemCells = m_specialItemCells;
		for (int i = 0; i < specialItemCells.Length; i++)
		{
			specialItemCells[i].ItemCell.m_onItemSelectEvent.AddListener(OnItemCellClicked);
		}
	}

	private void OnEnable()
	{
		RefreshData();
		ActiveMode = Mode.Normal;
		m_onItemRemovedEventChannel?.Register(OnItemRemoved);
		GlobalReferences.Instance.EventChannels.Inventory.EquippedMeleeItemChanged.Register(OnItemEquippedChanged);
		GlobalReferences.Instance.EventChannels.Inventory.EquippedSecondaryItemChanged.Register(OnItemEquippedChanged);
		GlobalReferences.Instance.EventChannels.Inventory.EquippedRangedItemChanged.Register(OnItemEquippedChanged);
		m_requestTransferItemEventChannel?.Register(OnRequestItemTransfer);
		m_onKeyRingEnabledEventChannel?.Register(OnKeyRingView);
		m_onItemInstanceStackAmountChangedEventChannel?.Register(OnItemStackAmountChanged);
		GlobalReferences.Instance.EventChannels.Inventory.EquippedArtifactsChanged.Register(OnItemEquippedChanged);
		GlobalReferences.Instance.Anchors.Inventory.QueuedUseItemInstanceAnchor.Register(OnQueuedItemInstanceChanged);
		StartCoroutine(PostEnableRefreshSelection());
		GameInputManager.GameInputActions.UI.RightClick.performed += DeselectInputAction;
		GameInputManager.GameInputActions.UI.Cancel.performed += DeselectInputAction;
		GameInputManager.GameInputActions.UI.Submit.performed += OnSubmitInput;
		GameInputManager.GameInputActions.UI.MoveSelected.performed += OnMoveSelectedInput;
		GameInputManager.GameInputActions.UI.SecondaryMenuAction.performed += OnTransferSelectedInput;
		GameInputManager.GameInputActions.UI.RotateSelected.performed += OnRotateSelectedInput;
		if (m_isMainInventoryPanel)
		{
			GameInputManager.GameInputActions.UI.ToggleItemShortcut.performed += OnAddToItemWheelShortcutInput;
			if (ActiveMode == Mode.Normal)
			{
				PlayerMainInventory playerMainInventory = m_trackedInventory as PlayerMainInventory;
				if (playerMainInventory != null)
				{
					ItemInstance newInventoryUpgrade = playerMainInventory.GetNewInventoryUpgrade();
					if (newInventoryUpgrade != null)
					{
						HighlightNewInventorySlots(newInventoryUpgrade.ItemDefinition);
						playerMainInventory.ClearNewInventoryUpgrades();
					}
				}
			}
		}
		if (m_unlockFlashEffectTemplate != null)
		{
			m_unlockFlashEffectTemplate.gameObject.SetActive(value: false);
		}
		if (IsSelected())
		{
			m_currentlyHoveredCell = null;
			SetHoveredItemCellFromCursorPosition();
		}
	}

	private IEnumerator PostEnableRefreshSelection()
	{
		yield return new WaitForEndOfFrame();
		if (m_activeMode != Mode.ViewOnly)
		{
			ResetEventSystemSelection();
		}
	}

	private void OnDisable()
	{
		TryClearModeAndSelection();
		m_onItemRemovedEventChannel?.Unregister(OnItemRemoved);
		GlobalReferences.Instance.EventChannels.Inventory.EquippedMeleeItemChanged.Unregister(OnItemEquippedChanged);
		GlobalReferences.Instance.EventChannels.Inventory.EquippedSecondaryItemChanged.Unregister(OnItemEquippedChanged);
		GlobalReferences.Instance.EventChannels.Inventory.EquippedRangedItemChanged.Unregister(OnItemEquippedChanged);
		m_requestTransferItemEventChannel?.Unregister(OnRequestItemTransfer);
		m_onKeyRingEnabledEventChannel?.Unregister(OnKeyRingView);
		m_onItemInstanceStackAmountChangedEventChannel?.Unregister(OnItemStackAmountChanged);
		GlobalReferences.Instance.EventChannels.Inventory.EquippedArtifactsChanged.Unregister(OnItemEquippedChanged);
		GlobalReferences.Instance.Anchors.Inventory.QueuedUseItemInstanceAnchor.Unregister(OnQueuedItemInstanceChanged);
		if (GameInputManager.GameInputActions != null)
		{
			GameInputManager.GameInputActions.UI.RightClick.performed -= DeselectInputAction;
			GameInputManager.GameInputActions.UI.Cancel.performed -= DeselectInputAction;
			GameInputManager.GameInputActions.UI.Submit.performed -= OnSubmitInput;
			GameInputManager.GameInputActions.UI.MoveSelected.performed -= OnMoveSelectedInput;
			GameInputManager.GameInputActions.UI.SecondaryMenuAction.performed -= OnTransferSelectedInput;
			GameInputManager.GameInputActions.UI.RotateSelected.performed -= OnRotateSelectedInput;
			if (m_isMainInventoryPanel)
			{
				GameInputManager.GameInputActions.UI.ToggleItemShortcut.performed -= OnAddToItemWheelShortcutInput;
			}
		}
	}

	public void TryClearModeAndSelection()
	{
		if (m_activeMode != Mode.Move || m_moveMode.TryExitMoveMode())
		{
			if (m_currentlySelectedCell != null)
			{
				m_currentlySelectedCell.SetSelected(selected: false);
			}
			if (ActiveMode != 0)
			{
				m_lastClearTime = Time.unscaledTime;
			}
			if (ActiveMode != Mode.ViewOnly)
			{
				ActiveMode = Mode.Normal;
			}
			m_currentlySelectedCell = null;
		}
	}

	public void ClearSelectionInfo()
	{
		if (m_currentlySelectedCell != null)
		{
			m_currentlySelectedCell.SetSelected(selected: false);
		}
		m_currentlySelectedCell = null;
		if (m_currentlyHoveredCell != null)
		{
			m_currentlyHoveredCell.SetHovered(hovered: false);
		}
		m_currentlyHoveredCell = null;
		ClearItemBlockStatus();
		OnDeselect(null);
	}

	private void StartMoveMode(InventoryItemCell itemCell)
	{
		m_moveMode.SetActive(itemCell.ItemInstance, this);
		itemCell.gameObject.SetActive(value: false);
	}

	private void StartMoveMode(ItemInstance itemInstance)
	{
		m_moveMode.SetActive(itemInstance, this);
		foreach (InventoryItemCell itemCell in m_itemCells)
		{
			if (itemCell.ItemInstance == itemInstance)
			{
				itemCell.gameObject.SetActive(value: false);
			}
		}
	}

	public TryPlaceItemResult TransferItemInPosition(ItemInstance itemInstance, Vector2Int position, bool rotation)
	{
		UpdateItemCellBlockStatus(itemInstance, position, rotation);
		int num = 0;
		ItemInstance itemInstance2 = null;
		foreach (InventoryItemCell itemCell in m_itemCells)
		{
			if (itemCell.ItemInstance != itemInstance && itemCell.MoveBlocked)
			{
				itemInstance2 = itemCell.ItemInstance;
				num++;
			}
		}
		switch (num)
		{
		case 0:
			PlaceItem(itemInstance, position, rotation);
			return TryPlaceItemResult.OK;
		case 1:
			PlaceItem(itemInstance, position, rotation);
			StartMoveMode(itemInstance2);
			return TryPlaceItemResult.PickUpNewItem;
		default:
			return TryPlaceItemResult.Failed;
		}
	}

	public TryPlaceItemResult ValidatePlaceItemInPosition(ItemInstance itemInstance, Vector2Int position, bool rotation, out ItemInstance blockedItem)
	{
		UpdateItemCellBlockStatus(itemInstance, position, rotation);
		int num = 0;
		blockedItem = null;
		if (!m_trackedInventory.IsItemAtPositionWithinInventoryBounds(itemInstance.ItemDefinition, position, rotation))
		{
			return TryPlaceItemResult.Failed;
		}
		foreach (InventoryItemCell itemCell in m_itemCells)
		{
			if (itemCell.ItemInstance != itemInstance && itemCell.MoveBlocked)
			{
				blockedItem = itemCell.ItemInstance;
				num++;
			}
		}
		return num switch
		{
			0 => TryPlaceItemResult.OK, 
			1 => TryPlaceItemResult.PickUpNewItem, 
			_ => TryPlaceItemResult.Failed, 
		};
	}

	public TryPlaceItemResult TryPlaceItemInPositionAndExitMoveMode(ItemInstance itemInstance, Vector2Int position, bool rotation, out ItemInstance blockedItem)
	{
		TryPlaceItemResult tryPlaceItemResult = ValidatePlaceItemInPosition(itemInstance, position, rotation, out blockedItem);
		switch (tryPlaceItemResult)
		{
		case TryPlaceItemResult.OK:
			PlaceItem(itemInstance, position, rotation);
			break;
		case TryPlaceItemResult.PickUpNewItem:
			PlaceItem(itemInstance, position, rotation);
			StartMoveMode(blockedItem);
			break;
		}
		return tryPlaceItemResult;
	}

	public void RefreshAndMoveItem(ItemInstance itemInstance)
	{
		RefreshData();
		TempBlockInput();
		InventoryItemCell inventoryItemCell = null;
		foreach (InventoryItemCell itemCell in m_itemCells)
		{
			if (itemCell.ItemInstance == itemInstance)
			{
				inventoryItemCell = itemCell;
				break;
			}
		}
		if (inventoryItemCell != null)
		{
			m_currentlySelectedCell = inventoryItemCell;
			ActiveMode = Mode.Move;
		}
	}

	private void PlaceItem(ItemInstance itemInstance, Vector2Int position, bool rotation)
	{
		itemInstance.ItemGridPosition = position;
		itemInstance.ItemRotated = rotation;
		AudioEvent.Play2D((itemInstance.ItemDefinition.Audio.OnPlaced != null) ? itemInstance.ItemDefinition.Audio.OnPlaced : m_audioMoveItemDone);
		m_cursorCellPosition = position;
		HoverItemCell(m_currentlySelectedCell);
		RefreshData();
	}

	private bool IsSpecialItem(ItemInstance item)
	{
		if (item == null)
		{
			return false;
		}
		SpecialItemCell[] specialItemCells = m_specialItemCells;
		for (int i = 0; i < specialItemCells.Length; i++)
		{
			if (specialItemCells[i].ItemDefinition == item.ItemDefinition)
			{
				return true;
			}
		}
		return false;
	}

	public void RefreshData()
	{
		if (m_itemCells != null)
		{
			for (int num = m_itemCells.Count - 1; num >= 0; num--)
			{
				Object.Destroy(m_itemCells[num].gameObject);
			}
		}
		if (m_isMainInventoryPanel)
		{
			RectTransform component = GetComponent<RectTransform>();
			float num2 = (float)m_trackedInventory.InventorySize.x * (m_inventoryItemCellPrefab.Size.x + m_gridSpacing);
			m_calculatedGridOffset = new Vector2((component.rect.width - num2) * 0.5f, 0f);
		}
		else
		{
			m_calculatedGridOffset = Vector2.zero;
		}
		m_itemCells = new List<InventoryItemCell>();
		foreach (ItemInstance item in m_trackedInventory.ItemList)
		{
			if (!IsSpecialItem(item))
			{
				AddItemCell(item);
			}
		}
		if (m_expandingBackground != null)
		{
			int x = m_trackedInventory.InventorySize.x;
			int y = m_trackedInventory.InventorySize.y;
			Vector2 expandingBackgroundStartSize = m_expandingBackgroundStartSize;
			expandingBackgroundStartSize.x += (float)x * (m_inventoryItemCellPrefab.Size.x + m_gridSpacing);
			expandingBackgroundStartSize.y += (float)y * (m_inventoryItemCellPrefab.Size.y + m_gridSpacing);
			m_expandingBackground.sizeDelta = expandingBackgroundStartSize;
		}
		if (m_gridBackground != null)
		{
			Vector2Int inventorySize = m_trackedInventory.InventorySize;
			Vector2 vector = inventorySize;
			float num3 = m_inventoryItemCellPrefab.Size.x + m_gridSpacing;
			m_gridBackground.anchoredPosition = m_calculatedGridOffset;
			m_gridBackground.sizeDelta = vector * num3;
			Shape component2 = m_gridBackground.GetComponent<Shape>();
			component2.settings.gridSize = num3;
			component2.settings.fillOffset = new Vector2((inventorySize.x % 2 != 0) ? (num3 * 0.5f) : 0f, (inventorySize.y % 2 != 0) ? (num3 * 0.5f) : 0f);
			if (m_blockedCellsGridOverlay != null)
			{
				int availableLastRowSlots = m_trackedInventory.GetAvailableLastRowSlots();
				if (availableLastRowSlots == -1 || availableLastRowSlots == inventorySize.x)
				{
					m_blockedCellsGridOverlay.gameObject.SetActive(value: false);
				}
				else
				{
					m_blockedCellsGridOverlay.gameObject.SetActive(value: true);
					int num4 = inventorySize.x - availableLastRowSlots;
					m_blockedCellsGridOverlay.sizeDelta = new Vector2(num4, 1f) * num3;
				}
			}
		}
		if (m_activeMode != Mode.ViewOnly)
		{
			ResetEventSystemSelection();
		}
		if (m_inputState.InputMode == InputState.Mode.Gamepad && (CursorCellPosition.x == -1 || CursorCellPosition.y == -1))
		{
			CursorCellPosition = Vector2Int.zero;
		}
		SpecialItemCell[] specialItemCells = m_specialItemCells;
		foreach (SpecialItemCell specialItemCell in specialItemCells)
		{
			ItemInstance itemOfType = m_trackedInventory.GetItemOfType(specialItemCell.ItemDefinition);
			specialItemCell.SetItem(itemOfType);
		}
		SetHoveredItemCellFromCursorPosition();
	}

	private InventoryItemCell AddItemCell(ItemInstance item)
	{
		InventoryItemCell inventoryItemCell = Object.Instantiate(m_inventoryItemCellPrefab, base.transform);
		m_itemCells.Add(inventoryItemCell);
		inventoryItemCell.SetItem(item);
		UpdateItemCellEquipState(inventoryItemCell);
		PlayerMainInventory playerMainInventory = m_trackedInventory as PlayerMainInventory;
		if ((bool)playerMainInventory)
		{
			inventoryItemCell.IsShortcut = playerMainInventory.HasShortcutSet(item.ItemDefinition);
		}
		else
		{
			inventoryItemCell.IsShortcut = false;
		}
		inventoryItemCell.m_onItemSelectEvent.AddListener(OnItemCellClicked);
		inventoryItemCell.m_onDragEvent.AddListener(OnDragItemCell);
		PositionItemCell(inventoryItemCell, item.ItemGridPosition, item.ItemRotated);
		inventoryItemCell.SetItemBeingUsed(item == GlobalReferences.Instance.Anchors.Inventory.QueuedUseItemInstanceAnchor.Item);
		return inventoryItemCell;
	}

	private Vector2 GetAnchorPositionForItemPosition(Vector2Int itemPosition)
	{
		Vector2 vector = new Vector2((float)itemPosition.x * (m_inventoryItemCellPrefab.Size.x + m_gridSpacing), (float)itemPosition.y * (0f - (m_inventoryItemCellPrefab.Size.y + m_gridSpacing)));
		vector.x += m_margin;
		vector.y -= m_margin;
		return vector + m_calculatedGridOffset;
	}

	public void PositionItemCell(InventoryItemCell cell, Vector2Int position, bool rotated)
	{
		cell.GetComponent<RectTransform>().anchoredPosition = GetAnchorPositionForItemPosition(position);
		cell.SetCellPosition(position, rotated);
	}

	public Vector2 GetScreenPositionForCellPosition(Vector2Int position)
	{
		Vector2 anchorPositionForItemPosition = GetAnchorPositionForItemPosition(position);
		return GetComponent<RectTransform>().TransformPoint(anchorPositionForItemPosition);
	}

	public void ShowValidCombineTargetsForItem(ItemInstance item)
	{
		foreach (InventoryItemCell itemCell in m_itemCells)
		{
			if (itemCell.ItemInstance == item)
			{
				itemCell.ShowCombineOptionGlow(InventoryItemCell.CombineHighlightMode.ActiveCombiner);
				continue;
			}
			bool flag = m_trackedInventory.CanItemBeCombined(item, itemCell.ItemInstance);
			itemCell.ShowCombineOptionGlow((!flag) ? InventoryItemCell.CombineHighlightMode.DimInvalid : InventoryItemCell.CombineHighlightMode.Highlight);
		}
		SpecialItemCell[] specialItemCells = m_specialItemCells;
		foreach (SpecialItemCell specialItemCell in specialItemCells)
		{
			if (specialItemCell.ItemCell.ItemInstance == item)
			{
				specialItemCell.ItemCell.ShowCombineOptionGlow(InventoryItemCell.CombineHighlightMode.ActiveCombiner);
				continue;
			}
			bool flag2 = m_trackedInventory.CanItemBeCombined(item, specialItemCell.ItemCell.ItemInstance);
			specialItemCell.ItemCell.ShowCombineOptionGlow((!flag2) ? InventoryItemCell.CombineHighlightMode.DimInvalid : InventoryItemCell.CombineHighlightMode.Highlight);
		}
	}

	public void ClearCombineTargets()
	{
		foreach (InventoryItemCell itemCell in m_itemCells)
		{
			itemCell.ShowCombineOptionGlow(InventoryItemCell.CombineHighlightMode.None);
		}
		SpecialItemCell[] specialItemCells = m_specialItemCells;
		for (int i = 0; i < specialItemCells.Length; i++)
		{
			specialItemCells[i].ItemCell.ShowCombineOptionGlow(InventoryItemCell.CombineHighlightMode.None);
		}
	}

	public void OnItemCellClicked(InventoryItemCell itemCell)
	{
		if (m_inputState.InputMode == InputState.Mode.KeyboardMouse)
		{
			TrySelectItem(itemCell);
		}
	}

	private void DoubleTapOnItemCell(InventoryItemCell itemCell)
	{
		if ((bool)GlobalReferences.Instance.Anchors.Inventory.ItemBoxInteractableAnchor.Item)
		{
			TransferSelectedItem();
			return;
		}
		ItemInstance itemInstance = itemCell.ItemInstance;
		if (itemInstance.CanEquip(m_trackedInventory))
		{
			EquipSelectedItem();
		}
		else if (itemInstance.CanUse(m_trackedInventory, checkConditions: true))
		{
			UseSelectedItem(useItemFSMState: true);
		}
	}

	public void TrySelectItem(InventoryItemCell trySelectItemCell)
	{
		if (ActiveMode == Mode.ViewOnly || m_inputBlockedTimer > 0f || m_moveMode.IsMoveModeActive())
		{
			return;
		}
		if (m_combineMode.IsCombineModeActive())
		{
			if (trySelectItemCell != null)
			{
				m_combineMode.TryCombine(trySelectItemCell.ItemInstance, m_trackedInventory);
			}
		}
		else
		{
			if (ActiveMode == Mode.ViewOnly || ActiveMode == Mode.KeyRingView)
			{
				return;
			}
			if (ActiveMode == Mode.Menu && trySelectItemCell == m_currentlySelectedCell && Time.unscaledTime - m_itemSelectTime < s_doubleTapTime)
			{
				DoubleTapOnItemCell(m_currentlySelectedCell);
				m_itemSelectTime = 0f;
				return;
			}
			SelectItemCell(trySelectItemCell);
			if (trySelectItemCell != null)
			{
				m_requestTransferItemEventChannel?.Raise(new TransferItemInstanceEventData
				{
					m_fromInventory = m_trackedInventory,
					m_position = trySelectItemCell.CellPosition
				});
				if (trySelectItemCell.ItemInstance != null)
				{
					ActiveMode = Mode.Menu;
				}
				else
				{
					ActiveMode = Mode.Normal;
				}
			}
		}
	}

	private void SelectItemCell(InventoryItemCell itemCell)
	{
		if (m_currentlySelectedCell != itemCell)
		{
			if (m_currentlySelectedCell != null)
			{
				m_currentlySelectedCell.SetSelected(selected: false);
			}
			if (itemCell != null && itemCell.ItemInstance != null)
			{
				itemCell.SetSelected(selected: true);
				AlignActionBoxToItemCell(itemCell);
			}
			m_currentlySelectedCell = itemCell;
			m_onSelectedItemChangedEvent.Invoke((itemCell != null) ? itemCell.ItemInstance : null);
			m_itemSelectTime = Time.unscaledTime;
		}
	}

	private void AlignActionBoxToItemCell(InventoryItemCell itemCell)
	{
		RectTransform component = itemCell.GetComponent<RectTransform>();
		Vector2 vector = component.position;
		if (!m_isMainInventoryPanel)
		{
			vector.x += component.rect.width;
			vector.x -= 2f;
		}
		else
		{
			vector.x += 2f;
		}
		m_actionsContainer.position = vector;
	}

	public void SetMoveMode()
	{
		ActiveMode = Mode.Move;
	}

	public void SetCombineMode()
	{
		ActiveMode = Mode.Combine;
	}

	public void DiscardSelectedItem()
	{
		if (m_currentlySelectedCell != null && m_currentlySelectedCell.ItemInstance != null)
		{
			RemoveItemInstanceEventData value = new RemoveItemInstanceEventData
			{
				m_itemInstance = m_currentlySelectedCell.ItemInstance,
				m_itemAmount = 1
			};
			m_removeItemEventChannel.Raise(value);
			TryClearModeAndSelection();
			RefreshData();
		}
	}

	public void EquipSelectedItem()
	{
		if (m_currentlySelectedCell != null && m_currentlySelectedCell.ItemInstance != null)
		{
			m_requestEquipItemEventChannel.Raise(m_currentlySelectedCell.ItemInstance);
		}
		TryClearModeAndSelection();
	}

	public void ToggleActivatedSelectedItem()
	{
		if (m_currentlySelectedCell != null && m_currentlySelectedCell.ItemInstance != null)
		{
			if (!m_currentlySelectedCell.ItemInstance.CanBeActivated())
			{
				Debug.LogError("Trying to activate an item that cannot be activated! " + m_currentlySelectedCell.ItemInstance.ItemDefinition.ItemName);
				return;
			}
			m_currentlySelectedCell.ItemInstance.Activated = !m_currentlySelectedCell.ItemInstance.Activated;
			UpdateItemCellEquipState(m_currentlySelectedCell);
		}
		TryClearModeAndSelection();
	}

	public void UnloadSelectedItem()
	{
		if (m_audioUnload != null)
		{
			AudioEvent.Play2D(m_audioUnload);
		}
		if (m_currentlySelectedCell.ItemInstance is ProjectileWeaponItemInstance item && !m_trackedInventory.RequestUnloadItem(item))
		{
			GlobalReferences.Instance.EventChannels.InGameMenu.MenuInfoMessage.Raise(new MenuInfoMessageData
			{
				m_stringReference = m_stringUnloadFailedNoSpace
			});
		}
		RefreshData();
		ActiveMode = Mode.Normal;
	}

	public void DropSelectedItem()
	{
		GlobalReferences.Instance.EventChannels.Inventory.RequestDropItemInstance.Raise(m_currentlySelectedCell.ItemInstance);
		RefreshData();
		ActiveMode = Mode.Normal;
		GlobalReferences.Instance.GameMenuState.ClearInMenu(GameMenuState.GameMenu.InGameMenu);
	}

	public void ExamineSelectedItem()
	{
		GlobalReferences.Instance.EventChannels.Inventory.ExamineItemInstance.Raise(m_currentlySelectedCell.ItemInstance);
		TryClearModeAndSelection();
	}

	public void UseSelectedItem(bool useItemFSMState)
	{
		if (useItemFSMState && m_currentlySelectedCell.ItemInstance.ItemDefinition.UsingItemHasState)
		{
			GlobalReferences.Instance.Anchors.Inventory.QueuedUseItemInstanceAnchor.Set(m_currentlySelectedCell.ItemInstance);
		}
		else
		{
			m_useItemEventChannel.Raise(m_currentlySelectedCell.ItemInstance);
			RefreshData();
		}
		ActiveMode = Mode.Normal;
	}

	public void ToggleItemOnShortcutItemWheel()
	{
		PlayerMainInventory playerMainInventory = m_trackedInventory as PlayerMainInventory;
		if (playerMainInventory != null)
		{
			playerMainInventory.ToggleItemOnShortcutItemWheel(m_currentlySelectedCell.ItemInstance.ItemDefinition);
			RefreshData();
		}
		ActiveMode = Mode.Normal;
	}

	public void PowerUpSelectedArtifact()
	{
		if (m_currentlySelectedCell.ItemInstance is ArtifactItemInstance artifactItemInstance)
		{
			artifactItemInstance.PowerUp();
			GlobalReferences.Instance.EventChannels.Inventory.OnPowerUpArtifactItem.Raise(m_currentlySelectedCell.ItemInstance);
		}
		ActiveMode = Mode.Normal;
	}

	public void ApplySelectedItem()
	{
		bool flag = false;
		if (m_activeApplyItemInteractable != null && m_activeApplyItemInteractable.Item != null)
		{
			IApplyItem item = m_activeApplyItemInteractable.Item;
			if (item != null && item.CanApplyItem(m_currentlySelectedCell.ItemInstance))
			{
				flag = true;
			}
		}
		if (flag)
		{
			m_currentlySelectedCell.AnimateApplyItemCorrectItem();
			m_applyItemToInteractableEventChannel.Raise(m_currentlySelectedCell.ItemInstance);
			TryClearModeAndSelection();
			ActiveMode = Mode.ViewOnly;
		}
		else
		{
			m_currentlySelectedCell.AnimateApplyItemWrongItem();
		}
		GlobalReferences.Instance.EventChannels.Generic.TryApplyItemSuccessFail.Raise(flag);
	}

	public void DoSpecialItemAction()
	{
		m_currentlySelectedCell.ItemInstance.DoSpecialAction();
	}

	private void OnItemRemoved(ItemInstance item)
	{
		if (m_currentlySelectedCell != null && m_currentlySelectedCell.ItemInstance == item)
		{
			TryClearModeAndSelection();
		}
		RefreshData();
	}

	private void OnItemEquippedChanged(ItemEquippedEventData equipEvent)
	{
		if (equipEvent.m_equipped)
		{
			AudioEvent audioEvent = ((equipEvent.m_itemInstance.ItemDefinition.Audio.OnEquip != null) ? equipEvent.m_itemInstance.ItemDefinition.Audio.OnEquip : m_audioEquipItem);
			if (audioEvent != null)
			{
				AudioEvent.Play2D(audioEvent);
			}
		}
		else
		{
			AudioEvent audioEvent2 = ((equipEvent.m_itemInstance.ItemDefinition.Audio.OnUnEquip != null) ? equipEvent.m_itemInstance.ItemDefinition.Audio.OnUnEquip : m_audioUnequipItem);
			if (audioEvent2 != null)
			{
				AudioEvent.Play2D(audioEvent2);
			}
		}
		foreach (InventoryItemCell itemCell in m_itemCells)
		{
			UpdateItemCellEquipState(itemCell);
		}
	}

	private void UpdateItemCellEquipState(InventoryItemCell itemCell)
	{
		InventoryItemCell.EquippedState itemEquippedState = InventoryItemCell.EquippedState.None;
		PlayerMainInventory playerMainInventory = m_trackedInventory as PlayerMainInventory;
		if (itemCell.ItemInstance.Activated)
		{
			itemEquippedState = InventoryItemCell.EquippedState.Activated;
		}
		if (playerMainInventory != null)
		{
			if (itemCell.ItemInstance == playerMainInventory.EquippedRangedItem)
			{
				itemEquippedState = InventoryItemCell.EquippedState.Ranged;
			}
			else if (itemCell.ItemInstance == playerMainInventory.EquippedMeleeItem)
			{
				itemEquippedState = InventoryItemCell.EquippedState.Melee;
			}
			else if (itemCell.ItemInstance == playerMainInventory.EquippedSecondaryItem)
			{
				itemEquippedState = InventoryItemCell.EquippedState.Secondary;
			}
			else if (itemCell.ItemInstance is ArtifactItemInstance && playerMainInventory.IsArtifactEquipped(itemCell.ItemInstance))
			{
				itemEquippedState = InventoryItemCell.EquippedState.Artifact;
			}
		}
		itemCell.ItemEquippedState = itemEquippedState;
	}

	public void SetInteractionsEnabled(bool enabled)
	{
		if (enabled || m_inputState.InputMode != InputState.Mode.KeyboardMouse)
		{
			m_panelCanvasGroup.interactable = enabled;
			if (enabled)
			{
				ResetEventSystemSelection();
			}
		}
	}

	private InventoryItemCell GetItemCellAtPosition(Vector2Int position)
	{
		foreach (InventoryItemCell itemCell in m_itemCells)
		{
			if (itemCell.CheckForOverlapWithPosition(position))
			{
				return itemCell;
			}
		}
		SpecialItemCell[] specialItemCells = m_specialItemCells;
		foreach (SpecialItemCell specialItemCell in specialItemCells)
		{
			if (specialItemCell.ItemCell.ItemInstance != null && specialItemCell.ItemCell.CheckForOverlapWithPosition(position))
			{
				return specialItemCell.ItemCell;
			}
		}
		return null;
	}

	private void Update()
	{
		if (m_inventoryItemPickupPanel != null && m_inventoryItemPickupPanel.IsShowing)
		{
			return;
		}
		if ((m_activeMode == Mode.Normal || m_activeMode == Mode.Move) && EventSystem.current.currentSelectedGameObject == null)
		{
			ResetEventSystemSelection();
		}
		if (m_inputState.InputMode == InputState.Mode.KeyboardMouse)
		{
			if (m_activeMode != Mode.Move)
			{
				UpdateInventoryMouseCursor();
			}
			if (m_inputBlockedTimer > 0f)
			{
				if (Mouse.current.leftButton.isPressed)
				{
					m_invalidMouseClick = true;
				}
				m_inputBlockedTimer -= Time.unscaledDeltaTime;
				if (!Mouse.current.leftButton.isPressed)
				{
					m_inputBlockedTimer = 0f;
				}
			}
			else if (m_invalidMouseClick && !Mouse.current.leftButton.isPressed)
			{
				m_invalidMouseClick = false;
			}
			else if (m_activeMode == Mode.Move)
			{
				if (Mouse.current.leftButton.wasPressedThisFrame && IsMouseHoveringInventoryPanel())
				{
					m_moveMode.TryPlaceItem();
				}
			}
			else
			{
				if (!Mouse.current.leftButton.wasReleasedThisFrame)
				{
					return;
				}
				if (m_invalidMouseClick)
				{
					m_invalidMouseClick = false;
				}
				else if (IsMouseHoveringInventoryPanel())
				{
					bool flag = false;
					if (Keyboard.current != null && m_currentlyHoveredCell != null && m_currentlyHoveredCell.ItemInstance != null && GlobalReferences.Instance.Anchors.Inventory.ItemBoxInteractableAnchor.Item != null && (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.leftCtrlKey.isPressed))
					{
						flag = true;
					}
					if (flag)
					{
						TransferItem(m_currentlyHoveredCell.ItemInstance);
					}
					else
					{
						TrySelectItem(m_currentlyHoveredCell);
					}
				}
			}
		}
		else if (m_inputState.InputMode == InputState.Mode.Gamepad && m_inputBlockedTimer > 0f)
		{
			m_inputBlockedTimer -= Time.unscaledDeltaTime;
		}
	}

	private bool IsMouseHoveringInventoryPanel()
	{
		Vector2 position = Mouse.current.position.ReadValue();
		PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
		pointerEventData.position = position;
		List<RaycastResult> list = new List<RaycastResult>();
		m_graphicRaycaster.Raycast(pointerEventData, list);
		bool result = true;
		foreach (RaycastResult item in list)
		{
			if ((bool)item.gameObject.GetComponentInParent<InventoryActionButton>())
			{
				return false;
			}
		}
		return result;
	}

	private void UpdateInventoryMouseCursor()
	{
		Vector2 vector = Mouse.current.position.ReadValue();
		bool flag = IsMouseHoveringInventoryPanel();
		if (flag)
		{
			SpecialItemCell specialItemCell = null;
			SpecialItemCell[] specialItemCells = m_specialItemCells;
			foreach (SpecialItemCell specialItemCell2 in specialItemCells)
			{
				if (specialItemCell2.IsHovered(vector))
				{
					specialItemCell = specialItemCell2;
					break;
				}
			}
			if ((bool)specialItemCell)
			{
				m_cursorCellPosition = new Vector2Int(-1, -1);
				m_cursorSelectionHighlight.gameObject.SetActive(value: false);
				flag = true;
				HoverItemCell(specialItemCell.ItemCell);
			}
			else
			{
				RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RectTransform>(), vector, null, out var localPoint);
				localPoint.y *= -1f;
				localPoint.x -= m_margin;
				localPoint.y -= m_margin;
				localPoint.x -= m_calculatedGridOffset.x;
				localPoint.y += m_calculatedGridOffset.y;
				float num = m_inventoryItemCellPrefab.Size.x + m_gridSpacing;
				float num2 = m_inventoryItemCellPrefab.Size.y + m_gridSpacing;
				localPoint.x /= num;
				localPoint.y /= num2;
				Vector2Int vector2Int = new Vector2Int(Mathf.FloorToInt(localPoint.x), Mathf.FloorToInt(localPoint.y));
				Vector2Int one = Vector2Int.one;
				if (IsPositionInBounds(vector2Int, one))
				{
					CursorCellPosition = vector2Int;
				}
				else
				{
					flag = false;
				}
			}
		}
		if (!flag)
		{
			m_cursorCellPosition = -Vector2Int.one;
			m_cursorSelectionHighlight.gameObject.SetActive(value: false);
			HoverItemCell(null);
		}
	}

	public Vector2Int ClampCellPositionForItem(Vector2Int position, ItemInstance itemInstance, bool rotated)
	{
		Vector2Int itemSize = itemInstance.GetItemSize(rotated);
		Vector2Int zero = Vector2Int.zero;
		Vector2Int vector2Int = new Vector2Int(m_trackedInventory.InventorySize.x - itemSize.x, m_trackedInventory.InventorySize.y - itemSize.y);
		position.x = Mathf.Clamp(position.x, zero.x, vector2Int.x);
		position.y = Mathf.Clamp(position.y, zero.y, vector2Int.y);
		return position;
	}

	public bool UpdateCursorHoverFromMoveMode(Vector2 cursorPosition, Vector2 itemPivotPosition, ItemInstance itemInstance, bool itemRotated, out Vector2Int cellItemPosition)
	{
		if (RectTransformUtility.RectangleContainsScreenPoint(m_gridBackground, cursorPosition))
		{
			RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RectTransform>(), itemPivotPosition, null, out var localPoint);
			localPoint.x += m_inventoryItemCellPrefab.Size.x * 0.5f;
			localPoint.y -= m_inventoryItemCellPrefab.Size.y * 0.5f;
			localPoint.y *= -1f;
			localPoint.x -= m_margin;
			localPoint.y -= m_margin;
			localPoint.x -= m_calculatedGridOffset.x;
			localPoint.y += m_calculatedGridOffset.y;
			float num = m_inventoryItemCellPrefab.Size.x + m_gridSpacing;
			float num2 = m_inventoryItemCellPrefab.Size.y + m_gridSpacing;
			localPoint.x /= num;
			localPoint.y /= num2;
			cellItemPosition = new Vector2Int(Mathf.FloorToInt(localPoint.x), Mathf.FloorToInt(localPoint.y));
			Vector2Int one = Vector2Int.one;
			ClampCellPositionForItem(cellItemPosition, itemInstance, itemRotated);
			if (IsPositionInBounds(cellItemPosition, one) && m_trackedInventory.IsItemAtPositionWithinInventoryBounds(itemInstance.ItemDefinition, cellItemPosition, itemRotated))
			{
				SetItemMoveGhostHighlight(itemInstance, cellItemPosition, itemRotated);
				UpdateItemCellBlockStatus(itemInstance, cellItemPosition, itemRotated);
				return true;
			}
			m_cursorSelectionHighlight.gameObject.SetActive(value: false);
			ClearItemBlockStatus();
			return false;
		}
		cellItemPosition = default(Vector2Int);
		m_cursorSelectionHighlight.gameObject.SetActive(value: false);
		ClearItemBlockStatus();
		return false;
	}

	private bool IsPositionInBounds(Vector2Int position, Vector2Int cursorSize)
	{
		SpecialItemCell[] specialItemCells = m_specialItemCells;
		foreach (SpecialItemCell specialItemCell in specialItemCells)
		{
			if (specialItemCell.isActiveAndEnabled && specialItemCell.ItemCell.GetCellOccupiedPositions().Contains(position))
			{
				return true;
			}
		}
		if (position.x < 0 || position.y < 0 || position.x + cursorSize.x > m_trackedInventory.InventorySize.x || position.y + cursorSize.y > m_trackedInventory.InventorySize.y)
		{
			return false;
		}
		if (m_trackedInventory.IsSlotLocked(position))
		{
			return false;
		}
		return true;
	}

	private void ResetEventSystemSelection()
	{
		if (!(EventSystem.current == null))
		{
			EventSystem.current.SetSelectedGameObject(base.gameObject);
		}
	}

	public bool ShouldExitOnCancel()
	{
		if (ActiveMode != 0)
		{
			return ActiveMode == Mode.ViewOnly;
		}
		return true;
	}

	public void OtherInventoryPanelSelected(ItemInstance instance)
	{
		if (instance != null && !m_moveMode.IsMoveModeActive())
		{
			TryClearModeAndSelection();
		}
	}

	private void OnRequestItemTransfer(TransferItemInstanceEventData eventData)
	{
		if (!(eventData.m_fromInventory == m_trackedInventory) && !(m_currentlySelectedCell == null) && m_currentlySelectedCell.ItemInstance != null && m_activeMode == Mode.Move)
		{
			TransferItem(eventData.m_specificItemToMove);
		}
	}

	private void OnKeyRingView(ItemInstance itemInstance)
	{
		if (itemInstance != null)
		{
			ActiveMode = Mode.KeyRingView;
		}
		else
		{
			ActiveMode = Mode.Normal;
		}
	}

	private void DeselectInputAction(InputAction.CallbackContext obj)
	{
		if (ActiveMode != Mode.KeyRingView)
		{
			TryClearModeAndSelection();
		}
	}

	public void TransferSelectedItem()
	{
		TransferItem(m_currentlySelectedCell.ItemInstance);
	}

	private void TransferItem(ItemInstance itemInstance)
	{
		if (m_trackedInventory.IsItemMovable(itemInstance))
		{
			if (m_otherInventory.CanAddItemToInventory(itemInstance.ItemDefinition, 1))
			{
				m_trackedInventory.TransferItem(itemInstance, m_otherInventory, new Vector2Int(-1, -1));
				RefreshData();
			}
			if (m_audioTranferItem != null)
			{
				m_audioTranferItem.Play2D();
			}
			TryClearModeAndSelection();
		}
	}

	private void OnItemStackAmountChanged(ItemInstance itemInstance)
	{
		foreach (InventoryItemCell itemCell in m_itemCells)
		{
			if (itemCell.ItemInstance == itemInstance)
			{
				itemCell.SetItem(itemInstance);
			}
		}
		SpecialItemCell[] specialItemCells = m_specialItemCells;
		foreach (SpecialItemCell specialItemCell in specialItemCells)
		{
			if (specialItemCell.ItemCell.ItemInstance == itemInstance)
			{
				specialItemCell.SetItem(itemInstance);
			}
		}
	}

	private IEnumerator ForceKeyRingView()
	{
		yield return new WaitForEndOfFrame();
		ItemInstance itemOfType = m_trackedInventory.GetItemOfType(m_keyRingItemDefinition);
		if (itemOfType != null)
		{
			ActiveMode = Mode.KeyRingView;
			itemOfType.DoSpecialAction();
		}
	}

	private void HighlightNewInventorySlots(ItemDefinition upgradeItem)
	{
		InventoryUpgradeItemDefinition inventoryUpgradeItemDefinition = upgradeItem as InventoryUpgradeItemDefinition;
		if (!(inventoryUpgradeItemDefinition != null))
		{
			return;
		}
		Vector2Int inventorySize = m_trackedInventory.InventorySize;
		int num = m_trackedInventory.GetAvailableLastRowSlots();
		if (num == -1)
		{
			num = inventorySize.x;
		}
		int num2 = inventorySize.x - num;
		float num3 = m_inventoryItemCellPrefab.Size.x + m_gridSpacing;
		for (int i = 0; i < inventoryUpgradeItemDefinition.AddedSlots; i++)
		{
			InventoryAddNewSlotEffect inventoryAddNewSlotEffect = Object.Instantiate(m_unlockFlashEffectTemplate, m_unlockFlashEffectTemplate.transform.parent);
			RectTransform component = inventoryAddNewSlotEffect.GetComponent<RectTransform>();
			int num4 = i + num2;
			int num5 = 0;
			while (num4 >= inventorySize.x)
			{
				num4 -= inventorySize.x;
				num5++;
			}
			component.anchoredPosition = new Vector2(num3 * (float)(-num4), num3 * (float)num5);
			inventoryAddNewSlotEffect.Animate();
		}
	}

	public virtual void OnMove(AxisEventData eventData)
	{
		if (m_activeMode == Mode.Menu || m_activeMode == Mode.ViewOnly || m_activeMode == Mode.KeyRingView || m_moveMode.IsMoveModeActive() || (m_inventoryItemPickupPanel != null && m_inventoryItemPickupPanel.IsShowing))
		{
			return;
		}
		bool flag = false;
		Vector2Int vector2Int = new Vector2Int(Mathf.RoundToInt(eventData.moveVector.x), Mathf.RoundToInt(eventData.moveVector.y));
		vector2Int.x = Mathf.Clamp(vector2Int.x, -1, 1);
		vector2Int.y = Mathf.Clamp(vector2Int.y, -1, 1);
		if (vector2Int.x != 0 || vector2Int.y != 0)
		{
			Vector2Int cursorCellPosition = CursorCellPosition;
			vector2Int.y *= -1;
			Vector2Int vector2Int2 = cursorCellPosition + vector2Int;
			Vector2Int cursorSize = Vector2Int.one;
			if (m_currentlyHoveredCell != null)
			{
				Vector2Int cellPosition = m_currentlyHoveredCell.CellPosition;
				Vector2Int rotatedItemSize = m_currentlyHoveredCell.RotatedItemSize;
				if (m_activeMode != Mode.Move)
				{
					if (vector2Int.x == -1)
					{
						vector2Int2.x = cellPosition.x - 1;
					}
					else if (vector2Int.x == 1)
					{
						vector2Int2.x = cellPosition.x + rotatedItemSize.x;
					}
					if (vector2Int.y == -1)
					{
						vector2Int2.y = cellPosition.y - 1;
					}
					else if (vector2Int.y == 1)
					{
						vector2Int2.y = cellPosition.y + rotatedItemSize.y;
					}
				}
				else
				{
					cursorSize = m_currentlySelectedCell.RotatedItemSize;
				}
			}
			if (!IsAboveSpecialInventorySlot(vector2Int2) && m_trackedInventory.IsSlotLocked(vector2Int2))
			{
				int availableLastRowSlots = m_trackedInventory.GetAvailableLastRowSlots();
				if (availableLastRowSlots != -1)
				{
					vector2Int2.x = availableLastRowSlots - 1;
				}
			}
			if (IsPositionInBounds(vector2Int2, cursorSize))
			{
				flag = true;
				CursorCellPosition = vector2Int2;
				_ = ActiveMode;
				_ = 1;
			}
		}
		if (!flag && m_otherInventoryPanel != null && m_otherInventoryPanel.isActiveAndEnabled && ((m_isMainInventoryPanel && eventData.moveDir == MoveDirection.Left) || (!m_isMainInventoryPanel && eventData.moveDir == MoveDirection.Right)))
		{
			HoverItemCell(null);
			eventData.selectedObject = m_otherInventoryPanel.gameObject;
			m_otherInventoryPanel.TransferCursor(CursorCellPosition);
		}
	}

	private void OnSubmitInput(InputAction.CallbackContext callback)
	{
		if (callback.performed && (!(m_inventoryItemPickupPanel != null) || !m_inventoryItemPickupPanel.IsShowing))
		{
			if (m_activeMode == Mode.Move)
			{
				m_moveMode.TryPlaceItem();
			}
			else if (m_activeMode != Mode.Menu && m_activeMode != Mode.ViewOnly && m_activeMode != Mode.KeyRingView && m_inputState.InputMode == InputState.Mode.Gamepad && IsSelected())
			{
				StartCoroutine(SelectItemDelayed());
			}
		}
	}

	private IEnumerator SelectItemDelayed()
	{
		yield return new WaitForEndOfFrame();
		TrySelectItem(m_currentlyHoveredCell);
	}

	private bool UpdateItemCellBlockStatus(ItemInstance itemInstance, Vector2Int targetPosition, bool targetRotation)
	{
		bool result = false;
		foreach (InventoryItemCell itemCell in m_itemCells)
		{
			if (!(itemCell == m_currentlySelectedCell))
			{
				if (itemCell.CheckForOverlap(itemInstance, targetPosition, targetRotation))
				{
					result = true;
					itemCell.MoveBlocked = true;
				}
				else
				{
					itemCell.MoveBlocked = false;
				}
			}
		}
		return result;
	}

	public void ClearItemBlockStatus()
	{
		if (m_itemCells == null)
		{
			return;
		}
		foreach (InventoryItemCell itemCell in m_itemCells)
		{
			itemCell.MoveBlocked = false;
		}
	}

	private void ValidateMoveExit()
	{
		if (ActiveMode == Mode.Move)
		{
			m_moveMode.TryExitMoveMode();
		}
	}

	private List<InventoryItemCell> GetMoveOverlappingItemCells()
	{
		List<InventoryItemCell> list = new List<InventoryItemCell>();
		foreach (InventoryItemCell itemCell in m_itemCells)
		{
			if (!(itemCell == m_currentlySelectedCell) && itemCell.MoveBlocked)
			{
				list.Add(itemCell);
			}
		}
		return list;
	}

	private InventoryItemCell GetDefaultSelectedItemCell()
	{
		int num = int.MaxValue;
		int num2 = int.MaxValue;
		InventoryItemCell result = null;
		foreach (InventoryItemCell itemCell in m_itemCells)
		{
			if (itemCell.CellPosition.y < num2 || (itemCell.CellPosition.x < num && itemCell.CellPosition.y == num2))
			{
				num = itemCell.CellPosition.x;
				num2 = itemCell.CellPosition.y;
				result = itemCell;
			}
		}
		return result;
	}

	private void OnDragItemCell(InventoryItemCell itemCell)
	{
		if (m_activeMode != Mode.Move)
		{
			SelectItemCell(itemCell);
			SetMoveMode();
		}
	}

	public bool CanExitInventoryPanelSafely()
	{
		if (m_activeMode == Mode.Move)
		{
			return false;
		}
		return true;
	}

	private bool IsSelected()
	{
		return EventSystem.current.currentSelectedGameObject == base.gameObject;
	}

	private void OnMoveSelectedInput(InputAction.CallbackContext callback)
	{
		if (!(m_inputBlockedTimer > 0f))
		{
			if (m_activeMode == Mode.Move)
			{
				m_moveMode.TryPlaceItem();
			}
			else if (m_currentlyHoveredCell != null && m_trackedInventory.IsItemMovable(m_currentlyHoveredCell.ItemInstance))
			{
				SelectItemCell(m_currentlyHoveredCell);
				SetMoveMode();
			}
		}
	}

	private void OnTransferSelectedInput(InputAction.CallbackContext callback)
	{
		if (!(GlobalReferences.Instance.Anchors.Inventory.ItemBoxInteractableAnchor.Item == null) && m_activeMode == Mode.Normal && !(Time.unscaledTime - m_lastClearTime < 0.1f) && m_currentlyHoveredCell != null)
		{
			TransferItem(m_currentlyHoveredCell.ItemInstance);
		}
	}

	public void OnSelect(BaseEventData eventData)
	{
		SetHoveredItemCellFromCursorPosition();
	}

	public void OnDeselect(BaseEventData eventData)
	{
		if (m_cursorSelectionHighlight != null)
		{
			m_cursorSelectionHighlight.gameObject.SetActive(value: false);
		}
		HoverItemCell(null);
	}

	public void TransferCursor(Vector2Int position)
	{
		Vector2Int cursorCellPosition = new Vector2Int((!m_isMainInventoryPanel) ? (m_trackedInventory.InventorySize.x - 1) : 0, 0);
		cursorCellPosition.y = Mathf.Clamp(position.y, 0, m_trackedInventory.InventorySize.y - 1);
		CursorCellPosition = cursorCellPosition;
	}

	private bool CanRotateItem(InventoryItemCell itemCell)
	{
		if (itemCell.CanBeRotated)
		{
			return m_trackedInventory.IsItemMovable(itemCell.ItemInstance);
		}
		return false;
	}

	private void OnRotateSelectedInput(InputAction.CallbackContext callback)
	{
		if (m_activeMode == Mode.Move)
		{
			m_moveMode.RotateInput();
		}
	}

	public bool CanDoRotateInput()
	{
		if (m_activeMode == Mode.Move && m_currentlySelectedCell != null && CanRotateItem(m_currentlySelectedCell))
		{
			return true;
		}
		return false;
	}

	public bool CanTransferItem()
	{
		if (m_activeMode == Mode.Normal)
		{
			if (m_currentlyHoveredCell != null)
			{
				return m_trackedInventory.IsItemMovable(m_currentlyHoveredCell.ItemInstance);
			}
			return false;
		}
		return false;
	}

	private void OnQueuedItemInstanceChanged(ItemInstance itemInstance)
	{
		foreach (InventoryItemCell itemCell in m_itemCells)
		{
			itemCell.SetItemBeingUsed(itemInstance == itemCell.ItemInstance);
		}
	}

	public bool CanDoShortcutInput()
	{
		return false;
	}

	private void OnAddToItemWheelShortcutInput(InputAction.CallbackContext callback)
	{
		if (CanDoShortcutInput() && m_currentlySelectedCell == null && m_currentlyHoveredCell != null && m_currentlyHoveredCell.ItemInstance.ItemDefinition.CanHaveShortcutSet(m_trackedInventory))
		{
			PlayerMainInventory playerMainInventory = m_trackedInventory as PlayerMainInventory;
			if (playerMainInventory != null)
			{
				playerMainInventory.ToggleItemOnShortcutItemWheel(m_currentlyHoveredCell.ItemInstance.ItemDefinition);
			}
			RefreshData();
			ActiveMode = Mode.Normal;
		}
	}

	public ItemInstance GetSelectedItemInstance()
	{
		if (m_currentlySelectedCell != null && m_currentlySelectedCell.ItemInstance != null)
		{
			return m_currentlySelectedCell.ItemInstance;
		}
		return null;
	}

	private bool IsAboveSpecialInventorySlot(Vector2Int position)
	{
		if (m_specialItemCells != null)
		{
			SpecialItemCell[] specialItemCells = m_specialItemCells;
			foreach (SpecialItemCell specialItemCell in specialItemCells)
			{
				if (specialItemCell.isActiveAndEnabled && specialItemCell.ItemCell.ItemInstance != null && specialItemCell.ItemCell.CheckForOverlapWithPosition(position))
				{
					return true;
				}
			}
		}
		return false;
	}

	public ItemInstance GetHoveredItemInstance()
	{
		if (m_itemCells != null)
		{
			foreach (InventoryItemCell itemCell in m_itemCells)
			{
				if (itemCell.IsHovered && itemCell.ItemInstance != null)
				{
					return itemCell.ItemInstance;
				}
			}
		}
		if (m_specialItemCells != null)
		{
			SpecialItemCell[] specialItemCells = m_specialItemCells;
			foreach (SpecialItemCell specialItemCell in specialItemCells)
			{
				if (specialItemCell.isActiveAndEnabled && specialItemCell.ItemCell.IsHovered && specialItemCell.ItemCell.ItemInstance != null)
				{
					return specialItemCell.ItemCell.ItemInstance;
				}
			}
		}
		return null;
	}

	public void UnequipItemIntoMoveMode(ItemInstance itemInstance)
	{
		bool flag = false;
		foreach (InventoryItemCell itemCell in m_itemCells)
		{
			if (itemCell.ItemInstance == itemInstance)
			{
				SelectItemCell(itemCell);
				flag = true;
				break;
			}
		}
		if (flag)
		{
			GlobalReferences.Instance.EventChannels.Inventory.RequestEquipItem.Raise(itemInstance);
			SetMoveMode();
		}
	}
}
