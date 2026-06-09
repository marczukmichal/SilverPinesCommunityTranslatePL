using System;
using UnityEngine;

[Serializable]
public class RefillableItemInstance : ItemInstance
{
	[SerializeField]
	private int m_useCount;

	public RefillableItemDefinition RefillableItemDefinition => base.ItemDefinition as RefillableItemDefinition;

	public int Uses
	{
		get
		{
			return m_useCount;
		}
		set
		{
			if (m_useCount != value)
			{
				m_useCount = Mathf.Clamp(value, 0, RefillableItemDefinition.MaxUses);
			}
		}
	}

	public RefillableItemInstance(RefillableItemDefinition definition, int stackSize, Vector2Int position, bool rotated)
		: base(definition, 1, position, rotated)
	{
		m_useCount = stackSize;
	}

	public override bool CanUse(Inventory inventory, bool checkConditions)
	{
		if (RefillableItemDefinition.RequiresUsesToUse && Uses == 0)
		{
			return false;
		}
		return base.CanUse(inventory, checkConditions);
	}

	public bool CanRefillFromItem(ItemInstance item)
	{
		if (RefillableItemDefinition.RefillItemDefinition == item.ItemDefinition)
		{
			return true;
		}
		return false;
	}
}
