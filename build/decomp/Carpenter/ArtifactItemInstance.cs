using System;
using UnityEngine;

[Serializable]
public class ArtifactItemInstance : ItemInstance
{
	public bool PoweredUp => true;

	public ArtifactItemDefinition ArtifactDefinition => base.ItemDefinition as ArtifactItemDefinition;

	public override string Description
	{
		get
		{
			if (PoweredUp)
			{
				return base.Description + "\n<size=8>\n</size><size=16>" + ArtifactDefinition.GetArtifactEffectDescription(PoweredUp) + "</size>";
			}
			return ArtifactDefinition.GetArtifactEffectDescription(PoweredUp);
		}
	}

	public ArtifactItemInstance(ArtifactItemDefinition definition, int stackSize, Vector2Int position, bool rotated)
		: base(definition, stackSize, position, rotated)
	{
	}

	public override bool CanEquip(Inventory inventory)
	{
		if (inventory is PlayerMainInventory playerMainInventory)
		{
			if (PoweredUp)
			{
				return playerMainInventory.CanEquipArtifact(this);
			}
			return false;
		}
		return false;
	}

	public void PowerUp()
	{
	}
}
