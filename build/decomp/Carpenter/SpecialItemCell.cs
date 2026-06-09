using UnityEngine;

public class SpecialItemCell : MonoBehaviour
{
	[SerializeField]
	private ItemDefinition m_itemDefinition;

	[SerializeField]
	private InventoryItemCell m_itemCell;

	[SerializeField]
	private Transform m_layoutParent;

	public ItemDefinition ItemDefinition => m_itemDefinition;

	public InventoryItemCell ItemCell => m_itemCell;

	public void SetItem(ItemInstance item)
	{
		if (item != null)
		{
			m_itemCell.SetItem(item, modifySize: false, overrideRotation: true);
			base.gameObject.SetActive(value: true);
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
	}

	public bool IsHovered(Vector2 cursorScreenPoint)
	{
		return RectTransformUtility.RectangleContainsScreenPoint(GetComponent<RectTransform>(), cursorScreenPoint);
	}
}
