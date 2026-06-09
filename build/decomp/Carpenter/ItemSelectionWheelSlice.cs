using Shapes2D;
using UnityEngine;
using UnityEngine.UI;

public class ItemSelectionWheelSlice : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private Shape m_shape;

	[SerializeField]
	private Shape m_equippedShape;

	[SerializeField]
	private InventoryItemCell m_itemCell;

	[SerializeField]
	private Image m_cancelIcon;

	[Header("Settings")]
	[SerializeField]
	private float m_iconOffset;

	[SerializeField]
	private Sprite m_noItemSprite;

	[Header("Color")]
	[SerializeField]
	private Color m_outlineNormalColor;

	[SerializeField]
	private Color m_outlineSelectedColor;

	[SerializeField]
	private Color m_fillNormalColor1;

	[SerializeField]
	private Color m_fillSelectedColor1;

	[SerializeField]
	private Color m_fillEquippedColor1;

	[SerializeField]
	private Color m_fillNormalColor2;

	[SerializeField]
	private Color m_fillSelectedColor2;

	[SerializeField]
	private Color m_fillEquippedColor2;

	[SerializeField]
	private Color m_iconNormalColor;

	[SerializeField]
	private Color m_iconSelectedColor;

	[Header("Item Types")]
	[SerializeField]
	private Image m_itemTypeIcon;

	[SerializeField]
	private Sprite m_projectileWeaponIcon;

	[SerializeField]
	private Sprite m_meleeWeaponIcon;

	[SerializeField]
	private Sprite m_secondaryIcon;

	[SerializeField]
	private Sprite m_activatableIcon;

	private bool m_equipped;

	private ItemInstance m_itemInstance;

	private bool m_hovered;

	private bool m_hasItemTypeIcon;

	public ItemInstance ItemInstance => m_itemInstance;

	public void SetItem(ItemInstance itemInstance, float sliceAngleSize, int index)
	{
		m_itemInstance = itemInstance;
		if (m_itemInstance != null)
		{
			m_itemCell.SetItem(itemInstance, modifySize: false);
		}
		m_itemCell.gameObject.SetActive(itemInstance != null);
		m_cancelIcon.gameObject.SetActive(itemInstance == null);
		float num3 = (m_equippedShape.settings.startAngle = (m_shape.settings.startAngle = 90f - sliceAngleSize * 0.5f + 1f));
		num3 = (m_equippedShape.settings.endAngle = (m_shape.settings.endAngle = 90f + sliceAngleSize * 0.5f - 1f));
		(base.transform as RectTransform).rotation = Quaternion.Euler(0f, 0f, (0f - sliceAngleSize) * (float)index);
		m_hasItemTypeIcon = false;
		if (itemInstance != null && itemInstance.CanBeActivated())
		{
			m_hasItemTypeIcon = true;
			m_itemTypeIcon.sprite = m_activatableIcon;
		}
		m_itemTypeIcon.gameObject.SetActive(value: false);
		m_itemCell.transform.rotation = Quaternion.identity;
		m_itemTypeIcon.transform.rotation = Quaternion.identity;
		UpdateDecorations();
	}

	public void SetSelected(bool hovered)
	{
		if (m_hovered != hovered)
		{
			m_hovered = hovered;
			UpdateDecorations();
		}
	}

	public void SetEquipped(bool isEquipped)
	{
		m_equipped = isEquipped;
		m_equippedShape.gameObject.SetActive(m_equipped);
		m_itemTypeIcon.gameObject.SetActive(m_equipped && m_hasItemTypeIcon);
		UpdateDecorations();
	}

	private void UpdateDecorations()
	{
		Color fillColor = m_fillNormalColor1;
		Color fillColor2 = m_fillNormalColor2;
		if (m_hovered)
		{
			fillColor = m_fillSelectedColor1;
			fillColor2 = m_fillSelectedColor2;
		}
		else if (m_equipped)
		{
			fillColor = m_fillEquippedColor1;
			fillColor2 = m_fillEquippedColor2;
		}
		m_shape.settings.fillColor = fillColor;
		m_shape.settings.fillColor2 = fillColor2;
		m_shape.settings.outlineColor = (m_hovered ? m_outlineSelectedColor : m_outlineNormalColor);
	}
}
