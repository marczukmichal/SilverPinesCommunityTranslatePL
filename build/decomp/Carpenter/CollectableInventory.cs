using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CollectableInventory", menuName = "Collectables/Collectable Inventory")]
public class CollectableInventory : ScriptableObject
{
	[Header("Data")]
	[SerializeField]
	private List<CollectableInstance> m_carriedCollectables;

	[SerializeField]
	private List<CollectableInstance> m_deliveredCollectables;

	[Header("Event Channels")]
	[SerializeField]
	private CollectableDefinitionEventChannel m_pickupCollectableEventChannel;

	[SerializeField]
	private CollectableDefinitionEventChannel m_deliverCollectableEventChannel;

	public int CarriedCollectableCount => m_carriedCollectables.Count;

	public int DeliveredCollectableCount => m_deliveredCollectables.Count;

	public IEnumerable<CollectableInstance> CarriedCollectables => m_carriedCollectables.AsReadOnly();

	private void OnEnable()
	{
		m_pickupCollectableEventChannel.Register(OnPickupCollectable);
		m_deliverCollectableEventChannel.Register(OnDeliverCollectable);
	}

	private void OnDisable()
	{
		m_pickupCollectableEventChannel.Unregister(OnPickupCollectable);
		m_deliverCollectableEventChannel.Unregister(OnDeliverCollectable);
	}

	private bool AlreadyHasCollectable(CollectableDefinition collectableDefinition)
	{
		foreach (CollectableInstance carriedCollectable in m_carriedCollectables)
		{
			if (carriedCollectable.CollectableDefinition == collectableDefinition)
			{
				return true;
			}
		}
		foreach (CollectableInstance deliveredCollectable in m_deliveredCollectables)
		{
			if (deliveredCollectable.CollectableDefinition == collectableDefinition)
			{
				return true;
			}
		}
		return false;
	}

	private void OnDeliverCollectable(CollectableDefinition collectableDefinition)
	{
		CollectableInstance collectableInstance = null;
		foreach (CollectableInstance carriedCollectable in m_carriedCollectables)
		{
			if (carriedCollectable.CollectableDefinition == collectableDefinition)
			{
				collectableInstance = carriedCollectable;
				break;
			}
		}
		if (collectableInstance != null)
		{
			m_deliveredCollectables.Add(collectableInstance);
			m_carriedCollectables.Remove(collectableInstance);
		}
		else
		{
			Debug.LogWarning("Tried to deliver collectable " + collectableDefinition.name + " but don't currently have it in collectable carried inventory!");
		}
	}

	private void OnPickupCollectable(CollectableDefinition collectableDefinition)
	{
		if (!AlreadyHasCollectable(collectableDefinition))
		{
			if (m_carriedCollectables.Count == 0 && m_deliveredCollectables.Count == 0)
			{
				GlobalReferences.Instance.EventChannels.Collectables.OnPickupFirstCollectable.Raise();
			}
			m_carriedCollectables.Add(new CollectableInstance(collectableDefinition));
		}
		else
		{
			Debug.LogWarning("Already has collectable " + collectableDefinition.name);
		}
	}

	public void Clear()
	{
		m_carriedCollectables.Clear();
		m_deliveredCollectables.Clear();
	}

	public bool HasDelivered(CollectableDefinition collectable)
	{
		foreach (CollectableInstance deliveredCollectable in m_deliveredCollectables)
		{
			if (deliveredCollectable.CollectableDefinition == collectable)
			{
				return true;
			}
		}
		return false;
	}

	public PersistentDataCollectableInventory GetPersistentData()
	{
		return new PersistentDataCollectableInventory
		{
			m_carriedCollectables = new List<CollectableInstance>(m_carriedCollectables),
			m_deliveredCollectables = new List<CollectableInstance>(m_deliveredCollectables)
		};
	}

	public void ReadFromPersistentData(PersistentDataCollectableInventory data)
	{
		m_carriedCollectables = new List<CollectableInstance>(data.m_carriedCollectables);
		m_deliveredCollectables = new List<CollectableInstance>(data.m_deliveredCollectables);
	}
}
