using System;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
[ShowInDesignerInspector]
public class ItemPickup : MonoBehaviour, IPersistentComponent, IItemPickup, IObjectVisibilityListener
{
	[Serializable]
	private class PersistentData
	{
		public int m_itemsRemaining;
	}

	[Header("Item Options")]
	[SerializeField]
	private ItemDefinition m_itemType;

	[SerializeField]
	private Vector2Int m_itemAmountRange;

	[Header("Event Channels")]
	[SerializeField]
	private MapDynamicData m_mapCompletionData;

	[Header("Unity Events")]
	[Tooltip("This event is fired only when picking up the item, not on reload")]
	[SerializeField]
	private UnityEvent m_onPickupEvent;

	[Header("Small Container")]
	[SerializeField]
	private bool m_isSmallContainer;

	[SerializeField]
	private Sprite m_containerOpenSprite;

	[SerializeField]
	private Sprite m_containerClosedSprite;

	[SerializeField]
	private SpriteRenderer m_containerSpriteRenderer;

	[Header("Glint")]
	[SerializeField]
	private GameObject m_itemGlint;

	[Header("Weapons Only")]
	[SerializeField]
	private int m_amountInItem;

	[Header("UI")]
	[SerializeField]
	private bool m_suppressNotification;

	private int m_itemAmount;

	private UnityAction m_onTryPickup;

	private bool m_pickupAllowed = true;

	private PersistentDataObject m_persistentData;

	public int AmountInItem => m_amountInItem;

	public bool PickupAllowed
	{
		get
		{
			return m_pickupAllowed;
		}
		set
		{
			m_pickupAllowed = value;
		}
	}

	public ItemDefinition ItemDefinition => m_itemType;

	public int ItemAmount
	{
		get
		{
			return m_itemAmount;
		}
		set
		{
			if (m_itemAmount != value)
			{
				m_itemAmount = value;
				if (m_persistentData != null)
				{
					m_persistentData.Data = new PersistentData
					{
						m_itemsRemaining = value
					};
				}
				if (value == 0)
				{
					PickupItem();
				}
			}
		}
	}

	public bool ItemPickedup => m_itemAmount == 0;

	public LootType LootType => LootType.ItemDefinition;

	public bool SuppressNotification => m_suppressNotification;

	public void RegisterOnPickupEventListener(UnityAction action)
	{
		m_onPickupEvent.AddListener(action);
	}

	public void UnregisterOnPickupEventListener(UnityAction action)
	{
		m_onPickupEvent.RemoveListener(action);
	}

	public void RegisterOnTryPickupEventListener(UnityAction action)
	{
		m_onTryPickup = (UnityAction)Delegate.Combine(m_onTryPickup, action);
	}

	public void UnregisterOnTryPickupEventListener(UnityAction action)
	{
		m_onTryPickup = (UnityAction)Delegate.Remove(m_onTryPickup, action);
	}

	public void SetItemAmount(int amount)
	{
		ItemAmount = amount;
	}

	public bool RequiresPersistentData()
	{
		return true;
	}

	public void ReceiveDataStoreEntry(PersistentDataObject dataEntry)
	{
		m_persistentData = dataEntry;
		PersistentData persistentData = ((m_persistentData.Data != null) ? (m_persistentData.Data as PersistentData) : null);
		if (persistentData != null)
		{
			m_itemAmount = persistentData.m_itemsRemaining;
			if (m_itemAmount == 0)
			{
				ItemPickedUpHide();
			}
		}
		else
		{
			GenerateItemAmount();
		}
	}

	public void GenerateItemAmount()
	{
		Vector2Int itemAmountRange = m_itemAmountRange;
		if (m_itemType is AmmunitionItemDefinition)
		{
			itemAmountRange.x = Mathf.RoundToInt(itemAmountRange.x);
			itemAmountRange.y = Mathf.RoundToInt(itemAmountRange.y);
			itemAmountRange.x = Mathf.Min(itemAmountRange.x, itemAmountRange.y);
		}
		m_itemAmount = itemAmountRange.GetRandom();
	}

	public void StartInteraction(BaseInteractor interactor)
	{
		if (ItemPickedup)
		{
			if (m_isSmallContainer)
			{
				m_containerSpriteRenderer.sprite = m_containerOpenSprite;
			}
			m_onTryPickup?.Invoke();
		}
	}

	public void FinishInteraction()
	{
		if (ItemAmount > 0 && m_isSmallContainer)
		{
			m_containerSpriteRenderer.sprite = m_containerClosedSprite;
		}
	}

	public void PickupItem()
	{
		ItemAmount = 0;
		if (m_persistentData != null)
		{
			m_persistentData.Data = new PersistentData
			{
				m_itemsRemaining = 0
			};
		}
		if (m_mapCompletionData != null)
		{
			string mapCompletionIdentifier = GetMapCompletionIdentifier();
			if (!string.IsNullOrEmpty(mapCompletionIdentifier))
			{
				m_mapCompletionData.AddCompletedItemID(mapCompletionIdentifier);
			}
		}
		m_onPickupEvent.Invoke();
		ItemPickedUpHide();
		DynamicallySpawnedObject component = GetComponent<DynamicallySpawnedObject>();
		if (component != null)
		{
			component.Release();
			component.ClearPersistentData();
		}
		AppearsOnMap component2 = GetComponent<AppearsOnMap>();
		if (component2 != null)
		{
			component2.SetCleared();
		}
	}

	public void ReplaceItem()
	{
		ItemAmount = 1;
		if (m_persistentData != null)
		{
			m_persistentData.Data = new PersistentData
			{
				m_itemsRemaining = 1
			};
		}
	}

	private void ItemPickedUpHide()
	{
		if (m_isSmallContainer)
		{
			m_containerSpriteRenderer.sprite = m_containerOpenSprite;
		}
		ParticleSystem componentInChildren = GetComponentInChildren<ParticleSystem>();
		if (componentInChildren != null)
		{
			componentInChildren.gameObject.SetActive(value: false);
		}
	}

	public void SetEnabledIfNotPickedup(bool enabled)
	{
		if (m_persistentData == null)
		{
			PersistentDataIdentifier component = GetComponent<PersistentDataIdentifier>();
			if (component != null)
			{
				component.UpdateFromDatastore();
			}
		}
		bool active = enabled && !ItemPickedup;
		base.gameObject.SetActive(active);
	}

	public string GetMapCompletionIdentifier()
	{
		PersistentDataIdentifier component = GetComponent<PersistentDataIdentifier>();
		if (component != null)
		{
			return component.GUID;
		}
		return null;
	}

	public void SetObjectVisibility(bool visible)
	{
		if (m_itemGlint != null)
		{
			m_itemGlint.gameObject.SetActive(visible);
		}
	}
}
