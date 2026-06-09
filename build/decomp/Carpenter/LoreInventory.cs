using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LoreInventory", menuName = "Lore/Lore Inventory")]
public class LoreInventory : ScriptableObject
{
	[Header("Data")]
	[SerializeField]
	private List<CollectedLoreInstance> m_collectedLore;

	[Header("Event Channels")]
	[SerializeField]
	private LoreEntryEventChannel m_lorePickupEventChannel;

	[SerializeField]
	private LoreEntryEventChannel m_lorePickupNoShowEventChannel;

	private LoreEntry m_focusedLoreEntry;

	public LoreEntry FocusedLoreEntry
	{
		get
		{
			return m_focusedLoreEntry;
		}
		set
		{
			m_focusedLoreEntry = value;
		}
	}

	public IReadOnlyCollection<CollectedLoreInstance> CollectedLore => m_collectedLore.AsReadOnly();

	private void OnEnable()
	{
		m_lorePickupEventChannel.Register(OnFindLoreEntry);
		m_lorePickupNoShowEventChannel.Register(OnFindLoreEntry);
	}

	private void OnDisable()
	{
		m_lorePickupEventChannel.Unregister(OnFindLoreEntry);
		m_lorePickupNoShowEventChannel.Unregister(OnFindLoreEntry);
	}

	private void OnFindLoreEntry(LoreEntry entry)
	{
		if (entry.SaveToLoreInventory)
		{
			AddLoreEntry(entry);
		}
	}

	public void AddLoreEntry(LoreEntry entry)
	{
		if (!HasLoreEntry(entry))
		{
			m_focusedLoreEntry = entry;
			m_collectedLore.Add(new CollectedLoreInstance(entry));
			GlobalReferences.Instance.EventChannels.Lore.LoreAddedToLoreInventory.Raise(entry);
			GlobalReferences.Instance.EventChannels.Hints.DynamicHint.Raise(DynamicHintEvent.LoreAdded);
		}
	}

	public bool HasLoreEntry(LoreEntry entry)
	{
		foreach (CollectedLoreInstance item in m_collectedLore)
		{
			if (item.LoreEntry == entry)
			{
				return true;
			}
		}
		return false;
	}

	public void Clear()
	{
		m_collectedLore.Clear();
	}

	public int GetMapViewMetadataLoreCount(MapViewMetadata mapViewMetadata)
	{
		int num = 0;
		foreach (CollectedLoreInstance item in m_collectedLore)
		{
			if (mapViewMetadata.EncompassesRegion(item.LoreEntry.AssociatedRegion))
			{
				num++;
			}
		}
		return num;
	}

	public PersistentDataLoreInventory GetPersistentData()
	{
		return new PersistentDataLoreInventory
		{
			m_lore = new List<CollectedLoreInstance>(m_collectedLore)
		};
	}

	public void ReadFromPersistentData(PersistentDataLoreInventory data)
	{
		m_collectedLore = new List<CollectedLoreInstance>(data.m_lore);
	}
}
