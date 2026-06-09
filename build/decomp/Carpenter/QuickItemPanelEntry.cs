using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuickItemPanelEntry : MonoBehaviour
{
	[SerializeField]
	private Image m_image;

	[SerializeField]
	private CanvasGroup m_promptCanvasGroup;

	[SerializeField]
	private TextMeshProUGUI m_itemNameText;

	[SerializeField]
	private BatteryChargeCounter m_battery;

	[SerializeField]
	private Vector2 m_poweredItemOffset;

	[SerializeField]
	private Color m_usableColor;

	[SerializeField]
	private Color m_unusableColor;

	private bool m_shortcutActive;

	private ItemDefinition m_itemDefinition;

	public bool ShortcutActive => m_shortcutActive;

	public ItemDefinition ItemDefinition => m_itemDefinition;

	public void Populate(ItemDefinition itemDefinition, ItemInstance itemInstance, int count, bool isUsable)
	{
		m_itemDefinition = itemDefinition;
		m_shortcutActive = isUsable;
		m_image.sprite = itemInstance.ItemSprite;
		m_image.color = (isUsable ? m_usableColor : m_unusableColor);
		m_promptCanvasGroup.alpha = (isUsable ? 1f : 0.4f);
		bool flag = true;
		if (count <= 1 && itemDefinition.IsImportantItem && !(itemInstance is RefillableItemInstance))
		{
			flag = false;
		}
		string text = itemDefinition.ItemName;
		if (flag)
		{
			text = text + " <b>(x" + count + ")</b>";
		}
		m_itemNameText.text = text;
		if (itemInstance is PoweredItemInstance poweredInstance)
		{
			m_battery.gameObject.SetActive(value: true);
			m_battery.PopulateFromPoweredItemInstance(poweredInstance);
			m_image.rectTransform.anchoredPosition = m_poweredItemOffset;
		}
		else
		{
			m_battery.gameObject.SetActive(value: false);
			m_image.rectTransform.anchoredPosition = Vector2.zero;
		}
	}
}
