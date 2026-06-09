using System;
using System.Collections;
using DG.Tweening;
using Shapes2D;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.UI;

public class InventoryItemPickupPanel : MonoBehaviour, IInputBackHandler
{
	public enum Mode
	{
		GameItemPickup,
		Inventory
	}

	private enum InventoryItemMode
	{
		Combined,
		Examine
	}

	[Header("Event Channels")]
	[SerializeField]
	private ItemPickupAnchor m_activeItemPickupAnchor;

	[Header("UI")]
	[SerializeField]
	private GameObject m_container;

	[SerializeField]
	private InventoryItemInfoPanel m_infoPanel;

	[SerializeField]
	private TextMeshProUGUI m_pickupText;

	[SerializeField]
	private GameObject m_buttonPromptsContainer;

	[SerializeField]
	private GameObject m_takeButtonPrompt;

	[SerializeField]
	private GameObject m_quickUseButtonPrompt;

	[SerializeField]
	private Color m_itemHighlightColor;

	[SerializeField]
	private Color m_normalTextColor;

	[SerializeField]
	private Color m_artifactTextColor;

	[SerializeField]
	private Color m_insufficientSpaceTextColor;

	[SerializeField]
	private CanvasGroup m_canvasGroup;

	[SerializeField]
	private GameObject m_artifactIcon;

	[SerializeField]
	private RectTransform m_buttonPromptParentTransform;

	[Header("Strings")]
	[SerializeField]
	private LocalizedString m_pickupStringReference;

	[SerializeField]
	private LocalizedString m_artifactStringReference;

	[SerializeField]
	private LocalizedString m_notEnoughSpaceTextString;

	[SerializeField]
	private LocalizedString m_takeMoneyStringReference;

	[Header("Item Image")]
	[SerializeField]
	private Image m_itemImage;

	[SerializeField]
	private int m_itemCellWidthForMaxSize = 6;

	[SerializeField]
	private float m_itemImageMaxWidthMargin = -20f;

	[SerializeField]
	private float m_itemImageMinWidthMargin = -650f;

	[Header("Item Grid")]
	[SerializeField]
	private Shape m_grid;

	[Header("Other")]
	[SerializeField]
	private PlayerMainInventory m_inventory;

	[SerializeField]
	private Sprite m_moneySprite;

	[Header("Animation")]
	[SerializeField]
	private float m_fadeInTime;

	[SerializeField]
	private float m_fadeOutTime;

	[Header("Mode")]
	[SerializeField]
	private Mode m_mode;

	[Header("Audio")]
	[SerializeField]
	private AudioEvent m_onNewItemStinger;

	[Header("Animated Element")]
	[SerializeField]
	private RectTransform m_animatedElementParent;

	private GameObject m_animatedElement;

	private bool m_waitingForAnimation;

	private bool m_closing;

	private bool m_isShowing;

	public UnityAction<bool> OnPickupPanelStateChanged;

	private bool m_takeButtonEnabled;

	private bool m_canQuickUse;

	public bool IsShowing
	{
		get
		{
			if (!m_isShowing)
			{
				return m_canvasGroup.alpha > 0f;
			}
			return true;
		}
	}

	public void DoneAnimation()
	{
		m_waitingForAnimation = false;
	}

	private void OnEnable()
	{
		m_container.gameObject.SetActive(value: false);
		m_canvasGroup.alpha = 0f;
		m_canvasGroup.blocksRaycasts = false;
		if (m_mode == Mode.GameItemPickup)
		{
			GameMenuState gameMenuState = GlobalReferences.Instance.GameMenuState;
			gameMenuState.OnGameMenuChanged = (UnityAction<GameMenuState.GameMenu>)Delegate.Combine(gameMenuState.OnGameMenuChanged, new UnityAction<GameMenuState.GameMenu>(OnGameMenuChanged));
		}
		else if (m_mode == Mode.Inventory)
		{
			GlobalReferences.Instance.EventChannels.Inventory.NewItemCombined.Register(OnItemCombinedResult);
			GlobalReferences.Instance.EventChannels.Inventory.ExamineItemInstance.Register(OnExamineItem);
		}
	}

	private void OnDisable()
	{
		if (m_animatedElement != null)
		{
			UnityEngine.Object.Destroy(m_animatedElement);
		}
		if (m_mode == Mode.GameItemPickup)
		{
			GameMenuState gameMenuState = GlobalReferences.Instance.GameMenuState;
			gameMenuState.OnGameMenuChanged = (UnityAction<GameMenuState.GameMenu>)Delegate.Remove(gameMenuState.OnGameMenuChanged, new UnityAction<GameMenuState.GameMenu>(OnGameMenuChanged));
		}
		else if (m_mode == Mode.Inventory)
		{
			GlobalReferences.Instance.EventChannels.Inventory.NewItemCombined.Unregister(OnItemCombinedResult);
			GlobalReferences.Instance.EventChannels.Inventory.ExamineItemInstance.Unregister(OnExamineItem);
		}
		if (GameInputManager.GameInputActions != null)
		{
			GameInputManager.RemoveBackInputHandler(this);
			GameInputManager.GameInputActions.UI.Cancel.performed -= Cancelinput;
			GameInputManager.GameInputActions.UI.TakeItem.performed -= TakeItemInput;
			GameInputManager.GameInputActions.UI.UseItem.performed -= OnQuickuseItemInput;
		}
	}

