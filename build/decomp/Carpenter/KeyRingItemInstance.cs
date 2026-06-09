using System.Collections.Generic;
using UnityEngine;

public class KeyRingItemInstance : ItemInstance
{
	[SerializeField]
	private List<ItemInstance> m_containedItems = new List<ItemInstance>();

	public KeyRingItemInstance(ItemDefinition definition, int stackSize)
		: base(definition, stackSize)
	{
	}

	public KeyRingItemInstance(ItemDefinition definition, int stackSize, Vector2Int position, bool rotated)
		: base(definition, stackSize, position, rotated)
	{
	}

	public bool CanAddItem(ItemInstance itemInstance)
	{
		return itemInstance.ItemDefinition.IsKey;
	}

	public void AddItem(ItemInstance itemInstance)
	{
		if (m_containedItems.Contains(itemInstance))
		{
			Debug.LogWarning("Tried to add a key to a key ring that we already have added! " + itemInstance.ItemDefinition.ItemName);
		}
		else
		{
			m_containedItems.Add(itemInstance);
		}
	}

	public void RemoveItem(ItemInstance itemInstance)
	{
		m_containedItems.Remove(itemInstance);
	}

	public override void DoSpecialAction()
	{
		(base.ItemDefinition as KeyRingItemDefinition).SendShowKeyRingEventChannel(this);
	}

	public int GetKeyCount()
	{
		return m_containedItems.Count;
	}

	public ItemInstance GetKey(int index)
	{
		return m_containedItems[index];
	}
}
