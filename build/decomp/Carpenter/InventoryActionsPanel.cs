using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.UI;

public class InventoryActionsPanel : MonoBehaviour
{
	[Header("Inventory Panel")]
	[SerializeField]
	private InventoryPanel m_parentInventoryPanel;

	[Header("UI")]
	[SerializeField]
	private GameObject m_container;

	[SerializeField]
	private CanvasGroup m_canvasGroup;

	[SerializeField]
	private ApplyItemAnchor m_activeApplyItemInteractable;

	[SerializeField]
	private InteractableAnchor m_itemBoxInteractableAnchor;

	[SerializeField]
	private InventoryActionButton m_moveButton;

	[SerializeField]
	private InventoryActionButton m_applyButton;

	[SerializeField]
	private InventoryActionButton m_examineButton;

	[SerializeField]
	private InventoryActionButton m_useButton;

	[SerializeField]
	private InventoryActionButton m_equipButton;

	[SerializeField]
	private InventoryActionButton m_combineButton;

	[SerializeField]
	private InventoryActionButton m_unloadButton;

	[SerializeField]
	private InventoryActionButton m_activateButton;

	[SerializeField]
	private InventoryActionButton m_specialButton;

	[SerializeField]
	private InventoryActionButton m_transferButton;

	[SerializeField]
	private InventoryActionButton m_toggleItemWheelButton;

	[SerializeField]
	private InventoryActionButton m_dropButton;

	[Header("Data")]
	[SerializeField]
	private Inventory m_inventory;

	[SerializeField]
	private Inventory m_transferInventory;

	[Header("Animation")]
	[SerializeField]
	private float m_showAnimTime = 0.25f;

	[SerializeField]
	private InputState m_inputState;

	[SerializeField]
	private bool m_flipPivot;

	[Header("Strings")]
	[SerializeField]
	private LocalizedString m_equipLocalizedString;

	[SerializeField]
	private LocalizedString m_unequipLocalizedString;

	[SerializeField]
	private LocalizedString m_setShortcutLocalizedString;

	[SerializeField]
	private LocalizedString m_unSetShortcutLocalizedString;

	[Header("Icons")]
	[SerializeField]
	private Sprite m_equipMeleeIcon;

	[SerializeField]
	private Sprite m_equipRangedIcon;

	[SerializeField]
	private Sprite m_equipArtifactIcon;

	[SerializeField]
	private Sprite m_equipSecondaryIcon;

	[SerializeField]
	private Sprite m_useIcon;

	[SerializeField]
	private Sprite m_activateIcon;

	private RectTransform m_rectTransform;

	private ItemInstance m_activeItem;

	private bool m_isShown;

	private bool m_canUseItem;

	private bool m_canDropItem;

	private bool m_canCombineItem;

	private bool IsApplyMode => m_activeApplyItemInteractable.Item != null;

	private void OnEnable()
	{
		InventoryPanel parentInventoryPanel = m_parentInventoryPanel;
		parentInventoryPanel.m_onModeChanged = (UnityAction<InventoryPanel.Mode>)Delegate.Combine(parentInventoryPanel.m_onModeChanged, new UnityAction<InventoryPanel.Mode>(SetInventoryPanelMode));
		m_parentInventoryPanel.m_onSelectedItemChangedEvent.AddListener(SetActiveItem);
	}

	private void OnDisable()
	{
		InventoryPanel parentInventoryPanel = m_parentInventoryPanel;
		parentInventoryPanel.m_onModeChanged = (UnityAction<InventoryPanel.Mode>)Delegate.Remove(parentInventoryPanel.m_onModeChanged, new UnityAction<InventoryPanel.Mode>(SetInventoryPanelMode));
		m_parentInventoryPanel.m_onSelectedItemChangedEvent.RemoveListener(SetActiveItem);
	}

	private void Start()
	{
		m_rectTransform = m_container.GetComponent<RectTransform>();
		m_rectTransform.pivot = new Vector2(m_flipPivot ? 1f : 0f, 1f);
		m_container.SetActive(value: false);
		RegisterListeners();
		ApplyDefaultIcons();
	}