	private void OnItemCombinedResult(ItemInstance newItem)
	{
		m_onNewItemStinger?.Play2D();
		if (!(newItem is ArtifactItemInstance))
		{
			ShowInventoryItem(newItem, InventoryItemMode.Combined);
		}
	}

	private void OnExamineItem(ItemInstance newItem)
	{
		ShowInventoryItem(newItem, InventoryItemMode.Examine);
	}

	private void ShowInventoryItem(ItemInstance item, InventoryItemMode mode)
	{
		if (!m_isShowing)
		{
			DOTween.Kill(m_canvasGroup);
			m_container.gameObject.SetActive(value: true);
			m_canvasGroup.blocksRaycasts = true;
			m_canvasGroup.DOFade(1f, m_fadeInTime).SetUpdate(isIndependentUpdate: true);
			PopulateForItemInstance(item, mode);
			m_closing = false;
			m_isShowing = true;
			OnPickupPanelStateChanged?.Invoke(arg0: true);
		}
	}

	private void Show()
	{
		if (!m_isShowing)
		{
			DOTween.Kill(m_canvasGroup);
			m_container.gameObject.SetActive(value: true);
			m_canvasGroup.DOFade(1f, m_fadeInTime).SetUpdate(isIndependentUpdate: true);
			if (m_activeItemPickupAnchor != null && m_activeItemPickupAnchor.Item != null)
			{
				PopulateForItemPickup(m_activeItemPickupAnchor.Item);
				GameInputManager.PushBackInputHandler(this);
			}
			m_closing = false;
			m_isShowing = true;
			OnPickupPanelStateChanged?.Invoke(arg0: true);
		}
	}

	private void Hide()
	{
		if (m_isShowing)
		{
			DOTween.Kill(m_canvasGroup);
			m_canvasGroup.DOFade(0f, m_fadeOutTime).SetUpdate(isIndependentUpdate: true).OnComplete(delegate
			{
				m_container.gameObject.SetActive(value: false);
			});
			m_canvasGroup.blocksRaycasts = false;
			if (GameInputManager.GameInputActions != null)
			{
				GameInputManager.RemoveBackInputHandler(this);
				GameInputManager.GameInputActions.UI.Cancel.performed -= Cancelinput;
				GameInputManager.GameInputActions.UI.TakeItem.performed -= TakeItemInput;
				GameInputManager.GameInputActions.UI.UseItem.performed -= OnQuickuseItemInput;
			}
			m_isShowing = false;
			OnPickupPanelStateChanged?.Invoke(arg0: false);
		}
	}

	private void OnGameMenuChanged(GameMenuState.GameMenu menu)
	{
		if (menu.HasFlag(GameMenuState.GameMenu.ItemPickup))
		{
			m_onNewItemStinger?.Play2D();
			Show();
		}
		else
		{
			Hide();
		}
	}

	private string GetItemNameString(IItemPickup itemPickup)
	{
		return GetItemNameString(itemPickup.ItemDefinition, itemPickup.ItemAmount);
	}

	private string GetItemNameString(ItemInstance itemInstance)
	{
		return GetItemNameString(itemInstance.ItemDefinition, itemInstance.StackSize);
	}

	private string GetItemNameString(ItemDefinition itemDefinition, int stackCount)
	{
		string text = "<uppercase><b><color=#" + ColorUtility.ToHtmlStringRGB(m_itemHighlightColor) + ">" + itemDefinition.ItemName;
		if (itemDefinition.MaxStackSize > 1 || itemDefinition is AmmunitionItemDefinition)
		{
			text = text + " x" + stackCount;
		}
		return text + "</color></b></uppercase>";
	}

	private float GetImageMarginSizeDeltaForItemSize(Vector2Int size)
	{
		if (size.x >= m_itemCellWidthForMaxSize)
		{
			return m_itemImageMaxWidthMargin;
		}
		return Mathf.Lerp(m_itemImageMinWidthMargin, m_itemImageMaxWidthMargin, (float)(size.x - 1) / (float)(m_itemCellWidthForMaxSize - 1));
	}

