using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Items/Secondary Weapon Item Definition")]
public class SecondaryWeaponItemDefinition : ItemDefinition
{
	public enum SecondaryWeaponMode
	{
		Throwable
	}

	[SerializeField]
	private SecondaryWeaponMode m_secondaryWeaponMode;

	[SerializeField]
	private ProjectileSettings m_thrownProjectileSettings;

	[SerializeField]
	private bool m_consumeOnUsed;

	[SerializeField]
	private Vector3 m_heldPosition;

	[SerializeField]
	private Vector3 m_heldRotation;

	[Tooltip("This is an optional rotational offset that can be added to the throw angle")]
	[SerializeField]
	private float m_aimThrowingRotationOffset;

	public SecondaryWeaponMode WeaponMode => m_secondaryWeaponMode;

	public ProjectileSettings ThrownProjectileSettings => m_thrownProjectileSettings;

	public bool ConsumeOnUsed => m_consumeOnUsed;

	public Vector3 HeldPosition => m_heldPosition;

	public Vector3 HeldRotation => m_heldRotation;

	public float AimThrowingRotationOffset => m_aimThrowingRotationOffset;

	public override bool HideUseButton()
	{
		return true;
	}
}
