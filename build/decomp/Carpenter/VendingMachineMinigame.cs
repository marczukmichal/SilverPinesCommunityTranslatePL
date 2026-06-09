using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;

public class VendingMachineMinigame : MonoBehaviour, IMinigameComponent
{
	private enum TextMode
	{
		Normal,
		InsufficientCredits,
		InvalidEntry,
		SuccesfulPurchase
	}

	[SerializeField]
	private ItemDefinition m_coinItemDefinition;

	[SerializeField]
	private KeypadMinigame m_keypad;

	[Header("Audio")]
	[SerializeField]
	private AudioEvent m_coinAddAudioEvent;

	[SerializeField]
	private AudioEvent m_coinReturnAudioEvent;

	[SerializeField]
	private AudioEvent m_purchaseItemEvent;

	[Header("LCD Display")]
	[SerializeField]
	private TextMeshProUGUI m_lcdTextDisplay;

	[SerializeField]
	private int m_lcdMaxCharacters = 7;

	[SerializeField]
	private float m_animTime = 0.3f;

	[SerializeField]
	private LocalizedString m_idleDisplayString;

	[SerializeField]
	private LocalizedString m_insufficientCreditString;

	[SerializeField]
	private LocalizedString m_invalidEntryString;

	[SerializeField]
	private LocalizedString m_succesfulPurchaseString;

	[SerializeField]
	private InventoryItemInfoPanel m_itemInfoPanel;

	[SerializeField]
	private UnityEvent m_onAddCoin;

	private Dictionary<string, VendingMachineItemSlot> m_slotDictionary = new Dictionary<string, VendingMachineItemSlot>();

	private VendingMachineMinigameData m_data;

	private TextMode m_lcdDisplayTextMode;

	private int m_lcdTextLoopIndex;

	private int m_insertedCoinCount;

	private string m_currentCodeInput;

	private VendingMachineItemSlot m_selectedSlot;

	private VendingMachineItemSlot m_hoveredSlot;

	private VendingMachineItemSlot m_previouslySelectedSlot;

	private Coroutine m_activeCoroutine;

	private string m_lastPurchasedString = "";

	private bool m_animatingOut;

	public string StringToShow
	{
		get
		{
			switch (m_lcdDisplayTextMode)
			{
			case TextMode.InsufficientCredits:
				return m_insufficientCreditString.GetLocalizedString() + " ";
			case TextMode.InvalidEntry:
				return m_invalidEntryString.GetLocalizedString() + " ";
			case TextMode.SuccesfulPurchase:
				return m_succesfulPurchaseString.GetLocalizedString(m_lastPurchasedString) + " ";
			default:
				if (m_insertedCoinCount > 0 || !string.IsNullOrEmpty(m_currentCodeInput))
				{
					string text = "    $" + m_insertedCoinCount + " ";
					if (!string.IsNullOrEmpty(m_currentCodeInput))
					{
						text = ((m_insertedCoinCount < 10) ? (text + "    " + m_currentCodeInput) : (text + "   " + m_currentCodeInput));
					}
					return text;
				}
				return m_idleDisplayString.GetLocalizedString() + " ";
			}
		}
	}

	private void OnEnable()
	{
		UpdateCoinCount();
		KeypadMinigame keypad = m_keypad;
		keypad.m_codeUpdated = (UnityAction<string>)Delegate.Combine(keypad.m_codeUpdated, new UnityAction<string>(OnCodeUpdated));
		GlobalReferences.Instance.EventChannels.Inventory.QuickUseOnPickupItem.Register(OnQuickItemUse);
	}

	private void OnDisable()
	{
		ReturnCoin();
		KeypadMinigame keypad = m_keypad;
		keypad.m_codeUpdated = (UnityAction<string>)Delegate.Remove(keypad.m_codeUpdated, new UnityAction<string>(OnCodeUpdated));
		GlobalReferences.Instance.EventChannels.Inventory.QuickUseOnPickupItem.Unregister(OnQuickItemUse);
	}

	private void OnCodeUpdated(string code)
	{
		if (m_animatingOut)
		{
			return;
		}
		m_currentCodeInput = code;
		VendingMachineItemSlot vendingMachineItemSlot = null;
		if (m_slotDictionary.ContainsKey(code))
		{
			VendingMachineItemSlot vendingMachineItemSlot2 = m_slotDictionary[code];
			if (vendingMachineItemSlot2.HasItem())
			{
				vendingMachineItemSlot = vendingMachineItemSlot2;
			}
		}
		if (vendingMachineItemSlot != m_selectedSlot)
		{
			if (m_selectedSlot != null)
			{
				m_selectedSlot.SetHighlighted(highlight: false);
			}
			m_selectedSlot = vendingMachineItemSlot;
			if (m_selectedSlot != null)
			{
				m_selectedSlot.SetHighlighted(highlight: true);
			}
			UpdateItemInfoPanel();
		}
		StopFlash();
	}

