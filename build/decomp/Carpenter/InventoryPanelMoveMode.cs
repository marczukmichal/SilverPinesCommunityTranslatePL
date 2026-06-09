using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InventoryPanelMoveMode : MonoBehaviour, IMoveHandler, IEventSystemHandler
{
	[Header("UI")]
	[SerializeField]
	private RectTransform m_screenRectTransform;

	[SerializeField]
	private InventoryItemCell m_itemCell;

	[SerializeField]
	private RectTransform m_itemCellRectTransform;

	[SerializeField]
	private InventoryPanel m_mainInventoryPanel;

	[SerializeField]
	private InventoryPanel m_stashInventoryPanel;

	[SerializeField]
	private PlayerMainInventory m_mainInventory;

	[SerializeField]
	private RectTransform m_equippedWeaponsRect;

	[SerializeField]
	private RectTransform[] m_equippedArtifactsSlots;

	[Header("Audio")]
	[SerializeField]
	private AudioEvent m_audioStartMovingItem;

	[SerializeField]
	private AudioEvent m_audioRotateItem;

	[Header("Combining")]
	[SerializeField]
	private InventoryPanelCombineMode m_combineMode;

	private ItemInstance m_activeItem;

	private bool m_isItemRotated;

	private InventoryPanel m_originPanel;

	private InventoryPanel m_hoveredPanel;

	private Vector2Int m_inventoryPanelCellPosition;

	private bool m_isInUnresolvableState;

	private Vector2Int m_gampadCursorCellPosition;

	private Vector2 m_cursorScreenPoint;

	private bool m_isInUnsolvedState;

	private void Start()
	{
		m_itemCell.gameObject.SetActive(value: false);
	}

	public bool IsMoveModeActive()
	{
		return m_activeItem != null;
	}

	public void SetActive(ItemInstance item, InventoryPanel requestedPanel)
	{
		m_isInUnsolvedState = false;
		m_activeItem = item;
		m_itemCell.SetItem(item);
		m_itemCell.gameObject.SetActive(value: true);
		m_isItemRotated = item.ItemRotated;
		m_hoveredPanel = (m_originPanel = requestedPanel);
		m_gampadCursorCellPosition = (m_inventoryPanelCellPosition = item.ItemGridPosition);
		EventSystem.current.SetSelectedGameObject(base.gameObject);
		AudioEvent.Play2D((item.ItemDefinition.Audio.OnStartMoving != null) ? item.ItemDefinition.Audio.OnStartMoving : m_audioStartMovingItem);
	}

	public void Deactivate()
	{
		m_activeItem = null;
		m_itemCell.gameObject.SetActive(value: false);
		m_mainInventoryPanel.ClearItemBlockStatus();
		m_stashInventoryPanel.ClearItemBlockStatus();
	}

	private void CheckForPanelHover(Vector2 cursorScreenPoint, Vector2 pivotPosition)
	{
	}

	public void LateUpdate()
	{
		if (!IsMoveModeActive())
		{
			return;
		}
		InputState.Mode inputMode = GlobalReferences.Instance.InputState.InputMode;
		m_cursorScreenPoint = Vector2.zero;
		switch (inputMode)
		{
		case InputState.Mode.KeyboardMouse:
			m_cursorScreenPoint = Mouse.current.position.ReadValue();
			break;
		case InputState.Mode.Gamepad:
			if (m_hoveredPanel == null)
			{
				m_hoveredPanel = m_originPanel;
			}
			m_cursorScreenPoint = m_hoveredPanel.GetScreenPositionForCellPosition(m_gampadCursorCellPosition);
			break;
		}
		Vector2 cursorScreenPoint = m_cursorScreenPoint;
		switch (inputMode)
		{
		case InputState.Mode.KeyboardMouse:
			cursorScreenPoint.x -= m_itemCellRectTransform.rect.width * 0.5f * m_screenRectTransform.lossyScale.x;
			cursorScreenPoint.y += m_itemCellRectTransform.rect.height * 0.5f * m_screenRectTransform.lossyScale.y;
			if (m_mainInventoryPanel.UpdateCursorHoverFromMoveMode(m_cursorScreenPoint, cursorScreenPoint, m_activeItem, m_isItemRotated, out m_inventoryPanelCellPosition))
			{
				m_hoveredPanel = m_mainInventoryPanel;
			}
			else if (m_stashInventoryPanel.isActiveAndEnabled && m_stashInventoryPanel.UpdateCursorHoverFromMoveMode(m_cursorScreenPoint, cursorScreenPoint, m_activeItem, m_isItemRotated, out m_inventoryPanelCellPosition))
			{
				m_hoveredPanel = m_stashInventoryPanel;
			}
			else
			{
				m_hoveredPanel = null;
			}
			break;
		case InputState.Mode.Gamepad:
			m_cursorScreenPoint.x += 16f;
			m_cursorScreenPoint.y -= 16f;
			m_hoveredPanel.UpdateCursorHoverFromMoveMode(m_cursorScreenPoint, cursorScreenPoint, m_activeItem, m_isItemRotated, out m_inventoryPanelCellPosition);
			break;
		}
		RectTransformUtility.ScreenPointToLocalPointInRectangle(m_screenRectTransform, cursorScreenPoint, null, out var localPoint);
		m_itemCellRectTransform.localPosition = localPoint;
	}

	private bool TryUseInMinigameApplyItem()
	{
		InventoryApplyItemInteract inventoryApplyItemInteract = Object.FindFirstObjectByType<InventoryApplyItemInteract>();
		if (inventoryApplyItemInteract != null && RectTransformUtility.RectangleContainsScreenPoint(inventoryApplyItemInteract.ClickableArea, m_cursorScreenPoint))
		{
			return true;
		}
		MinigameInteractor minigameInteractor = Object.FindFirstObjectByType<MinigameInteractor>();
		if (minigameInteractor != null)
		{
			Interactable activeInteractable = minigameInteractor.ActiveInteractable;
			if (activeInteractable != null)
			{
				RectTransform component = activeInteractable.GetComponent<RectTransform>();
				if (component != null && RectTransformUtility.RectangleContainsScreenPoint(component, m_cursorScreenPoint))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void TryPlaceItem()
	{
		if (!IsMoveModeActive())
		{
			return;
		}
		InventoryPanel originPanel = m_originPanel;
		InventoryPanel hoveredPanel = m_hoveredPanel;
		if (m_hoveredPanel != null)
		{
			ItemInstance blockedItem;
			InventoryPanel.TryPlaceItemResult tryPlaceItemResult = m_hoveredPanel.ValidatePlaceItemInPosition(m_activeItem, m_inventoryPanelCellPosition, m_isItemRotated, out blockedItem);
			if (tryPlaceItemResult != InventoryPanel.TryPlaceItemResult.Failed)
			{
				bool flag = false;
				bool flag2 = false;
				if (hoveredPanel != originPanel)
				{
					Inventory.TransferItemOwnership(originPanel.TrackedInventory, hoveredPanel.TrackedInventory, m_activeItem);
					flag = true;
				}
				hoveredPanel.TryPlaceItemInPositionAndExitMoveMode(m_activeItem, m_inventoryPanelCellPosition, m_isItemRotated, out var blockedItem2);
				switch (tryPlaceItemResult)
				{
				case InventoryPanel.TryPlaceItemResult.OK:
					m_isInUnresolvableState = false;
					flag2 = true;
					break;
				case InventoryPanel.TryPlaceItemResult.PickUpNewItem:
					m_isInUnresolvableState = true;
					break;
				}
				if (flag)
				{
					originPanel.RefreshData();
					originPanel.ClearSelectionInfo();
				}
				if (flag2)
				{
					Deactivate();
					hoveredPanel.TempBlockInput();
					originPanel.ActiveMode = InventoryPanel.Mode.Normal;
					hoveredPanel.ActiveMode = InventoryPanel.Mode.Normal;
					hoveredPanel.CursorCellPosition = m_inventoryPanelCellPosition;
					EventSystem.current.SetSelectedGameObject(hoveredPanel.gameObject);
				}
				else if (flag && tryPlaceItemResult == InventoryPanel.TryPlaceItemResult.PickUpNewItem)
				{
					originPanel.ActiveMode = InventoryPanel.Mode.Normal;
					hoveredPanel.RefreshAndMoveItem(blockedItem2);
					m_originPanel = m_hoveredPanel;
				}
				if (tryPlaceItemResult == InventoryPanel.TryPlaceItemResult.PickUpNewItem)
				{
					m_isInUnsolvedState = true;
				}
			}
		}
		else if (!m_isInUnsolvedState && TryUseInMinigameApplyItem())
		{
			Deactivate();
			originPanel.ApplySelectedItem();
			originPanel.TryClearModeAndSelection();
		}
		else if (!m_isInUnsolvedState && m_equippedWeaponsRect.gameObject.activeInHierarchy && RectTransformUtility.RectangleContainsScreenPoint(m_equippedWeaponsRect, m_cursorScreenPoint))
		{
			GlobalReferences.Instance.EventChannels.Inventory.RequestEquipItem.Raise(m_activeItem);
			Deactivate();
			originPanel.TryClearModeAndSelection();
		}
	}

	public bool TryExitMoveMode()
	{
		return !m_isInUnresolvableState;
	}

	private bool CanRotateItem()
	{
		if (m_activeItem == null)
		{
			return false;
		}
		return true;
	}

	public void RotateInput()
	{
		if (!IsMoveModeActive() || !CanRotateItem())
		{
			return;
		}
		if (m_audioRotateItem != null)
		{
			m_audioRotateItem.Play2D();
		}
		m_isItemRotated = !m_isItemRotated;
		m_itemCell.SetItem(m_activeItem, modifySize: true, overrideRotation: true, m_isItemRotated);
		if (GlobalReferences.Instance.InputState.InputMode == InputState.Mode.Gamepad)
		{
			if (m_hoveredPanel == null)
			{
				m_hoveredPanel = m_originPanel;
			}
			m_gampadCursorCellPosition = m_hoveredPanel.ClampCellPositionForItem(m_gampadCursorCellPosition, m_activeItem, m_isItemRotated);
		}
	}

	public void OnMove(AxisEventData eventData)
	{
		if (!IsMoveModeActive())
		{
			return;
		}
		Vector2Int vector2Int = new Vector2Int(Mathf.RoundToInt(eventData.moveVector.x), Mathf.RoundToInt(eventData.moveVector.y));
		vector2Int.x = Mathf.Clamp(vector2Int.x, -1, 1);
		vector2Int.y = Mathf.Clamp(vector2Int.y, -1, 1);
		if (vector2Int.x == 0 && vector2Int.y == 0)
		{
			return;
		}
		vector2Int.y *= -1;
		m_gampadCursorCellPosition += vector2Int;
		if (m_stashInventoryPanel.isActiveAndEnabled)
		{
			if (m_hoveredPanel == m_mainInventoryPanel && m_gampadCursorCellPosition.x < 0)
			{
				m_hoveredPanel = m_stashInventoryPanel;
				m_mainInventoryPanel.ClearSelectionInfo();
				m_gampadCursorCellPosition.x = m_stashInventoryPanel.TrackedInventory.InventorySize.x - 1;
			}
			else if (m_hoveredPanel == m_stashInventoryPanel && m_gampadCursorCellPosition.x + m_activeItem.GetItemSize(m_isItemRotated).x > m_stashInventoryPanel.TrackedInventory.InventorySize.x)
			{
				m_hoveredPanel = m_mainInventoryPanel;
				m_stashInventoryPanel.ClearSelectionInfo();
				m_gampadCursorCellPosition.x = 0;
			}
		}
		m_gampadCursorCellPosition = m_hoveredPanel.ClampCellPositionForItem(m_gampadCursorCellPosition, m_activeItem, m_isItemRotated);
	}
}
