using UnityEngine;

public class AddItemEventData
{
	public ItemDefinition m_itemDefinition;

	public int m_itemAmount;

	public Vector2Int m_position;

	public bool m_rotation;

	public bool m_autoEquip;

	public int m_amountInItemOverride;

	public bool m_supressNotification;

	public AddItemEventData()
	{
		m_position = new Vector2Int(-1, -1);
		m_amountInItemOverride = -1;
	}
}