	private void RegisterListeners()
	{
		if (m_applyButton != null)
		{
			InventoryActionButton applyButton = m_applyButton;
			applyButton.m_onClickEvent = (UnityAction)Delegate.Combine(applyButton.m_onClickEvent, new UnityAction(OnApplyItemButtonClicked));
		}
		if (m_useButton != null)
		{
			InventoryActionButton useButton = m_useButton;
			useButton.m_onClickEvent = (UnityAction)Delegate.Combine(useButton.m_onClickEvent, new UnityAction(OnUseItemButtonClicked));
		}
		if (m_equipButton != null)
		{
			InventoryActionButton equipButton = m_equipButton;
			equipButton.m_onClickEvent = (UnityAction)Delegate.Combine(equipButton.m_onClickEvent, new UnityAction(OnEquipItemButtonClicked));
		}
		if (m_combineButton != null)
		{
			InventoryActionButton combineButton = m_combineButton;
			combineButton.m_onClickEvent = (UnityAction)Delegate.Combine(combineButton.m_onClickEvent, new UnityAction(OnCombineItemButtonClicked));
		}
		if (m_unloadButton != null)
		{
			InventoryActionButton unloadButton = m_unloadButton;
			unloadButton.m_onClickEvent = (UnityAction)Delegate.Combine(unloadButton.m_onClickEvent, new UnityAction(OnUnloadButtonClicked));
		}
		if (m_activateButton != null)
		{
			InventoryActionButton activateButton = m_activateButton;
			activateButton.m_onClickEvent = (UnityAction)Delegate.Combine(activateButton.m_onClickEvent, new UnityAction(OnEnableItemButtonClicked));
		}
		if (m_specialButton != null)
		{
			InventoryActionButton specialButton = m_specialButton;
			specialButton.m_onClickEvent = (UnityAction)Delegate.Combine(specialButton.m_onClickEvent, new UnityAction(OnSpecialItemButtonClicked));
		}
		if (m_transferButton != null)
		{
			InventoryActionButton transferButton = m_transferButton;
			transferButton.m_onClickEvent = (UnityAction)Delegate.Combine(transferButton.m_onClickEvent, new UnityAction(OnTransferItemButtonClicked));
		}
		if (m_toggleItemWheelButton != null)
		{
			InventoryActionButton toggleItemWheelButton = m_toggleItemWheelButton;
			toggleItemWheelButton.m_onClickEvent = (UnityAction)Delegate.Combine(toggleItemWheelButton.m_onClickEvent, new UnityAction(OnToggleItemWheelButtonClicked));
		}
		if (m_dropButton != null)
		{
			InventoryActionButton dropButton = m_dropButton;
			dropButton.m_onClickEvent = (UnityAction)Delegate.Combine(dropButton.m_onClickEvent, new UnityAction(OnDropButtonClicked));
		}
		if (m_examineButton != null)
		{
			InventoryActionButton examineButton = m_examineButton;
			examineButton.m_onClickEvent = (UnityAction)Delegate.Combine(examineButton.m_onClickEvent, new UnityAction(OnExamineButtonClicked));
		}
	}

	private void ApplyDefaultIcons()
	{
		if (m_useButton != null)
		{
			m_useButton.SetIconSprite(m_useIcon);
		}
		if (m_activateButton != null)
		{
			m_activateButton.SetIconSprite(m_activateIcon);
		}
	}

