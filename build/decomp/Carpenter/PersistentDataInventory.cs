using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PersistentDataInventory
{
	[SerializeReference]
	public List<ItemInstance> m_items;
}
