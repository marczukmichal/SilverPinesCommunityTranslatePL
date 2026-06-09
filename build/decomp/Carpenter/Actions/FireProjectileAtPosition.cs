using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("Combat")]
public class FireProjectileAtPosition : FsmStateAction
{
	public FsmVector3 m_position;

	public Transform m_fireTransform;

	public FsmGameObject m_fireFromGameObject;

	[SerializeField]
	public bool m_adjustSpeedIfNeeded;

	[ObjectType(typeof(ProjectileUtils.CalculatedTrajectory))]
	public FsmEnum m_calculateTrajectoryVariable;

	[ObjectType(typeof(ProjectileSettings))]
	public FsmObject m_projectileSettingsVariable;

	private Transform FireFromTransform
	{
		get
		{
			if (m_fireTransform != null)
			{
				return m_fireTransform;
			}
			if ((bool)m_fireFromGameObject.Value)
			{
				return m_fireFromGameObject.Value.transform;
			}
			return null;
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
		ProjectileSettings projectileSettings = m_projectileSettingsVariable.Value as ProjectileSettings;
		bool isValidShot = true;
		float num = projectileSettings.Speed;
		float gravityScalar = projectileSettings.GravityScalar;
		Quaternion rotation = CalculateTrajectory(ref isValidShot, num, gravityScalar);
		if (!isValidShot)
		{
			if (m_adjustSpeedIfNeeded)
			{
				for (int i = 0; i < 100; i++)
				{
					if (isValidShot)
					{
						break;
					}
					num *= 1.1f;
					rotation = CalculateTrajectory(ref isValidShot, num, gravityScalar);
				}
			}
			if (!isValidShot)
			{
				Debug.LogWarning("Invalid shot?");
			}
		}
		ProjectileUtils.FireProjectile(FireFromTransform.position, rotation, projectileSettings, base.Owner, null, num);
		Finish();
	}

	private Quaternion CalculateTrajectory(ref bool isValidShot, float speed, float gravityScalar)
	{
		Quaternion result = Quaternion.identity;
		switch ((ProjectileUtils.CalculatedTrajectory)(object)m_calculateTrajectoryVariable.Value)
		{
		case ProjectileUtils.CalculatedTrajectory.None:
			result = Quaternion.FromToRotation(toDirection: (m_position.Value - FireFromTransform.position).normalized, fromDirection: Vector3.right);
			break;
		case ProjectileUtils.CalculatedTrajectory.Low:
			isValidShot = ProjectileUtils.CalculateRotationTrajectory(speed, gravityScalar, FireFromTransform.position, m_position.Value, useLowAngle: true, out result);
			break;
		case ProjectileUtils.CalculatedTrajectory.High:
			isValidShot = ProjectileUtils.CalculateRotationTrajectory(speed, gravityScalar, FireFromTransform.position, m_position.Value, useLowAngle: false, out result);
			break;
		}
		return result;
	}
}
