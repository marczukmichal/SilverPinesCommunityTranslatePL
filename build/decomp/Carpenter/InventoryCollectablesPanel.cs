using TMPro;
using UnityEngine;

public class InventoryCollectablesPanel : MonoBehaviour
{
	[SerializeField]
	private CollectableInventory m_collectablesInventory;

	[SerializeField]
	private TextMeshProUGUI m_text;

	private bool m_dirty;

	private void OnEnable()
	{
		GlobalReferences.Instance.EventChannels.Collectables.DeliverCollectableDefinition.Register(MarkDirty);
		GlobalReferences.Instance.EventChannels.Collectables.PickupCollectableDefinition.Register(MarkDirty);
		UpdateCollectableCount();
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.Collectables.DeliverCollectableDefinition.Unregister(MarkDirty);
		GlobalReferences.Instance.EventChannels.Collectables.PickupCollectableDefinition.Unregister(MarkDirty);
	}

	private void MarkDirty(CollectableDefinition arg0)
	{
		m_dirty = true;
	}

	private void UpdateCollectableCount()
	{
		m_text.text = m_collectablesInventory.CarriedCollectableCount + "<size=24>(" + m_collectablesInventory.DeliveredCollectableCount + ")</size>";
	}

	private void Update()
	{
		if (m_dirty)
		{
			m_dirty = false;
			UpdateCollectableCount();
		}
	}
}