	private void PopulateForItemPickup(IItemPickup itemPickup)
	{
		if (itemPickup == null)
		{
			return;
		}
		m_takeButtonEnabled = true;
		Color color = m_normalTextColor;
		m_infoPanel.SetActiveItem((ItemDefinition)null);
		Vector2 sizeDelta = m_itemImage.rectTransform.sizeDelta;
		sizeDelta.x = m_itemImageMaxWidthMargin;
		bool flag = false;
		Vector2Int vector2Int = Vector2Int.one;
		m_canQuickUse = false;
		bool active = false;
		if (itemPickup.LootType == LootType.ItemDefinition)
		{
			ItemDefinition itemDefinition = itemPickup.ItemDefinition;
			m_infoPanel.SetActiveItem(itemDefinition);
			m_canQuickUse = itemDefinition.CanUse(GlobalReferences.Instance.MainInventory, checkConditions: true) && itemDefinition.IsDroppable && !itemDefinition.IsImportantItem && !itemDefinition.HideUseButton();
			bool flag2 = (m_takeButtonEnabled = m_inventory.CanAddItemToInventory(itemDefinition, itemPickup.ItemAmount));
			m_itemImage.sprite = itemDefinition.InventorySprite;
			sizeDelta.x = GetImageMarginSizeDeltaForItemSize(itemDefinition.ItemSize);
			vector2Int = itemDefinition.ItemSize;
			if (flag2)
			{
				if (itemDefinition is ArtifactItemDefinition)
				{
					m_pickupText.text = m_artifactStringReference.GetLocalizedString(GetItemNameString(itemPickup));
					color = m_artifactTextColor;
					active = true;
				}
				else
				{
					m_pickupText.text = m_pickupStringReference.GetLocalizedString(GetItemNameString(itemPickup));
				}
			}
			else
			{
				string text = "(" + itemDefinition.ItemSize.x + "x" + itemDefinition.ItemSize.y + ")";
				m_pickupText.text = m_notEnoughSpaceTextString.GetLocalizedString(GetItemNameString(itemPickup), text);
				color = m_insufficientSpaceTextColor;
				flag = true;
			}
		}
		else if (itemPickup.LootType == LootType.Money)
		{
			m_itemImage.sprite = m_moneySprite;
			m_pickupText.text = m_takeMoneyStringReference.GetLocalizedString("<b><color=#" + ColorUtility.ToHtmlStringRGB(m_itemHighlightColor) + ">$" + itemPickup.ItemAmount + "</color></b>");
		}
		m_itemImage.rectTransform.sizeDelta = sizeDelta;
		m_itemImage.gameObject.SetActive(value: true);
		if (flag)
		{
			RectTransform component = m_grid.GetComponent<RectTransform>();
			Vector2 size = m_itemImage.rectTransform.rect.size;
			size.x += 64f;
			size.y += 64f;
			if (vector2Int.x > vector2Int.y)
			{
				m_grid.settings.gridSize = size.x / (float)vector2Int.x;
				size.y = m_grid.settings.gridSize * (float)vector2Int.y;
			}
			else
			{
				m_grid.settings.gridSize = size.y / (float)vector2Int.y;
				size.x = m_grid.settings.gridSize * (float)vector2Int.x;
			}
			component.sizeDelta = size;
			Vector2 fillOffset = new Vector2((vector2Int.x % 2 == 0) ? 0f : (m_grid.settings.gridSize * 0.5f), (vector2Int.y % 2 == 0) ? 0f : (m_grid.settings.gridSize * 0.5f));
			m_grid.settings.fillOffset = fillOffset;
			m_grid.gameObject.SetActive(value: true);
		}
		else
		{
			m_grid.gameObject.SetActive(value: false);
		}
		m_waitingForAnimation = false;
		m_pickupText.color = color;
		m_artifactIcon.SetActive(active);
		m_takeButtonPrompt.SetActive(m_takeButtonEnabled);
		m_quickUseButtonPrompt.SetActive(m_canQuickUse);
		LayoutRebuilder.ForceRebuildLayoutImmediate(m_buttonPromptParentTransform);
		LayoutRebuilder.MarkLayoutForRebuild(m_buttonPromptParentTransform);
		StartCoroutine(EnableInputDelay(m_takeButtonEnabled, m_canQuickUse));
	}

