using UnityEngine;

public abstract class ItemCondition : ScriptableObject
{
	public abstract bool Check(ItemDefinition item, Inventory inventory);
}
