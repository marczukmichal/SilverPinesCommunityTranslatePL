using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Items/Ammunition Item Definition")]
public class AmmunitionItemDefinition : ItemDefinition
{
	[SerializeField]
	private ProjectileSettings m_projectileSettings;

	[SerializeField]
	private AmmunitionFiringSettings m_firingSettings;

	[SerializeField]
	private Sprite m_ammoCountSprite;

	public ProjectileSettings AmmoProjectileSettings => m_projectileSettings;

	public AmmunitionFiringSettings FiringSettings => m_firingSettings;

	public Sprite AmmoCountSprite => m_ammoCountSprite;

	public override bool CanBeCombinedWith(ItemDefinition other)
	{
		if (other is ProjectileWeaponItemDefinition)
		{
			return other.CanBeCombinedWith(this);
		}
		return base.CanBeCombinedWith(other);
	}

	public override bool CanBeCombinedWithAnything()
	{
		return true;
	}
}