	private void UpdateItemInfoPanel()
	{
		if (m_selectedSlot != null)
		{
			m_itemInfoPanel.SetActiveItem(m_selectedSlot.ItemDefinition);
			m_itemInfoPanel.SetCountValue(m_selectedSlot.VendingItemDefinition.ItemDefinition.ShopStackSize);
		}
		else if (m_hoveredSlot != null && m_hoveredSlot.ItemDefinition != null && !m_hoveredSlot.SoldOut)
		{
			m_itemInfoPanel.SetActiveItem(m_hoveredSlot.ItemDefinition);
			m_itemInfoPanel.SetCountValue(m_hoveredSlot.VendingItemDefinition.ItemDefinition.ShopStackSize);
		}
		else
		{
			m_itemInfoPanel.SetActiveItem((ItemDefinition)null);
		}
	}

	private void UpdateCoinCount()
	{
		m_lcdTextLoopIndex = 0;
		StopFlash();
		UpdateLCDText();
	}

	public void Setup(GameObject parent)
	{
		m_data = parent.GetComponent<VendingMachineMinigameData>();
		if (m_data != null)
		{
			foreach (VendingMachineMinigameData.VendingMachineItemDefinition availableItem in m_data.GetAvailableItems())
			{
				if (m_slotDictionary.ContainsKey(availableItem.m_slotID))
				{
					m_slotDictionary[availableItem.m_slotID].Configure(availableItem, m_data.GetSlotInstanceCount(availableItem.m_slotID));
				}
				else
				{
					Debug.LogError("Item for vending machine wants to live in slot " + availableItem.m_slotID + " but slot doesn't exist!");
				}
			}
			return;
		}
		Debug.LogError("Went into VendingMachineMinigame scene but spawning object (" + parent.name + ") has no VendingMachineMinigameData component!", parent);
	}

	public void TryAddCoin()
	{
		if (m_insertedCoinCount < 99 && !m_animatingOut && GlobalReferences.Instance.MainInventory.Money > 0)
		{
			m_coinAddAudioEvent.Play2D();
			GlobalReferences.Instance.MainInventory.RemoveMoney(1);
			m_insertedCoinCount++;
			m_onAddCoin.Invoke();
			UpdateCoinCount();
		}
	}

	public void ReturnCoin()
	{
		if (m_insertedCoinCount > 0)
		{
			m_coinReturnAudioEvent.Play2D();
			GlobalReferences.Instance.MainInventory.AddMoney(m_insertedCoinCount);
			m_insertedCoinCount = 0;
			UpdateCoinCount();
		}
	}

	private bool HasSpaceInInventory(PlayerMainInventory inventory, VendingMachineItemSlot slot)
	{
		return inventory.CanAddItemToInventory(slot.ItemDefinition, slot.VendingItemDefinition.ItemDefinition.ShopStackSize);
	}

	private void AddItemToInventory(PlayerMainInventory inventory, VendingMachineItemSlot slot)
	{
		AudioEvent onAddedToInventory = slot.ItemDefinition.Audio.OnAddedToInventory;
		if (onAddedToInventory != null)
		{
			AudioEvent.Play2D(onAddedToInventory);
		}
		AddItemEventData value = new AddItemEventData
		{
			m_itemDefinition = slot.ItemDefinition,
			m_itemAmount = slot.VendingItemDefinition.ItemDefinition.ShopStackSize
		};
		GlobalReferences.Instance.EventChannels.Inventory.AddItem.Raise(value);
		GlobalReferences.Instance.EventChannels.Inventory.ItemPurchased.Raise(value);
	}

	public void TryBuy()
	{
		if (m_animatingOut)
		{
			return;
		}
		if (m_selectedSlot != null && !m_selectedSlot.SoldOut)
		{
			if (m_selectedSlot.Cost > m_insertedCoinCount)
			{
				StopFlash();
				m_activeCoroutine = StartCoroutine(ShowInsufficientCreditText());
			}
			else
			{
				PlayerMainInventory mainInventory = GlobalReferences.Instance.MainInventory;
				if (HasSpaceInInventory(mainInventory, m_selectedSlot))
				{
					AddItemToInventory(mainInventory, m_selectedSlot);
					CompletePurchaseItemInSlot(m_selectedSlot);
				}
				else
				{
					m_previouslySelectedSlot = m_selectedSlot;
					GlobalReferences.Instance.Anchors.Inventory.ItemPickupInteractAnchor.Set(m_selectedSlot);
					GlobalReferences.Instance.GameMenuState.SetInMenu(GameMenuState.GameMenu.ItemPickup);
				}
			}
		}
		else
		{
			StopFlash();
			m_activeCoroutine = StartCoroutine(ShowInvalidItemText());
		}
		m_keypad.Clear();
	}

