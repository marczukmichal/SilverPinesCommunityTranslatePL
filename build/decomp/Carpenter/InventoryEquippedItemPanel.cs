using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class InventoryEquippedItemPanel : MonoBehaviour
{
	[SerializeField]
	private InventoryItemCell m_equippedRangedItem;

	[SerializeField]
	private InventoryItemCell m_equippedMeleeItem;

	[SerializeField]
	private PlayerMainInventory m_inventory;

	[SerializeField]
	private TextMeshProUGUI m_rangedItemLabel;

	[SerializeField]
	private TextMeshProUGUI m_meleeItemLabel;

	[SerializeField]
	private InventoryItemCell[] m_equippedArtifacts;

	[SerializeField]
	private GameObject m_artifactLabel;

	[SerializeField]
	private LocalizedString m_noneStringReference;

	[SerializeField]
	private InventoryPanel m_inventoryPanel;

	[SerializeField]
	private RectTransform m_equippedRangedImageRectTransform;

	[SerializeField]
	private RectTransform m_equippedMeleeImageRectTransform;

	[SerializeField]
	private int m_itemCellWidthForMaxSize = 6;

	[SerializeField]
	private float m_itemImageMaxWidthMargin = -20f;

	[SerializeField]
	private float m_itemImageMinWidthMargin = -650f;

	private void SetEquippedItemRanged(ItemEquippedEventData equippedEvent)
	{
		SetEquippedItemRanged(equippedEvent.m_equipped ? equippedEvent.m_itemInstance : null);
	}

	private void SetEquippedItemRanged(ItemInstance item)
	{
		m_equippedRangedItem.SetItem(item, modifySize: false);
		UpdateSize(item, m_equippedRangedImageRectTransform);
		if (item == null)
		{
			m_rangedItemLabel.text = m_noneStringReference.GetLocalizedString();
		}
		else
		{
			m_rangedItemLabel.text = item.ItemDefinition.ItemName;
		}
	}

	private void SetEquippedItemMelee(ItemEquippedEventData equippedEvent)
	{
		SetEquippedItemMelee(equippedEvent.m_equipped ? equippedEvent.m_itemInstance : null);
	}

	private void SetEquippedItemMelee(ItemInstance item)
	{
		m_equippedMeleeItem.SetItem(item, modifySize: false);
		UpdateSize(item, m_equippedMeleeImageRectTransform);
		if (item == null)
		{
			m_meleeItemLabel.text = m_noneStringReference.GetLocalizedString();
		}
		else
		{
			m_meleeItemLabel.text = item.ItemDefinition.ItemName;
		}
	}

	private float GetImageMarginSizeDeltaForItemSize(Vector2Int size)
	{
		if (size.x >= m_itemCellWidthForMaxSize)
		{
			return m_itemImageMaxWidthMargin;
		}
		return Mathf.Lerp(m_itemImageMinWidthMargin, m_itemImageMaxWidthMargin, (float)(size.x - 1) / (float)(m_itemCellWidthForMaxSize - 1));
	}

	private void UpdateSize(ItemInstance itemInstance, RectTransform rectTransform)
	{
		if (itemInstance != null)
		{
			Vector2 sizeDelta = rectTransform.sizeDelta;
			sizeDelta.x = GetImageMarginSizeDeltaForItemSize(itemInstance.ItemDefinition.ItemSize);
			rectTransform.sizeDelta = sizeDelta;
		}
	}

	private void SetEquippedArtifact(ItemEquippedEventData equippedEvent)
	{
		RefreshArtifacts();
	}

	private void RefreshArtifacts()
	{
		m_artifactLabel.SetActive(m_inventory.MaxEquippedArtifactsCount > 0);
		IReadOnlyCollection<ItemInstance> equippedArtifacts = m_inventory.EquippedArtifacts;
		int i = 0;
		foreach (ItemInstance item in equippedArtifacts)
		{
			m_equippedArtifacts[i].SetItem(item, modifySize: false);
			m_equippedArtifacts[i].gameObject.SetActive(value: true);
			i++;
		}
		for (; i < m_equippedArtifacts.Length; i++)
		{
			m_equippedArtifacts[i].SetItem(null, modifySize: false);
			m_equippedArtifacts[i].gameObject.SetActive(i < m_inventory.MaxEquippedArtifactsCount);
		}
	}

	private void Populate()
	{
		ItemInstance itemInstance = m_inventory.EquippedRangedItem;
		if (itemInstance == null)
		{
			itemInstance = m_inventory.EquippedSecondaryItem;
		}
		SetEquippedItemRanged(itemInstance);
		SetEquippedItemMelee(m_inventory.EquippedMeleeItem);
		RefreshArtifacts();
	}

	private void LocaleChanged(UnityEngine.Localization.Locale obj)
	{
		Populate();
	}

	private void RepairedMeleeWeapon(ItemInstance itemInstance)
	{
		SetEquippedItemMelee(m_inventory.EquippedMeleeItem);
	}

	private void OnEnable()
	{
		Populate();
		GlobalReferences.Instance.EventChannels.Inventory.EquippedRangedItemChanged.Register(SetEquippedItemRanged);
		GlobalReferences.Instance.EventChannels.Inventory.EquippedMeleeItemChanged.Register(SetEquippedItemMelee);
		GlobalReferences.Instance.EventChannels.Inventory.EquippedArtifactsChanged.Register(SetEquippedArtifact);
		GlobalReferences.Instance.EventChannels.Inventory.MeleeWeaponItemRepaired.Register(RepairedMeleeWeapon);
		LocalizationSettings.SelectedLocaleChanged += LocaleChanged;
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.Inventory.EquippedRangedItemChanged.Unregister(SetEquippedItemRanged);
		GlobalReferences.Instance.EventChannels.Inventory.EquippedMeleeItemChanged.Unregister(SetEquippedItemMelee);
		GlobalReferences.Instance.EventChannels.Inventory.EquippedArtifactsChanged.Unregister(SetEquippedArtifact);
		GlobalReferences.Instance.EventChannels.Inventory.MeleeWeaponItemRepaired.Unregister(RepairedMeleeWeapon);
		LocalizationSettings.SelectedLocaleChanged -= LocaleChanged;
	}

	public void OnEquippedItemDragged(InventoryItemCell itemCell)
	{
		if (m_inventoryPanel.ActiveMode == InventoryPanel.Mode.Normal)
		{
			m_inventoryPanel.UnequipItemIntoMoveMode(itemCell.ItemInstance);
		}
	}
}
