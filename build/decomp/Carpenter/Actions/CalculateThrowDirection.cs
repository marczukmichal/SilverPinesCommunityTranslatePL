using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("Combat")]
public class CalculateThrowDirection : FsmStateAction
{
	public FsmVector3 m_position;

	[RequiredField]
	public Transform m_throwFromTransform;

	public ProjectileUtils.CalculatedTrajectory m_calculateTrajectory;

	private CharacterEquipment m_characterEquipment;

	public override void Awake()
	{
		base.Awake();
		if (base.Owner != null)
		{
			m_characterEquipment = base.Owner.GetComponent<CharacterEquipment>();
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
		Vector2 aimDirection = Vector2.zero;
		bool flag = true;
		switch (m_calculateTrajectory)
		{
		case ProjectileUtils.CalculatedTrajectory.None:
			aimDirection = (m_position.Value - m_throwFromTransform.position).normalized;
			break;
		case ProjectileUtils.CalculatedTrajectory.Low:
			flag = ProjectileUtils.CalculateRotationTrajectory(m_characterEquipment.EquippedSecondaryWeaponItemInstance.WeaponDefinition.ThrownProjectileSettings.Speed, m_characterEquipment.EquippedSecondaryWeaponItemInstance.WeaponDefinition.ThrownProjectileSettings.GravityScalar, m_throwFromTransform.position, m_position.Value, useLowAngle: true, out aimDirection);
			break;
		case ProjectileUtils.CalculatedTrajectory.High:
			flag = ProjectileUtils.CalculateRotationTrajectory(m_characterEquipment.EquippedSecondaryWeaponItemInstance.WeaponDefinition.ThrownProjectileSettings.Speed, m_characterEquipment.EquippedSecondaryWeaponItemInstance.WeaponDefinition.ThrownProjectileSettings.GravityScalar, m_throwFromTransform.position, m_position.Value, useLowAngle: false, out aimDirection);
			break;
		}
		if (!flag)
		{
			aimDirection = Vector2.zero;
			Debug.LogWarning("Invalid shot?");
		}
		m_characterEquipment.ThrowDirection = aimDirection;
	}
}
