using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[Serializable]
public class ItemInstance
{
	[HideInInspector]
	[SerializeField]
	private AssetReferenceT<ItemDefinition> m_itemDefinitionAssetReference;

	private ItemDefinition m_itemDefinition;

	[SerializeField]
	private Vector2Int m_itemGridPosition;

	[SerializeField]
	private bool m_itemRotated;

	[SerializeField]
	private int m_stackSize;

	[SerializeField]
	private bool m_activated;

	public ItemDefinition ItemDefinition
	{
		get
		{
			if (m_itemDefinition == null)
			{
				m_itemDefinition = AddressablesContentManager.Instance.GetAsset(m_itemDefinitionAssetReference);
			}
			return m_itemDefinition;
		}
	}

	public Vector2Int ItemGridPosition
	{
		get
		{
			return m_itemGridPosition;
		}
		set
		{
			m_itemGridPosition = value;
		}
	}

	public bool ItemRotated
	{
		get
		{
			return m_itemRotated;
		}
		set
		{
			m_itemRotated = value;
		}
	}

	public int StackSize
	{
		get
		{
			return m_stackSize;
		}
		set
		{
			if (m_stackSize != value)
			{
				m_stackSize = value;
				if (m_stackSize > ItemDefinition.MaxStackSize)
				{
					Debug.LogError("Tried to add too many items to an item stack for item type " + ItemDefinition.name + " - current value is " + value + " max is " + ItemDefinition.MaxStackSize);
					m_stackSize = ItemDefinition.MaxStackSize;
				}
			}
		}
	}

	public bool Activated
	{
		get
		{
			return m_activated;
		}
		set
		{
			m_activated = value;
			GlobalReferences.Instance.EventChannels.Inventory.ItemActivatedChanged.Raise(this);
		}
	}

	public virtual string ItemName => ItemDefinition.ItemName;

	public virtual string Description => ItemDefinition.Description;

	public virtual Sprite ItemSprite
	{
		get
		{
			if (Activated && ItemDefinition.Activatable.ActivatedSprite != null)
			{
				return ItemDefinition.Activatable.ActivatedSprite;
			}
			return ItemDefinition.InventorySprite;
		}
	}

	public bool IsValid()
	{
		return m_itemDefinitionAssetReference != null;
	}

	public void ChangeItemDefinition(ItemDefinition itemDefinition)
	{
		if (itemDefinition.GetType() != m_itemDefinition.GetType())
		{
			Debug.LogError("Tried to change to an item type that is of a different base class! This is not possible! Use replace item instead of changing item definition.");
			return;
		}
		m_itemDefinitionAssetReference = itemDefinition.AssetReference;
		m_itemDefinition = itemDefinition;
		GlobalReferences.Instance.EventChannels.Inventory.ItemInstanceDefinitionChanged.Raise(this);
	}

	public virtual int GetDifficultyResourceValue(GameDifficultyResourceScoreType type)
	{
		return m_itemDefinition.GetDifficultyResourceValue(type) * m_stackSize;
	}

	public ItemInstance(ItemDefinition definition, int stackSize, Vector2Int position, bool rotated)
	{
		if (definition == null)
		{
			Debug.LogError("Tried to create an item instance with no item definiton! This isn't going to work!");
		}
		m_itemDefinition = definition;
		m_itemDefinitionAssetReference = definition.AssetReference;
		m_stackSize = stackSize;
		m_itemGridPosition = position;
		m_itemRotated = rotated;
	}

	public ItemInstance(ItemDefinition definition, int stackSize)
		: this(definition, stackSize, new Vector2Int(-1, -1), rotated: false)
	{
	}

	public int GetEmptyCapacity()
	{
		return ItemDefinition.MaxStackSize - m_stackSize;
	}

	public bool OccupiesPosition(Vector2Int position)
	{
		foreach (Vector2Int occupiedPosition in GetOccupiedPositions())
		{
			if (position == occupiedPosition)
			{
				return true;
			}
		}
		return false;
	}

	public List<Vector2Int> GetOccupiedPositions()
	{
		return GetOccupiedPositions(ItemGridPosition, ItemRotated);
	}

	public List<Vector2Int> GetOccupiedPositions(Vector2Int itemPosition, bool rotated)
	{
		List<Vector2Int> list = new List<Vector2Int>();
		Vector2Int itemSize = GetItemSize(rotated);
		for (int i = 0; i < itemSize.x; i++)
		{
			for (int j = 0; j < itemSize.y; j++)
			{
				list.Add(new Vector2Int(itemPosition.x + i, itemPosition.y + j));
			}
		}
		return list;
	}

	public Vector2Int GetItemSize(bool rotated)
	{
		return GetItemSize(ItemDefinition.ItemSize, rotated);
	}

	public Vector2Int GetItemSize()
	{
		return GetItemSize(ItemRotated);
	}

	public static Vector2Int GetItemSize(Vector2Int itemSize, bool rotated)
	{
		if (rotated)
		{
			return new Vector2Int(itemSize.y, itemSize.x);
		}
		return itemSize;
	}

	public virtual void DoSpecialAction()
	{
	}

	public virtual bool CanUse(Inventory inventory, bool checkConditions)
	{
		return ItemDefinition.CanUse(inventory, checkConditions);
	}

	public virtual bool CanEquip(Inventory inventory)
	{
		return m_itemDefinition.CanEquip(inventory);
	}

	public virtual bool CanBeActivated()
	{
		return m_itemDefinition.CanBeActivated();
	}
}
