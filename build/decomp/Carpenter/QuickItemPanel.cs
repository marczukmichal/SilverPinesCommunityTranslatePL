using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class QuickItemPanel : MonoBehaviour
{
	private struct QuickItemInstance
	{
		public ItemDefinition m_itemDefinition;

		public ItemInstance m_primaryItemInstance;

		public int m_count;
	}

	[SerializeField]
	private GameObject m_container;

	[SerializeField]
	private QuickItemPanelEntry m_itemPanelEntry;

	[SerializeField]
	private GameObject[] m_keyboardButtonPrompts;

	[SerializeField]
	private float m_hideTimer;

	[SerializeField]
	private CanvasGroup m_canvasGroup;

	[SerializeField]
	private GameObject m_arrowsContainer;

	[SerializeField]
	private PlayerMainInventory m_mainInventory;

	[Header("Animation Settings")]
	[SerializeField]
	private float m_showAnimationTime = 0.25f;

	[SerializeField]
	private float m_hideAnimationTime = 1f;

	private bool m_isShowing;

	private QuickItemInstance[] m_usableItems = new QuickItemInstance[32];

	private int m_usableItemCount;

	private ItemDefinition m_selectedItemDefinition;

	private int m_selectedItemIndex;

	private float m_autoHideTimer;

	private static float m_lastUsedQuickItemTime = -1f;

	public static bool UsedQuickItemRecently => Time.time < m_lastUsedQuickItemTime + 1f;

	private void Awake()
	{
		m_isShowing = false;
		m_container.SetActive(value: false);
	}

	private void OnEnable()
	{
		m_lastUsedQuickItemTime = -100f;
		m_canvasGroup.alpha = 0f;
		GlobalReferences.Instance.EventChannels.Inventory.ShowQuickItemPanel.Register(ShowQuickItemPanel);
		GlobalReferences.Instance.EventChannels.Gameplay.ShowAimingUI.Register(ShowQuickItemPanel);
		GlobalReferences.Instance.EventChannels.Generic.MeleeWeaponDurabilityChanged.Register(OnUsedMeleeWeapon);
		GlobalReferences.Instance.EventChannels.Inventory.AllowComplexInventoryActionsChanged.Register(OnComplexInventoryActionsAllowedChanged);
		GlobalReferences.Instance.EventChannels.Inventory.ItemActivatedChanged.Register(OnItemActivatedChanged);
		GlobalReferences.Instance.EventChannels.Inventory.OnItemRemoved.Register(OnItemRemoved);
		GameInputManager.GameInputActions.Player.QuickItemUse.performed += OnQuickItemPerformed;
		GameInputManager.GameInputActions.Player.QuickItemRight.performed += OnQuickItemCycleRight;
		GameInputManager.GameInputActions.Player.QuickItemLeft.performed += OnQuickItemCycleLeft;
		InputState inputState = GlobalReferences.Instance.InputState;
		inputState.OnInputModeChanged = (UnityAction<InputState.Mode>)Delegate.Combine(inputState.OnInputModeChanged, new UnityAction<InputState.Mode>(OnInputModeChanged));
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.Inventory.ShowQuickItemPanel.Unregister(ShowQuickItemPanel);
		GlobalReferences.Instance.EventChannels.Gameplay.ShowAimingUI.Unregister(ShowQuickItemPanel);
		GlobalReferences.Instance.EventChannels.Generic.MeleeWeaponDurabilityChanged.Unregister(OnUsedMeleeWeapon);
		GlobalReferences.Instance.EventChannels.Inventory.AllowComplexInventoryActionsChanged.Unregister(OnComplexInventoryActionsAllowedChanged);
		GlobalReferences.Instance.EventChannels.Inventory.ItemActivatedChanged.Unregister(OnItemActivatedChanged);
		GlobalReferences.Instance.EventChannels.Inventory.OnItemRemoved.Unregister(OnItemRemoved);
		InputState inputState = GlobalReferences.Instance.InputState;
		inputState.OnInputModeChanged = (UnityAction<InputState.Mode>)Delegate.Remove(inputState.OnInputModeChanged, new UnityAction<InputState.Mode>(OnInputModeChanged));
		if (GameInputManager.GameInputActions != null)
		{
			GameInputManager.GameInputActions.Player.QuickItemUse.performed -= OnQuickItemPerformed;
			GameInputManager.GameInputActions.Player.QuickItemRight.performed -= OnQuickItemCycleRight;
			GameInputManager.GameInputActions.Player.QuickItemLeft.performed -= OnQuickItemCycleLeft;
		}
	}

	private void OnInputModeChanged(InputState.Mode newMode)
	{
		bool active = newMode == InputState.Mode.KeyboardMouse;
		GameObject[] keyboardButtonPrompts = m_keyboardButtonPrompts;
		for (int i = 0; i < keyboardButtonPrompts.Length; i++)
		{
			keyboardButtonPrompts[i].SetActive(active);
		}
	}

	private void OnUsedMeleeWeapon(MeleeWeaponDurabilityChangedData _)
	{
		ShowQuickItemPanel(showing: true);
	}

	private void OnItemActivatedChanged(ItemInstance activated)
	{
		if (m_isShowing && m_itemPanelEntry.ItemDefinition == activated.ItemDefinition)
		{
			PopulateUsableItems();
		}
	}

	private void OnItemRemoved(ItemInstance activated)
	{
		if (m_isShowing)
		{
			PopulateUsableItems();
		}
	}

	private void Update()
	{
		if (m_isShowing && Time.time > m_autoHideTimer)
		{
			ShowQuickItemPanel(showing: false);
		}
	}

	private void ShowQuickItemPanel(bool showing)
	{
		if (showing && m_isShowing)
		{
			m_autoHideTimer = Time.time + m_hideTimer;
			return;
		}
		PopulateUsableItems();
		if (showing && m_usableItemCount == 0)
		{
			showing = false;
		}
		if (m_isShowing == showing)
		{
			return;
		}
		m_isShowing = showing;
		DOTween.Kill(m_canvasGroup);
		if (showing)
		{
			m_canvasGroup.DOFade(1f, m_showAnimationTime);
			m_container.SetActive(value: true);
		}
		else
		{
			m_canvasGroup.DOFade(0f, m_hideAnimationTime).OnComplete(delegate
			{
				m_container.SetActive(value: false);
			});
		}
		if (showing)
		{
			bool active = GlobalReferences.Instance.InputState.InputMode == InputState.Mode.KeyboardMouse;
			GameObject[] keyboardButtonPrompts = m_keyboardButtonPrompts;
			for (int i = 0; i < keyboardButtonPrompts.Length; i++)
			{
				keyboardButtonPrompts[i].SetActive(active);
			}
			m_autoHideTimer = Time.time + m_hideTimer;
		}
	}

	private void PopulateUsableItems()
	{
		m_usableItemCount = 0;
		for (int i = 0; i < m_usableItems.Length; i++)
		{
			m_usableItems[i].m_count = 0;
			m_usableItems[i].m_itemDefinition = null;
		}
		PlayerMainInventory mainInventory = m_mainInventory;
		int index = 0;
		foreach (ItemInstance item in mainInventory.ItemList)
		{
			if (item == null || item.ItemDefinition.HideFromQuickItem)
			{
				continue;
			}
			if (m_usableItemCount >= m_usableItems.Length)
			{
				break;
			}
			if (!item.CanUse(mainInventory, checkConditions: false) && !item.CanBeActivated())
			{
				continue;
			}
			bool flag = false;
			for (int j = 0; j < m_usableItemCount; j++)
			{
				if (m_usableItems[j].m_itemDefinition == item.ItemDefinition)
				{
					flag = true;
					m_usableItems[j].m_count += item.StackSize;
					break;
				}
			}
			if (!flag)
			{
				if (item.ItemDefinition == m_selectedItemDefinition)
				{
					index = m_usableItemCount;
				}
				m_usableItems[m_usableItemCount].m_count = item.StackSize;
				m_usableItems[m_usableItemCount].m_itemDefinition = item.ItemDefinition;
				m_usableItems[m_usableItemCount].m_primaryItemInstance = item;
				m_usableItemCount++;
			}
		}
		if (m_usableItemCount > 0)
		{
			SelectItem(index);
			m_arrowsContainer.SetActive(m_usableItemCount > 1);
		}
	}

	private void SelectItem(int index)
	{
		m_selectedItemDefinition = m_usableItems[index].m_itemDefinition;
		m_selectedItemIndex = index;
		RefreshItemPanelItemEntry();
	}

	private void RefreshItemPanelItemEntry()
	{
		ItemDefinition itemDefinition = m_usableItems[m_selectedItemIndex].m_itemDefinition;
		ItemInstance primaryItemInstance = m_usableItems[m_selectedItemIndex].m_primaryItemInstance;
		if (itemDefinition == null)
		{
			return;
		}
		int count = m_usableItems[m_selectedItemIndex].m_count;
		if (primaryItemInstance is RefillableItemInstance refillableItemInstance)
		{
			count = refillableItemInstance.Uses;
		}
		bool isUsable = itemDefinition.CanUse(m_mainInventory, checkConditions: true) || itemDefinition.CanBeActivated();
		if (!string.IsNullOrEmpty(itemDefinition.UseFSMEvent))
		{
			GameObject item = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item;
			if (item != null)
			{
				CharacterInventory component = item.GetComponent<CharacterInventory>();
				if (component != null)
				{
					bool flag = component.ComplexInventoryActionsAllowed == CharacterInventory.ComplexInventoryActionsMode.All;
					if (component.ComplexInventoryActionsAllowed == CharacterInventory.ComplexInventoryActionsMode.SecondaryOnly && itemDefinition is SecondaryWeaponItemDefinition)
					{
						flag = true;
					}
					if (!flag)
					{
						isUsable = false;
					}
				}
			}
		}
		m_itemPanelEntry.Populate(itemDefinition, primaryItemInstance, count, isUsable);
	}

	private void OnComplexInventoryActionsAllowedChanged(bool allowed)
	{
		RefreshItemPanelItemEntry();
	}

	private void QuickItemRequestUse()
	{
		ShowQuickItemPanel(showing: true);
		if (m_usableItemCount != 0)
		{
			ItemDefinition itemDefinition = m_usableItems[m_selectedItemIndex].m_itemDefinition;
			if (itemDefinition != null)
			{
				GlobalReferences.Instance.EventChannels.Inventory.SelectedItemFromQuickItemPanel.Raise(itemDefinition);
				RefreshItemPanelItemEntry();
			}
		}
	}

	private void OnQuickItemPerformed(InputAction.CallbackContext context)
	{
		if (m_isShowing)
		{
			QuickItemRequestUse();
		}
		else
		{
			ShowQuickItemPanel(showing: true);
		}
	}

	private void OnQuickItemCycleLeft(InputAction.CallbackContext context)
	{
		ShowQuickItemPanel(showing: true);
		if (m_usableItemCount > 1)
		{
			CycleItem(forward: false);
		}
	}

	private void OnQuickItemCycleRight(InputAction.CallbackContext context)
	{
		ShowQuickItemPanel(showing: true);
		if (m_usableItemCount > 1)
		{
			CycleItem(forward: true);
		}
	}

	private void CycleItem(bool forward)
	{
		m_lastUsedQuickItemTime = Time.time;
		int selectedItemIndex = m_selectedItemIndex;
		if (forward)
		{
			selectedItemIndex++;
			if (selectedItemIndex >= m_usableItemCount)
			{
				selectedItemIndex = 0;
			}
		}
		else
		{
			selectedItemIndex--;
			if (selectedItemIndex < 0)
			{
				selectedItemIndex = m_usableItemCount - 1;
			}
		}
		SelectItem(selectedItemIndex);
	}
}
