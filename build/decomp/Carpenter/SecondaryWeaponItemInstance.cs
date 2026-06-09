using System;
using UnityEngine;

[Serializable]
public class SecondaryWeaponItemInstance : ItemInstance
{
	public SecondaryWeaponItemDefinition WeaponDefinition => base.ItemDefinition as SecondaryWeaponItemDefinition;

	public SecondaryWeaponItemInstance(SecondaryWeaponItemDefinition definition, int stackSize, Vector2Int position, bool rotated)
		: base(definition, stackSize, position, rotated)
	{
	}

	public override bool CanUse(Inventory inventory, bool checkConditions)
	{
		return true;
	}
}
