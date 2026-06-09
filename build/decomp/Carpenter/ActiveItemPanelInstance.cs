using UnityEngine;
using UnityEngine.UI;

public class ActiveItemPanelInstance : MonoBehaviour
{
	[SerializeField]
	private BatteryChargeCounter m_batteryCounter;

	[SerializeField]
	private Image m_image;

	private ItemInstance m_item;

	private PoweredItemInstance m_poweredItem;

	public void SetItem(ItemInstance item)
	{
		m_item = item;
		m_poweredItem = item as PoweredItemInstance;
		m_image.sprite = item.ItemSprite;
	}

	private void Update()
	{
		m_batteryCounter.gameObject.SetActive(m_poweredItem != null);
		if (m_poweredItem != null)
		{
			m_batteryCounter.PopulateFromPoweredItemInstance(m_poweredItem);
		}
	}
}
