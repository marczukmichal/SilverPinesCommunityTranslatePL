using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Items/Projectile Weapon Item Definition")]
public class ProjectileWeaponItemDefinition : ItemDefinition
{
	[SerializeField]
	private GameObject m_wieldedWeaponPrefab;

	[SerializeField]
	private ProjectileWeaponSettings m_weaponSettings;

	[SerializeField]
	private List<AmmunitionItemDefinition> m_acceptedAmmunitionTypes;

	public GameObject WeaponGameObject => m_wieldedWeaponPrefab;

	public ProjectileWeaponSettings WeaponSettings => m_weaponSettings;

	public IEnumerable<AmmunitionItemDefinition> AcceptedAmmunitionTypes => m_acceptedAmmunitionTypes.AsReadOnly();

	public bool AcceptsAmmunition(AmmunitionItemDefinition ammunition)
	{
		return m_acceptedAmmunitionTypes.Contains(ammunition);
	}

	public AmmunitionItemDefinition GetDefaultAmmunitionType()
	{
		return m_acceptedAmmunitionTypes[0];
	}

	public override bool CanBeCombinedWith(ItemDefinition other)
	{
		if (other is AmmunitionItemDefinition && m_acceptedAmmunitionTypes.Contains(other as AmmunitionItemDefinition))
		{
			return true;
		}
		if (other is ProjectileWeaponUpgradeDefinition && (other as ProjectileWeaponUpgradeDefinition).IsCompatibleWith(this))
		{
			return true;
		}
		return base.CanBeCombinedWith(other);
	}

	public override bool CanEquip(Inventory inventory)
	{
		return true;
	}

	public override bool CanBeCombinedWithAnything()
	{
		return true;
	}
}
