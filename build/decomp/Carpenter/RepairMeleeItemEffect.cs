using UnityEngine;

[CreateAssetMenu(fileName = "RepairMeleeItemEffect", menuName = "Items/Effect/Repair Melee")]
public class RepairMeleeItemEffect : ItemEffect
{
	[SerializeField]
	private float m_repairProportion = 0.5f;

	public float RepairProportion => m_repairProportion;

	public override void ApplyEffect(GameObject character, Inventory inventory)
	{
		if (character == null)
		{
			Debug.LogWarning("Tried to heal but no character found!");
			return;
		}
		CharacterInventory component = character.GetComponent<CharacterInventory>();
		if ((bool)component)
		{
			if (component.Inventory.EquippedMeleeItem is MeleeWeaponItemInstance meleeWeapon)
			{
				component.Inventory.RepairProportion(meleeWeapon, m_repairProportion);
			}
		}
		else
		{
			Debug.LogWarning("Tried to repair a melee item but no character inventory component found!");
		}
	}
}