	private void SetButtonsActive(bool active)
	{
		Selectable[] componentsInChildren = GetComponentsInChildren<Selectable>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].interactable = active;
		}
	}

	private void SetShown()
	{
		if (!m_isShown)
		{
			m_isShown = true;
			m_container.SetActive(value: true);
			DOTween.Kill(m_rectTransform);
			m_rectTransform.DOPivotX(m_flipPivot ? 0f : 1f, m_showAnimTime);
			SetButtonsActive(active: true);
			m_canvasGroup.interactable = true;
			SelectFirstButton();
		}
	}

	private void SetHidden()
	{
		if (m_isShown)
		{
			m_isShown = false;
			DOTween.Kill(m_rectTransform);
			m_canvasGroup.interactable = false;
			m_rectTransform.DOPivotX(m_flipPivot ? 1f : 0f, m_showAnimTime).OnComplete(delegate
			{
				m_container.SetActive(value: false);
			});
			SetButtonsActive(active: false);
		}
	}

	private bool IsItemBoxActive()
	{
		return m_itemBoxInteractableAnchor.Item != null;
	}

	private bool CanBeCombinedWithAnything(ItemInstance item)
	{
		if (!item.ItemDefinition.CanBeCombinedWithAnything())
		{
			return true;
		}
		if (IsItemBoxActive())
		{
			if (!m_inventory.CanItemBeCombinedWithAnything(item))
			{
				return m_transferInventory.CanItemBeCombinedWithAnything(item);
			}
			return true;
		}
		return m_inventory.CanItemBeCombinedWithAnything(item);
	}

	private void PopulateFields(ItemInstance itemInstance)
	{
		bool active = false;
		PlayerMainInventory playerMainInventory = m_inventory as PlayerMainInventory;
		m_canCombineItem = CanBeCombinedWithAnything(itemInstance);
		m_canUseItem = itemInstance.CanUse(m_inventory, checkConditions: true);
		bool flag = false;
		GameObject item = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item;
		if (item != null)
		{
			CharacterInventory component = item.GetComponent<CharacterInventory>();
			if (component != null && component.UsingTempInventory)
			{
				flag = true;
			}
		}
		if ((bool)m_equipButton)
		{
			bool buttonEnabled = itemInstance.CanEquip(m_inventory) && !IsApplyMode && !flag;
			if (playerMainInventory != null && !IsApplyMode)
			{
				active = itemInstance.CanEquip(playerMainInventory);
				if (itemInstance is MeleeWeaponItemInstance)
				{
					m_equipButton.SetIconSprite(m_equipMeleeIcon);
					m_equipButton.SetString((playerMainInventory.EquippedMeleeItem == itemInstance) ? m_unequipLocalizedString : m_equipLocalizedString);
				}
				else if (itemInstance is ProjectileWeaponItemInstance)
				{
					m_equipButton.SetIconSprite(m_equipRangedIcon);
					m_equipButton.SetString((playerMainInventory.EquippedRangedItem == itemInstance) ? m_unequipLocalizedString : m_equipLocalizedString);
				}
				else if (itemInstance is SecondaryWeaponItemInstance)
				{
					m_equipButton.SetIconSprite(m_equipSecondaryIcon);
					m_equipButton.SetString((playerMainInventory.EquippedSecondaryItem == itemInstance) ? m_unequipLocalizedString : m_equipLocalizedString);
				}
				else if (itemInstance is ArtifactItemInstance)
				{
					m_equipButton.SetIconSprite(m_equipArtifactIcon);
					m_equipButton.SetString(playerMainInventory.IsArtifactEquipped(itemInstance) ? m_unequipLocalizedString : m_equipLocalizedString);
					active = true;
				}
			}
			m_equipButton.gameObject.SetActive(active);
			m_equipButton.SetButtonEnabled(buttonEnabled);
		}
		if ((bool)m_applyButton)
		{
			m_applyButton.gameObject.SetActive(IsApplyMode);
		}
		bool flag2 = false;
		if (!IsApplyMode && !flag && itemInstance.CanUse(m_inventory, checkConditions: false) && !itemInstance.ItemDefinition.HideUseButton())
		{
			flag2 = true;
		}
		if ((bool)m_useButton)
		{
			m_useButton.gameObject.SetActive(flag2);
			if (flag2)
			{
				m_useButton.SetButtonEnabled(itemInstance.CanUse(m_inventory, checkConditions: true));
			}
		}
		if ((bool)m_combineButton)
		{
			m_combineButton.gameObject.SetActive(!flag);
		}
		if ((bool)m_unloadButton)
		{
			m_unloadButton.gameObject.SetActive(m_inventory.CanUnloadItem(itemInstance) && !IsApplyMode && !flag);
		}
		if ((bool)m_moveButton)
		{
			m_moveButton.gameObject.SetActive(!IsItemBoxActive() && !IsApplyMode);
		}
		if ((bool)m_activateButton)
		{
			m_activateButton.gameObject.SetActive(itemInstance.CanBeActivated() && !IsApplyMode && !flag);
			if (itemInstance.CanBeActivated())
			{
				m_activateButton.SetString(itemInstance.Activated ? itemInstance.ItemDefinition.Activatable.DeactivateActionStringReference : itemInstance.ItemDefinition.Activatable.ActivateActionStringReference);
			}
		}
		if ((bool)m_specialButton)
		{
			m_specialButton.gameObject.SetActive(itemInstance.ItemDefinition.HasSpecialAction() && !IsApplyMode && !flag);
			if (itemInstance.ItemDefinition.HasSpecialAction())
			{
				m_specialButton.SetString(itemInstance.ItemDefinition.GetSpecialActionText());
			}
		}
		if ((bool)m_transferButton)
		{
			m_transferButton.gameObject.SetActive(IsItemBoxActive() && m_parentInventoryPanel.TrackedInventory.IsItemMovable(itemInstance));
		}
		if ((bool)m_toggleItemWheelButton)
		{
			m_toggleItemWheelButton.gameObject.SetActive(value: false);
		}
		if ((bool)m_dropButton)
		{
			m_canDropItem = itemInstance.ItemDefinition.IsDroppable && m_itemBoxInteractableAnchor.Item == null && !flag;
			m_dropButton.gameObject.SetActive(itemInstance.ItemDefinition.IsDroppable && !IsItemBoxActive() && !IsApplyMode);
		}
		ConfigureNavigation();
	}

	private void ConfigureNavigation()
	{
		Selectable[] componentsInChildren = GetComponentsInChildren<Selectable>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].navigation = default(Navigation);
		}
		for (int j = 0; j < componentsInChildren.Length - 1; j++)
		{
			Selectable selectable = componentsInChildren[j];
			Selectable selectable2 = componentsInChildren[j + 1];
			Navigation navigation = default(Navigation);
			navigation.mode = Navigation.Mode.Explicit;
			navigation.selectOnDown = selectable2;
			navigation.selectOnUp = selectable.navigation.selectOnUp;
			Navigation navigation3 = (selectable.navigation = navigation);
			navigation = default(Navigation);
			navigation.mode = Navigation.Mode.Explicit;
			navigation.selectOnUp = selectable;
			Navigation navigation5 = (selectable2.navigation = navigation);
		}
	}

	private void SetActiveItem(ItemInstance itemInstance)
	{
		if (m_activeItem != itemInstance)
		{
			m_activeItem = itemInstance;
			UpdateVisibility();
		}
	}

	private void UpdateVisibility()
	{
		if (m_activeItem != null && m_parentInventoryPanel.ActiveMode != InventoryPanel.Mode.Move && m_parentInventoryPanel.ActiveMode != InventoryPanel.Mode.Combine)
		{
			PopulateFields(m_activeItem);
			SetShown();
		}
		else
		{
			SetHidden();
		}
	}

	public void SetInteractionEnabled(bool enabled)
	{
		m_canvasGroup.interactable = enabled;
		if (enabled)
		{
			SelectFirstButton();
		}
	}

	private void SelectFirstButton()
	{
		if (m_inputState.InputMode != InputState.Mode.Gamepad)
		{
			return;
		}
		Selectable[] componentsInChildren = GetComponentsInChildren<Selectable>();
		foreach (Selectable selectable in componentsInChildren)
		{
			if (selectable.interactable && selectable.gameObject.activeSelf)
			{
				EventSystem.current.SetSelectedGameObject(selectable.gameObject);
				break;
			}
		}
	}

	private void Update()
	{
		if (!m_isShown)
		{
			return;
		}
		if (EventSystem.current.currentSelectedGameObject == null)
		{
			SelectFirstButton();
		}
		if (IsItemBoxActive())
		{
			if (m_useButton != null)
			{
				m_useButton.SetButtonEnabled(m_canUseItem);
			}
			return;
		}
		if (m_useButton != null)
		{
			m_useButton.SetButtonEnabled(m_canUseItem);
		}
		if (m_dropButton != null)
		{
			m_dropButton.SetButtonEnabled(m_canDropItem);
		}
	}

	public void SetInventoryPanelMode(InventoryPanel.Mode mode)
	{
		if (m_combineButton != null)
		{
			m_combineButton.OnActionActive(mode == InventoryPanel.Mode.Combine);
		}
		if (m_moveButton != null)
		{
			m_moveButton.OnActionActive(mode == InventoryPanel.Mode.Move);
		}
		if (m_specialButton != null)
		{
			m_specialButton.OnActionActive(mode == InventoryPanel.Mode.KeyRingView);
		}
		SetInteractionEnabled(mode == InventoryPanel.Mode.Menu);
		UpdateVisibility();
	}

	private void OnTransferItemButtonClicked()
	{
		m_parentInventoryPanel.TransferSelectedItem();
	}

	private void OnApplyItemButtonClicked()
	{
		m_parentInventoryPanel.ApplySelectedItem();
	}

	private void OnSpecialItemButtonClicked()
	{
		m_parentInventoryPanel.DoSpecialItemAction();
	}

	private void OnEquipItemButtonClicked()
	{
		m_parentInventoryPanel.EquipSelectedItem();
	}

	private void OnEnableItemButtonClicked()
	{
		m_parentInventoryPanel.ToggleActivatedSelectedItem();
	}

	private void OnUseItemButtonClicked()
	{
		m_parentInventoryPanel.UseSelectedItem(!IsItemBoxActive());
	}

	private void OnCombineItemButtonClicked()
	{
		m_parentInventoryPanel.SetCombineMode();
	}

	private void OnToggleItemWheelButtonClicked()
	{
		m_parentInventoryPanel.ToggleItemOnShortcutItemWheel();
	}

	private void OnPowerUpArtifactItemButtonClicked()
	{
		m_parentInventoryPanel.PowerUpSelectedArtifact();
	}

	private void OnUnloadButtonClicked()
	{
		m_parentInventoryPanel.UnloadSelectedItem();
	}

	private void OnDropButtonClicked()
	{
		m_parentInventoryPanel.DropSelectedItem();
	}

	private void OnExamineButtonClicked()
	{
		m_parentInventoryPanel.ExamineSelectedItem();
	}
}