	private void CompletePurchaseItemInSlot(VendingMachineItemSlot slot)
	{
		GlobalReferences.Instance.DataStore.Data.m_stats.m_totalMoneySpent += slot.Cost;
		m_insertedCoinCount -= slot.Cost;
		slot.OnPurchase();
		m_data.SellItem(slot);
		StopFlash();
		m_lastPurchasedString = slot.ID;
		m_activeCoroutine = StartCoroutine(ShowThankYouText());
		m_purchaseItemEvent.Play2D();
	}

	private void StopFlash()
	{
		m_lcdDisplayTextMode = TextMode.Normal;
		if (m_activeCoroutine != null)
		{
			StopCoroutine(m_activeCoroutine);
		}
		m_lcdTextDisplay.gameObject.SetActive(value: true);
	}

	private IEnumerator ShowInvalidItemText()
	{
		m_lcdDisplayTextMode = TextMode.InvalidEntry;
		yield return FlashText(5);
		m_lcdDisplayTextMode = TextMode.Normal;
	}

	private IEnumerator ShowInsufficientCreditText()
	{
		m_lcdDisplayTextMode = TextMode.InsufficientCredits;
		yield return FlashText(7);
		m_lcdDisplayTextMode = TextMode.Normal;
	}

	private IEnumerator ShowThankYouText()
	{
		m_lcdDisplayTextMode = TextMode.SuccesfulPurchase;
		yield return FlashText(4);
		m_lcdDisplayTextMode = TextMode.Normal;
	}

	private IEnumerator FlashText(int flashCount)
	{
		m_lcdTextLoopIndex = 0;
		m_lcdTextDisplay.gameObject.SetActive(value: true);
		for (int i = 0; i < flashCount; i++)
		{
			m_lcdTextDisplay.gameObject.SetActive(value: false);
			yield return new WaitForSecondsRealtime(0.2f);
			m_lcdTextDisplay.gameObject.SetActive(value: true);
			yield return new WaitForSecondsRealtime(0.5f);
		}
		m_lcdTextDisplay.gameObject.SetActive(value: true);
	}

	private void Start()
	{
		VendingMachineItemSlot[] componentsInChildren = GetComponentsInChildren<VendingMachineItemSlot>();
		foreach (VendingMachineItemSlot vendingMachineItemSlot in componentsInChildren)
		{
			m_slotDictionary.Add(vendingMachineItemSlot.ID, vendingMachineItemSlot);
			vendingMachineItemSlot.OnHovered = (UnityAction<VendingMachineItemSlot, bool>)Delegate.Combine(vendingMachineItemSlot.OnHovered, new UnityAction<VendingMachineItemSlot, bool>(OnSlotHoveredChanged));
		}
		m_lcdTextLoopIndex = UnityEngine.Random.Range(0, StringToShow.Length);
		UpdateLCDText();
		StartCoroutine(LCDDisplayUpdateLoop());
	}

	private void OnSlotHoveredChanged(VendingMachineItemSlot slot, bool isHovered)
	{
		if (isHovered)
		{
			m_hoveredSlot = slot;
		}
		else if (slot == m_hoveredSlot)
		{
			m_hoveredSlot = null;
		}
		UpdateItemInfoPanel();
	}

	private IEnumerator LCDDisplayUpdateLoop()
	{
		while (true)
		{
			yield return new WaitForSecondsRealtime(m_animTime);
			bool flag = false;
			if (m_lcdTextLoopIndex >= StringToShow.Length)
			{
				m_lcdTextLoopIndex = 0;
				flag = true;
			}
			UpdateLCDText();
			if (!flag)
			{
				m_lcdTextLoopIndex++;
			}
			if (m_lcdTextLoopIndex >= StringToShow.Length)
			{
				m_lcdTextLoopIndex = 0;
			}
		}
	}

	private void UpdateLCDText()
	{
		if (StringToShow.Length < m_lcdMaxCharacters)
		{
			m_lcdTextDisplay.text = StringToShow;
		}
		else
		{
			m_lcdTextDisplay.text = GameUIUtility.WrapAroundSubstring(StringToShow, m_lcdTextLoopIndex, m_lcdMaxCharacters);
		}
	}

	private void OnQuickItemUse(ItemInstance itemInstance)
	{
		CompletePurchaseItemInSlot(m_previouslySelectedSlot);
		StartCoroutine(AnimatingOutQuickItemUse());
	}

	private IEnumerator AnimatingOutQuickItemUse()
	{
		m_animatingOut = true;
		yield return new WaitForSecondsRealtime(1f);
		GetComponentInParent<MinigameScene>().TryCloseMinigame();
	}
}
