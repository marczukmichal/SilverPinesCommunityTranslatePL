using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PersistentDataCollectableInventory
{
	[SerializeField]
	public List<CollectableInstance> m_carriedCollectables;

	[SerializeField]
	public List<CollectableInstance> m_deliveredCollectables;
}
