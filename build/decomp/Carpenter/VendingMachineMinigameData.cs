using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

[DisallowMultipleComponent]
[ShowInDesignerInspector]
public class VendingMachineMinigameData : MonoBehaviour, IPersistentComponent
{
	[Serializable]
	public class VendingMachineVisualSlotData
	{
		public string m_slotID;

		public Transform m_slotTransform;

		[NonSerialized]
		public AsyncOperationHandle<GameObject> m_loadedAssetHandle;

		[NonSerialized]
		public GameObject m_spawnedVisuals;
	}

	[Serializable]
	public class VendingMachineItemDefinition
	{
		public string m_slotID;

		[SerializeField]
		private ItemDefinition m_itemDefintion;

		public int m_instanceCount = 1;

		[Header("Deprecated")]
		[SerializeField]
		private GameObject m_itemPrefab;

		[Header("StackAmount is Deprecated, see ShopStackSize in Item Definition")]
		public int m_stackAmount;

		public ItemDefinition ItemDefinition
		{
			get
			{
				if (m_itemDefintion != null)
				{
					return m_itemDefintion;
				}
				if (m_itemPrefab != null)
				{
					return m_itemPrefab.GetComponent<ItemPickup>().ItemDefinition;
				}
				return null;
			}
		}
	}

	[Serializable]
	public class VendingMachineSlotStatus
	{
		public string m_slotId;

		public int m_instanceCount;
	}

	[Serializable]
	private class PersistentData
	{
		public List<VendingMachineSlotStatus> m_itemStatusSlots;

		public PersistentData()
		{
			m_itemStatusSlots = new List<VendingMachineSlotStatus>();
		}
	}

	[SerializeField]
	private Transform m_dispenserCompartment;

	[SerializeField]
	private Vector3 m_dispenserSize;

	[SerializeField]
	private VendingMachineVisualSlotData[] m_slots;

	[SerializeField]
	private float m_itemVisualsScale = 0.8f;

	[SerializeField]
	private Color m_itemColor;

	[SerializeField]
	private VendingMachineItemDefinition[] m_items;

	private PersistentDataObject m_persistentDataObject;

	private PersistentData m_persistentData;

	private VendingMachineItemDefinition GetDefintionForSlot(string slotId)
	{
		VendingMachineItemDefinition[] items = m_items;
		foreach (VendingMachineItemDefinition vendingMachineItemDefinition in items)
		{
			if (vendingMachineItemDefinition.m_slotID.Equals(slotId))
			{
				return vendingMachineItemDefinition;
			}
		}
		return null;
	}

	public List<VendingMachineItemDefinition> GetAvailableItems()
	{
		List<VendingMachineItemDefinition> list = new List<VendingMachineItemDefinition>();
		VendingMachineItemDefinition[] items = m_items;
		foreach (VendingMachineItemDefinition vendingMachineItemDefinition in items)
		{
			if (GetSlotInstanceCount(vendingMachineItemDefinition.m_slotID) != 0 && !(vendingMachineItemDefinition.ItemDefinition == null))
			{
				list.Add(vendingMachineItemDefinition);
			}
		}
		return list;
	}

	private void OnDestroy()
	{
		VendingMachineVisualSlotData[] slots = m_slots;
		foreach (VendingMachineVisualSlotData vendingMachineVisualSlotData in slots)
		{
			if (vendingMachineVisualSlotData.m_loadedAssetHandle.IsValid())
			{
				vendingMachineVisualSlotData.m_loadedAssetHandle.Release();
			}
		}
	}

	public void ReceiveDataStoreEntry(PersistentDataObject dataEntry)
	{
		m_persistentDataObject = dataEntry;
		m_persistentData = ((m_persistentDataObject.Data != null) ? (m_persistentDataObject.Data as PersistentData) : null);
		if (m_persistentData == null)
		{
			m_persistentData = new PersistentData();
			m_persistentDataObject.Data = m_persistentData;
			VendingMachineItemDefinition[] items = m_items;
			foreach (VendingMachineItemDefinition vendingMachineItemDefinition in items)
			{
				int instanceCount = 1;
				if (vendingMachineItemDefinition.m_instanceCount >= 1)
				{
					instanceCount = vendingMachineItemDefinition.m_instanceCount;
				}
				SetSlotInstanceCount(vendingMachineItemDefinition.m_slotID, instanceCount);
			}
		}
		PopulateItemVisuals();
	}

