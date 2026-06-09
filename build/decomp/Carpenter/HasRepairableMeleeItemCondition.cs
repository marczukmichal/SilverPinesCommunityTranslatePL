using UnityEngine;

[CreateAssetMenu(fileName = "NewHasRepairableMeleeItemCondition", menuName = "Items/Condition/Has Repairable Melee Item Condition")]
public class HasRepairableMeleeItemCondition : ItemCondition
{
	public override bool Check(ItemDefinition item, Inventory inventory)
	{
		if (GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item != null)
		{
			PlayerMainInventory playerMainInventory = inventory as PlayerMainInventory;
			if (playerMainInventory != null && playerMainInventory.EquippedMeleeItem != null && playerMainInventory.EquippedMeleeItem is MeleeWeaponItemInstance meleeWeaponItemInstance && meleeWeaponItemInstance.WeaponDefinition.HasDurability)
			{
				return meleeWeaponItemInstance.DurabilityPercent < 100;
			}
		}
		return false;
	}
}
