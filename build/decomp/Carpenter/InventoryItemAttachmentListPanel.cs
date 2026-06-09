using System.Collections.Generic;
using UnityEngine;

public class InventoryItemAttachmentListPanel : MonoBehaviour
{
	[SerializeField]
	private List<InventoryItemCell> m_itemCells;

	private ItemInstance m_itemInstance;

	private int m_shownAttachmentCount;

	private void Start()
	{
		if (m_shownAttachmentCount == 0)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	public void ClearActiveItem()
	{
		SetActiveItemInstance(null);
	}

	public void SetActiveItemInstance(ItemInstance itemInstance)
	{
		int num = 0;
		if (itemInstance is ProjectileWeaponItemInstance projectileWeaponItemInstance)
		{
			num = projectileWeaponItemInstance.AttachedUpgrades.Count;
		}
		if (m_itemInstance == itemInstance && num == m_shownAttachmentCount)
		{
			return;
		}
		bool active = false;
		m_itemInstance = itemInstance;
		if (m_itemInstance == null || !(itemInstance is ProjectileWeaponItemInstance))
		{
			foreach (InventoryItemCell itemCell in m_itemCells)
			{
				itemCell.gameObject.SetActive(value: false);
			}
			m_shownAttachmentCount = 0;
		}
		else
		{
			IReadOnlyCollection<ItemInstance> attachedUpgrades = (m_itemInstance as ProjectileWeaponItemInstance).AttachedUpgrades;
			int i = 0;
			foreach (ItemInstance item in attachedUpgrades)
			{
				if (i >= m_itemCells.Count)
				{
					Debug.LogError("Too many item attachments for item " + itemInstance.ToString() + " has " + attachedUpgrades.Count + " attachments but max is " + m_itemCells.Count);
					break;
				}
				m_itemCells[i].SetItem(item, modifySize: false);
				m_itemCells[i].gameObject.SetActive(value: true);
				i++;
			}
			for (; i < m_itemCells.Count; i++)
			{
				m_itemCells[i].gameObject.SetActive(value: false);
			}
			active = attachedUpgrades.Count > 0;
			m_shownAttachmentCount = attachedUpgrades.Count;
		}
		base.gameObject.SetActive(active);
	}
}
