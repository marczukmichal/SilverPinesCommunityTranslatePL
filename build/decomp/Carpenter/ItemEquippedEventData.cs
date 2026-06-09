public class ItemEquippedEventData
{
	public ItemInstance m_itemInstance;

	public bool m_equipped;

	public ItemEquippedEventData(ItemInstance item, bool equipped)
	{
		m_itemInstance = item;
		m_equipped = equipped;
	}
}
