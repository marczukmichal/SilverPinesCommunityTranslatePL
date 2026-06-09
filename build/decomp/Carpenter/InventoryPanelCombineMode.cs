using UnityEngine;
using UnityEngine.Localization;

public class InventoryPanelCombineMode : MonoBehaviour
{
	[Header("UI")]
	[SerializeField]
	private InventoryPanel m_mainInventoryPanel;

	[SerializeField]
	private InventoryPanel m_stashInventoryPanel;

	[Header("Error Strings")]
	[SerializeField]
	private LocalizedString m_stringCombineFailedNoSpace;

	[Header("Audio")]
	[SerializeField]
	private AudioEvent m_combineAudioEvent;

	[SerializeField]
	private AudioEvent m_combineErrorAudioEvent;

	private ItemInstance m_activeItem;

	public bool IsCombineModeActive()
	{
		return m_activeItem != null;
	}

	public void SetActive(ItemInstance item, InventoryPanel requestedPanel)
	{
		m_activeItem = item;
		m_mainInventoryPanel.ShowValidCombineTargetsForItem(item);
		if (m_stashInventoryPanel.isActiveAndEnabled)
		{
			m_stashInventoryPanel.ShowValidCombineTargetsForItem(item);
		}
	}

	public void Deactivate()
	{
		m_activeItem = null;
		m_mainInventoryPanel.ClearCombineTargets();
		if (m_stashInventoryPanel.isActiveAndEnabled)
		{
			m_stashInventoryPanel.ClearCombineTargets();
		}
	}

	public void TryCombine(ItemInstance item, Inventory targetInventory)
	{
		if (item == null || item == GlobalReferences.Instance.Anchors.Inventory.QueuedUseItemInstanceAnchor.Item || !targetInventory.CanItemBeCombined(m_activeItem, item))
		{
			return;
		}
		Inventory inventory = null;
		if (CombineItem(sourceItemParentInventory: (!m_mainInventoryPanel.TrackedInventory.HasItem(m_activeItem)) ? m_stashInventoryPanel.TrackedInventory : m_mainInventoryPanel.TrackedInventory, source: m_activeItem, target: item, targetInventory: targetInventory))
		{
			if (m_stashInventoryPanel.isActiveAndEnabled)
			{
				m_stashInventoryPanel.RefreshData();
				m_stashInventoryPanel.TryClearModeAndSelection();
			}
			m_mainInventoryPanel.RefreshData();
			m_mainInventoryPanel.TryClearModeAndSelection();
		}
	}

	public bool CombineItem(ItemInstance source, ItemInstance target, Inventory targetInventory, Inventory sourceItemParentInventory)
	{
		if (targetInventory != null)
		{
			switch (targetInventory.TryCombineItems(source, target, sourceItemParentInventory))
			{
			case Inventory.CombineItemResult.FailedNotEnoughSpace:
				GlobalReferences.Instance.EventChannels.InGameMenu.MenuInfoMessage.Raise(new MenuInfoMessageData
				{
					m_stringReference = m_stringCombineFailedNoSpace
				});
				if (m_combineErrorAudioEvent != null)
				{
					m_combineErrorAudioEvent.Play2D();
				}
				return false;
			default:
				if (m_combineAudioEvent != null)
				{
					m_combineAudioEvent.Play2D();
				}
				return true;
			case Inventory.CombineItemResult.None:
				break;
			}
		}
		return false;
	}
}
