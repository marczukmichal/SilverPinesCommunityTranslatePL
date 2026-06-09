using System;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class LorePickup : MonoBehaviour, IPersistentComponent
{
	[Serializable]
	private class PersistentData
	{
		public bool m_pickedUp;
	}

	[Header("Lore Options")]
	[SerializeField]
	private LoreEntry m_loreItem;

	[SerializeField]
	private UnityEvent m_onLoreReadEvent;

	private bool m_canPickup = true;

	private PersistentDataObject m_persistentData;

	public LoreEntry LoreItem => m_loreItem;

	public bool LorePickedup => !m_canPickup;

	public void RegisterOnPickupEventListener(UnityAction action)
	{
		m_onLoreReadEvent.AddListener(action);
	}

	public bool RequiresPersistentData()
	{
		return true;
	}

	public void ReceiveDataStoreEntry(PersistentDataObject dataEntry)
	{
		m_persistentData = dataEntry;
		PersistentData persistentData = ((m_persistentData.Data != null) ? (m_persistentData.Data as PersistentData) : null);
		if (persistentData != null && persistentData.m_pickedUp)
		{
			m_canPickup = !persistentData.m_pickedUp;
		}
	}

	public void PickupItem()
	{
		if (m_loreItem.SaveToLoreInventory)
		{
			if (m_persistentData != null)
			{
				m_persistentData.Data = new PersistentData
				{
					m_pickedUp = true
				};
			}
			m_canPickup = false;
		}
		GlobalReferences.Instance.EventChannels.Lore.LorePickup.Raise(m_loreItem);
		m_onLoreReadEvent?.Invoke();
	}
}
