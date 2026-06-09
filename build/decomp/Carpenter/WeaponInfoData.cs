public class WeaponInfoData
{
	public enum UpdateType
	{
		Fire,
		Reload,
		DryFire,
		WeaponAction
	}

	public UpdateType m_updateType;

	public int m_currentAmmoCount;

	public int m_maxAmmoCount;

	public ProjectileWeaponItemInstance m_weaponInstance;
}