	public bool RequiresPersistentData()
	{
		return true;
	}

	private void SetSlotInstanceCount(string slotId, int instanceCount)
	{
		foreach (VendingMachineSlotStatus itemStatusSlot in m_persistentData.m_itemStatusSlots)
		{
			if (itemStatusSlot.m_slotId.Equals(slotId))
			{
				itemStatusSlot.m_instanceCount = instanceCount;
				return;
			}
		}
		VendingMachineSlotStatus vendingMachineSlotStatus = new VendingMachineSlotStatus();
		vendingMachineSlotStatus.m_slotId = slotId;
		vendingMachineSlotStatus.m_instanceCount = instanceCount;
		m_persistentData.m_itemStatusSlots.Add(vendingMachineSlotStatus);
	}

	public int GetSlotInstanceCount(string slotId)
	{
		foreach (VendingMachineSlotStatus itemStatusSlot in m_persistentData.m_itemStatusSlots)
		{
			if (itemStatusSlot.m_slotId.Equals(slotId))
			{
				return itemStatusSlot.m_instanceCount;
			}
		}
		return 0;
	}

	public void SellItem(VendingMachineItemSlot slot)
	{
		foreach (VendingMachineSlotStatus itemStatusSlot in m_persistentData.m_itemStatusSlots)
		{
			if (itemStatusSlot.m_slotId.Equals(slot.ID))
			{
				itemStatusSlot.m_instanceCount--;
				if (itemStatusSlot.m_instanceCount == 0)
				{
					HideSlotVisuals(slot.ID);
				}
				break;
			}
		}
	}

	private void HideSlotVisuals(string slotID)
	{
		VendingMachineVisualSlotData slotData = GetSlotData(slotID);
		if (slotData != null && slotData.m_spawnedVisuals != null)
		{
			slotData.m_spawnedVisuals.SetActive(value: false);
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.DrawWireCube(m_dispenserCompartment.position, m_dispenserSize);
	}

	private VendingMachineVisualSlotData GetSlotData(string slotID)
	{
		VendingMachineVisualSlotData[] slots = m_slots;
		foreach (VendingMachineVisualSlotData vendingMachineVisualSlotData in slots)
		{
			if (vendingMachineVisualSlotData.m_slotID.Equals(slotID))
			{
				return vendingMachineVisualSlotData;
			}
		}
		return null;
	}

	private void PopulateItemVisuals()
	{
		VendingMachineItemDefinition[] items = m_items;
		foreach (VendingMachineItemDefinition vendingMachineItemDefinition in items)
		{
			VendingMachineVisualSlotData slotData = GetSlotData(vendingMachineItemDefinition.m_slotID);
			if (GetSlotInstanceCount(slotData.m_slotID) == 0)
			{
				continue;
			}
			if (slotData != null)
			{
				ItemDefinition itemDefinition = vendingMachineItemDefinition.ItemDefinition;
				if (itemDefinition != null)
				{
					SpawnVisualsInSlot(itemDefinition, slotData);
				}
			}
			else
			{
				Debug.LogError("Item is in an unknown slot " + vendingMachineItemDefinition.m_slotID + " - " + vendingMachineItemDefinition.ItemDefinition.name);
			}
		}
	}

	private void SpawnVisualsInSlot(ItemDefinition itemDefinition, VendingMachineVisualSlotData slot)
	{
		Sprite inventorySprite = itemDefinition.InventorySprite;
		if (inventorySprite != null)
		{
			GameObject gameObject = new GameObject("VendingMachine_" + slot.m_slotID);
			gameObject.transform.SetParent(slot.m_slotTransform, worldPositionStays: false);
			gameObject.transform.localScale = Vector3.one * m_itemVisualsScale * itemDefinition.VendingMachineSizeScalar;
			SpriteRenderer spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
			spriteRenderer.sprite = inventorySprite;
			spriteRenderer.color = m_itemColor;
			slot.m_spawnedVisuals = gameObject;
		}
	}
}
