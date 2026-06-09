using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PersistentDataDynamicHints
{
	[SerializeField]
	public List<DynamicHintEvent> m_triggeredHintEvents;

	[SerializeField]
	public int m_loreCountSeen;

	[SerializeField]
	public int m_healingItemsSeen;
}