	private void PopulateForItemInstance(ItemInstance itemInstance, InventoryItemMode mode)
	{
		if (itemInstance != null)
		{
			m_takeButtonEnabled = true;
			m_canQuickUse = false;
			Color normalTextColor = m_normalTextColor;
			m_infoPanel.SetActiveItem((ItemDefinition)null);
			Vector2 sizeDelta = m_itemImage.rectTransform.sizeDelta;
			sizeDelta.x = m_itemImageMaxWidthMargin;
			ItemDefinition itemDefinition = itemInstance.ItemDefinition;
			m_infoPanel.SetActiveItem(itemDefinition);
			m_itemImage.sprite = itemDefinition.InventorySprite;
			sizeDelta.x = GetImageMarginSizeDeltaForItemSize(itemDefinition.ItemSize);
			bool active = true;
			if (mode == InventoryItemMode.Combined)
			{
				m_pickupText.text = m_pickupStringReference.GetLocalizedString(GetItemNameString(itemInstance));
			}
			else
			{
				m_pickupText.text = GetItemNameString(itemInstance);
			}
			m_waitingForAnimation = false;
			if (mode == InventoryItemMode.Combined && itemInstance.ItemDefinition.CombinedNewItemAnimationElement != null)
			{
				m_animatedElement = UnityEngine.Object.Instantiate(itemInstance.ItemDefinition.CombinedNewItemAnimationElement, m_animatedElementParent);
				active = false;
				m_waitingForAnimation = true;
			}
			m_itemImage.gameObject.SetActive(active);
			m_itemImage.rectTransform.sizeDelta = sizeDelta;
			m_artifactIcon.SetActive(value: false);
			m_pickupText.color = normalTextColor;
			m_grid.gameObject.SetActive(value: false);
			m_takeButtonPrompt.SetActive(m_takeButtonEnabled);
			m_quickUseButtonPrompt.SetActive(value: false);
			LayoutRebuilder.ForceRebuildLayoutImmediate(m_buttonPromptParentTransform);
			LayoutRebuilder.MarkLayoutForRebuild(m_buttonPromptParentTransform);
			StartCoroutine(EnableInputDelay(m_takeButtonEnabled, canQuickUse: false));
		}
	}

	private IEnumerator EnableInputDelay(bool hasSpace, bool canQuickUse)
	{
		m_buttonPromptsContainer.SetActive(value: false);
		yield return new WaitForSecondsRealtime(0.1f);
		yield return new WaitUntil(() => !m_waitingForAnimation);
		m_buttonPromptsContainer.SetActive(value: true);
		if (hasSpace)
		{
			GameInputManager.GameInputActions.UI.TakeItem.performed += TakeItemInput;
		}
		if (canQuickUse)
		{
			GameInputManager.GameInputActions.UI.UseItem.performed += OnQuickuseItemInput;
		}
		GameInputManager.GameInputActions.UI.Cancel.performed += Cancelinput;
	}

	private void OnQuickuseItemInput(InputAction.CallbackContext context)
	{
		if (m_canQuickUse)
		{
			if (m_mode == Mode.GameItemPickup)
			{
				QuickUseItem();
			}
			else
			{
				Cancel();
			}
		}
	}

	private void TakeItemInput(InputAction.CallbackContext obj)
	{
		if (m_takeButtonEnabled)
		{
			if (m_mode == Mode.GameItemPickup && m_activeItemPickupAnchor.Item != null)
			{
				TakeItem();
			}
			else
			{
				Cancel();
			}
		}
	}

	private void Cancelinput(InputAction.CallbackContext obj)
	{
		Cancel();
	}

	public void TakeItem()
	{
		if (!m_closing && m_activeItemPickupAnchor.Item.ItemAmount != 0)
		{
			m_closing = true;
			if (m_activeItemPickupAnchor.Item.LootType == LootType.ItemDefinition)
			{
				m_inventory.PickupItem(m_activeItemPickupAnchor.Item);
			}
			else
			{
				GlobalReferences.Instance.EventChannels.Inventory.AddPlayerMoney.Raise(m_activeItemPickupAnchor.Item.ItemAmount);
				m_activeItemPickupAnchor.Item.ItemAmount = 0;
			}
			StartCoroutine(HideCoroutine());
			m_activeItemPickupAnchor.Set(null);
		}
	}

	public void QuickUseItem()
	{
		if (!m_closing && m_activeItemPickupAnchor.Item.ItemAmount != 0)
		{
			m_closing = true;
			m_inventory.QuickUseItem(m_activeItemPickupAnchor.Item, useItemFSMState: true);
			StartCoroutine(HideCoroutine());
			m_activeItemPickupAnchor.Set(null);
		}
	}

	public bool OnInputBack()
	{
		Cancel();
		return true;
	}

	public void Cancel()
	{
		if (!m_closing)
		{
			m_closing = true;
			StartCoroutine(HideCoroutine());
			m_activeItemPickupAnchor.Set(null);
		}
	}

	private IEnumerator HideCoroutine()
	{
		yield return new WaitForSecondsRealtime(0.2f);
		if (m_mode == Mode.GameItemPickup)
		{
			GlobalReferences.Instance.GameMenuState.ClearInMenu(GameMenuState.GameMenu.ItemPickup);
		}
		else
		{
			Hide();
		}
	}
}
