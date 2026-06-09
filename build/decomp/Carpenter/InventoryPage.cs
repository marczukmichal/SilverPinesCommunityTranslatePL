using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class InventoryPage : MenuPage
{
	[SerializeField]
	private GameObject m_statusContainer;

	[SerializeField]
	protected InventoryPanel m_mainInventoryPanel;

	[SerializeField]
	private InventoryPanel m_stashInventoryPanel;

	[SerializeField]
	private InventoryItemInfoPanel m_itemInfoPanel;

	[SerializeField]
	private InventoryPanelMoveMode m_moveMode;

	[SerializeField]
	private InventoryItemPickupPanel m_pickupPanel;

	[SerializeField]
	private CanvasGroup m_canvasGroup;

	[SerializeField]
	private MenuInputPrompts.AvaialbleInput m_inputCombineInMoveMode;

	[SerializeField]
	private MenuInputPrompts.AvaialbleInput m_inputTransfer;

	[SerializeField]
	private MenuInputPrompts.AvaialbleInput m_inputRotation;

	[SerializeField]
	private MenuInputPrompts.AvaialbleInput m_inputMove;

	[SerializeField]
	private MenuInputPrompts.AvaialbleInput m_inputSelect;

	[SerializeField]
	private MenuInputPrompts.AvaialbleInput m_inputAddToItemWheel;

	private List<MenuInputPrompts.AvaialbleInput> m_inventoryAvailableInputs = new List<MenuInputPrompts.AvaialbleInput>();

	public override MenuInputPrompts.AvaialbleInput[] AvailableInputs
	{
		get
		{
			m_inventoryAvailableInputs.Clear();
			if (!m_pickupPanel.IsShowing)
			{
				bool flag = true;
				ItemInstance itemInstance = m_mainInventoryPanel.GetHoveredItemInstance();
				if (itemInstance == null)
				{
					ItemInstance hoveredItemInstance = m_stashInventoryPanel.GetHoveredItemInstance();
					if (hoveredItemInstance != null)
					{
						itemInstance = hoveredItemInstance;
						flag = false;
					}
				}
				if (itemInstance != null)
				{
					bool flag2 = true;
					if (flag && !m_mainInventoryPanel.TrackedInventory.IsItemMovable(itemInstance))
					{
						flag2 = false;
					}
					if (flag2)
					{
						m_inventoryAvailableInputs.Add(m_inputMove);
					}
					m_inventoryAvailableInputs.Add(m_inputSelect);
				}
				if (m_mainInventoryPanel.CanDoRotateInput() || (m_stashInventoryPanel != null && m_stashInventoryPanel.CanDoRotateInput()))
				{
					m_inventoryAvailableInputs.Add(m_inputRotation);
				}
				if (GlobalReferences.Instance.Anchors.Inventory.ItemBoxInteractableAnchor.Item != null && (m_mainInventoryPanel.CanTransferItem() || m_stashInventoryPanel.CanTransferItem()))
				{
					m_inventoryAvailableInputs.Add(m_inputTransfer);
				}
				if (m_mainInventoryPanel.CanDoShortcutInput())
				{
					m_inventoryAvailableInputs.Add(m_inputAddToItemWheel);
				}
			}
			MenuInputPrompts.AvaialbleInput[] availableInputs = base.AvailableInputs;
			foreach (MenuInputPrompts.AvaialbleInput item in availableInputs)
			{
				m_inventoryAvailableInputs.Add(item);
			}
			return m_inventoryAvailableInputs.ToArray();
		}
	}

	protected override void Awake()
	{
		base.Awake();
		InventoryItemPickupPanel pickupPanel = m_pickupPanel;
		pickupPanel.OnPickupPanelStateChanged = (UnityAction<bool>)Delegate.Combine(pickupPanel.OnPickupPanelStateChanged, new UnityAction<bool>(OnPickupPanelStateChanged));
	}

	private void OnPickupPanelStateChanged(bool showing)
	{
		m_canvasGroup.interactable = !showing;
		m_canvasGroup.alpha = (showing ? 0f : 1f);
	}

	public override void Show(bool instant = false, bool onAwake = false)
	{
		base.Show(instant, onAwake);
	}

	public override void Hide(bool instant = false)
	{
		base.Hide(instant);
		if (m_pickupPanel.IsShowing)
		{
			m_pickupPanel.Cancel();
		}
	}

	public void SetQuickInventoryMode(bool enabled)
	{
		m_statusContainer.SetActive(!enabled);
	}

	public override bool CanExitPage()
	{
		if (m_mainInventoryPanel.CanExitInventoryPanelSafely())
		{
			return !m_pickupPanel.IsShowing;
		}
		return false;
	}

	public override bool OnBackInput()
	{
		if (m_pickupPanel.IsShowing)
		{
			m_pickupPanel.Cancel();
			return true;
		}
		return base.OnBackInput();
	}

	private void RefreshItemInfoPanel()
	{
		if (m_itemInfoPanel != null)
		{
			ItemInstance itemInstance = null;
			itemInstance = m_mainInventoryPanel.GetSelectedItemInstance();
			if (itemInstance == null && m_stashInventoryPanel.isActiveAndEnabled)
			{
				itemInstance = m_stashInventoryPanel.GetSelectedItemInstance();
			}
			if (itemInstance == null)
			{
				itemInstance = m_mainInventoryPanel.GetHoveredItemInstance();
			}
			if (itemInstance == null && m_stashInventoryPanel.isActiveAndEnabled)
			{
				itemInstance = m_stashInventoryPanel.GetHoveredItemInstance();
			}
			bool num = m_moveMode.IsMoveModeActive();
			bool flag = m_mainInventoryPanel.ActiveMode == InventoryPanel.Mode.Combine || (m_stashInventoryPanel.isActiveAndEnabled && m_stashInventoryPanel.ActiveMode == InventoryPanel.Mode.Combine);
			if (num)
			{
				m_itemInfoPanel.SetMode(InventoryItemInfoPanel.Mode.MoveMode);
			}
			else if (flag)
			{
				m_itemInfoPanel.SetMode(InventoryItemInfoPanel.Mode.Combine);
			}
			else
			{
				m_itemInfoPanel.SetMode(InventoryItemInfoPanel.Mode.Normal);
			}
			m_itemInfoPanel.SetActiveItem(itemInstance);
		}
	}

	private void Update()
	{
		RefreshItemInfoPanel();
	}

	public override void SelectDefaultSelectable()
	{
		EventSystem.current.SetSelectedGameObject(m_mainInventoryPanel.gameObject);
	}
}
