using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;

public static class ProjectileUtils
{
	public enum CalculatedTrajectory
	{
		None,
		Low,
		High
	}

	[DebugCommand("golden_guns", "All player attacks do instant kill damage", "golden_guns <true/false>", typeof(bool), true)]
	private static bool CHEAT_GOLDEN_GUNS = false;

	public static readonly float s_minimumDamageScalar = 0.25f;

	public static bool CalculateRotationTrajectory(float speed, float gravityScalar, Vector3 startPos, Vector3 targetPos, bool useLowAngle, out Quaternion result)
	{
		float angle;
		bool num = CalculateRotationTrajectory(speed, gravityScalar, startPos, targetPos, useLowAngle, out angle);
		if (num)
		{
			result = Quaternion.Euler(0f, 0f, angle);
			return num;
		}
		result = Quaternion.identity;
		return num;
	}

	public static bool CalculateRotationTrajectory(float speed, float gravityScalar, Vector3 startPos, Vector3 targetPos, bool useLowAngle, out Vector2 aimDirection)
	{
		float angle;
		bool num = CalculateRotationTrajectory(speed, gravityScalar, startPos, targetPos, useLowAngle, out angle);
		if (num)
		{
			aimDirection = ExtensionMethods.DegreeToVector2(angle);
			return num;
		}
		aimDirection = Vector2.zero;
		return num;
	}

	public static bool CalculateRotationTrajectory(float speed, float gravityScalar, Vector3 startPos, Vector3 targetPos, bool useLowAngle, out float angle)
	{
		Vector3 vector = targetPos - startPos;
		float y = vector.y;
		vector.y = 0f;
		float magnitude = vector.magnitude;
		float num = (0f - Physics2D.gravity.y) * gravityScalar;
		float num2 = speed * speed;
		float num3 = num2 * num2 - num * (num * magnitude * magnitude + 2f * y * num2);
		float num4 = Mathf.Sqrt(num3);
		float y2 = (useLowAngle ? (num2 - num4) : (num2 + num4));
		angle = Mathf.Atan2(y2, num * magnitude) * 57.29578f;
		if (vector.x < 0f)
		{
			angle = 180f - angle;
		}
		return num3 >= 0f;
	}

	public static void FireProjectile(Vector3 position, Quaternion rotation, ProjectileSettings projectileSettings, GameObject owner, AmmunitionFiringSettings firingSettings = null, float speedOverride = -1f, float damageScalar = 1f)
	{
		int num = ((!firingSettings) ? 1 : firingSettings.ProjectileCount);
		for (int i = 0; i < num; i++)
		{
			float z = (firingSettings ? firingSettings.Spread.GetRandom() : 0f);
			Quaternion quaternion = rotation;
			Quaternion quaternion2 = Quaternion.Euler(0f, 0f, z);
			quaternion *= quaternion2;
			if (projectileSettings.ProjectilePrefab.HasAsset())
			{
				DynamicallySpawnObjectEventData eventData = new DynamicallySpawnObjectEventData(projectileSettings.ProjectilePrefab, persistent: false, position, quaternion);
				ref UnityAction<GameObject> onSpawnedAction = ref eventData.m_onSpawnedAction;
				onSpawnedAction = (UnityAction<GameObject>)Delegate.Combine(onSpawnedAction, (UnityAction<GameObject>)delegate(GameObject spawnedObject)
				{
					Projectile component2 = spawnedObject.GetComponent<Projectile>();
					Vector3 right2 = spawnedObject.transform.right;
					component2.Fire(right2, projectileSettings, owner, (speedOverride >= 0f) ? speedOverride : projectileSettings.Speed, projectileSettings.AngularVelocity, damageScalar);
				});
				DynamicallySpawnedObject.Spawn(eventData);
				continue;
			}
			Vector2 right = Vector2.right;
			right = quaternion * right;
			float z2 = position.z;
			if (owner != null)
			{
				z2 = owner.transform.position.z;
			}
			float minDepth = z2 - projectileSettings.DepthImpactRange;
			float maxDepth = z2 + projectileSettings.DepthImpactRange;
			RaycastHit2D[] array = Physics2D.RaycastAll(position, right, 20f, GameLayers.ProjectileMask, minDepth, maxDepth);
			List<RaycastHit2D> list = new List<RaycastHit2D>();
			RaycastHit2D[] array2 = array;
			for (int j = 0; j < array2.Length; j++)
			{
				RaycastHit2D item = array2[j];
				if (!(item.collider.gameObject == owner))
				{
					CharacterIdentifier componentInParent = item.collider.GetComponentInParent<CharacterIdentifier>();
					if ((!(componentInParent != null) || !(componentInParent.gameObject == owner)) && item.collider != null)
					{
						list.Add(item);
					}
				}
			}
			float num2 = 0f;
			List<GameObject> list2 = new List<GameObject>();
			foreach (RaycastHit2D item2 in list)
			{
				if (list2.Contains(item2.collider.gameObject))
				{
					continue;
				}
				float time = Vector2.Distance(position, item2.point);
				float num3 = projectileSettings.RangeDamageFalloff.Evaluate(time);
				if (num3 < float.Epsilon)
				{
					break;
				}
				bool flag = false;
				bool flag2 = true;
				bool flag3 = item2.collider.gameObject.layer == GameLayers.WaterLayer;
				IDamageable componentInParent2 = item2.collider.gameObject.GetComponentInParent<IDamageable>();
				if (componentInParent2.ShouldIgnoreDamageCategory(DamageCategory.Projectile))
				{
					continue;
				}
				CharacterIdentifier componentInParent3 = item2.collider.GetComponentInParent<CharacterIdentifier>();
				if (componentInParent3 != null)
				{
					Collider2D[] componentsInChildren = componentInParent3.GetComponentsInChildren<Collider2D>();
					foreach (Collider2D collider2D in componentsInChildren)
					{
						if (collider2D.gameObject != item2.collider.gameObject)
						{
							list2.Add(collider2D.gameObject);
						}
					}
				}
				if (componentInParent2 != null)
				{
					flag = componentInParent2.AllowPassThroughProjectile();
					flag2 = componentInParent2.AllowProjectilePenetration();
				}
				else if (!flag3)
				{
					flag2 = false;
				}
				Vector3 impactPoint = item2.point;
				impactPoint.z = position.z;
				if (num2 > 0f)
				{
					owner.GetComponent<CharacterIdentifier>().StartCoroutine(ProjectileImpactDelayed(num2, owner, item2.collider.gameObject, projectileSettings, damageScalar * num3, impactPoint, right * -1f, flag));
				}
				else
				{
					ProjectileImpact(owner, item2.collider.gameObject, projectileSettings, damageScalar * num3, impactPoint, right * -1f, flag);
				}
				num2 += 0.05f;
				if (flag2)
				{
					if (!flag && !flag3)
					{
						bool flag4 = false;
						float num4 = projectileSettings.TargetPenetrationChance;
						float num5 = projectileSettings.TargetPenetration;
						if (owner != null)
						{
							CharacterInventory component = owner.GetComponent<CharacterInventory>();
							if (component != null && component.Inventory.HasActiveArtifactEffect(GlobalReferences.Instance.ArtifactEffects.Generic.WeaponPenetrationChance, out float floatValue))
							{
								num4 = Mathf.Max(num4, floatValue);
								num5 = Mathf.Max(num4, 0.5f);
							}
						}
						if (num4 < 1f && UnityEngine.Random.Range(0f, 1f) > num4)
						{
							flag4 = true;
						}
						if (flag4)
						{
							damageScalar = 0f;
						}
						else
						{
							damageScalar *= num5;
						}
					}
				}
				else
				{
					damageScalar = 0f;
				}
				if (damageScalar < s_minimumDamageScalar)
				{
					break;
				}
			}
		}
	}

	public static IEnumerator ProjectileImpactDelayed(float delay, GameObject source, GameObject impactGO, ProjectileSettings projectileSettings, float damageScalar, Vector3 impactPoint, Vector3 impactNormal, bool passthrough, bool isMeleeWeaponBreakingHit = false)
	{
		yield return new WaitForSeconds(delay);
		ProjectileImpact(source, impactGO, projectileSettings, damageScalar, impactPoint, impactNormal, passthrough, isMeleeWeaponBreakingHit);
	}

	public static void ProjectileImpact(GameObject source, GameObject impactGO, ProjectileSettings projectileSettings, float damageScalar, Vector3 impactPoint, Vector3 impactNormal, bool passthrough, bool isMeleeWeaponBreakingHit = false)
	{
		float z = Vector2.SignedAngle(Vector2.up, impactNormal);
		Quaternion rotation = Quaternion.Euler(0f, 0f, z);
		DamageInstance damageInstance = ApplyDamageForProjectileImpact(source, impactGO, projectileSettings, impactPoint, impactNormal, isMeleeWeaponBreakingHit, damageScalar);
		if (!passthrough)
		{
			if (((impactGO != null) ? impactGO.GetComponentInParent<CharacterHitReact>() : null) == null || (damageInstance != null && !damageInstance.IsDamagingHit()))
			{
				PerformImpactEffect(impactGO, projectileSettings.HitSettings.ImpactType, impactPoint, rotation, spawnBulletHole: true);
			}
			if ((bool)projectileSettings.ImpactExplosion)
			{
				Explosion.SpawnExplosion(projectileSettings.ImpactExplosion, impactPoint, rotation, source);
			}
		}
	}

	public static void PerformImpactEffect(GameObject impactGO, ImpactType impactType, Vector3 impactPoint, Quaternion rotation, bool spawnBulletHole)
	{
		SurfaceType componentInParent = impactGO.GetComponentInParent<SurfaceType>();
		SurfaceSettings surfaceSettings = null;
		if (componentInParent != null)
		{
			surfaceSettings = componentInParent.Surface;
		}
		if (surfaceSettings == null)
		{
			surfaceSettings = GlobalReferences.Instance.DefaultSurfaceSettings;
		}
		AssetReference assetReference = null;
		if (surfaceSettings != null)
		{
			assetReference = surfaceSettings.GetImpactEffect(impactType);
		}
		if (assetReference != null && assetReference.HasAsset())
		{
			DynamicallySpawnedObject.Spawn(assetReference, persistent: false, impactPoint, rotation);
		}
		if (spawnBulletHole)
		{
			AssetReference bulletHolePrefab = surfaceSettings.BulletHolePrefab;
			if (bulletHolePrefab != null && bulletHolePrefab.HasAsset())
			{
				Vector3 position = impactPoint;
				position.z += UnityEngine.Random.Range(-0.2f, 0.2f);
				Quaternion rotation2 = rotation;
				rotation2 *= Quaternion.Euler(90f, 0f, 0f);
				DynamicallySpawnedObject.Spawn(bulletHolePrefab, persistent: true, position, rotation2);
			}
		}
	}

	private static DamageInstance ApplyDamageForProjectileImpact(GameObject source, GameObject impactGO, ProjectileSettings projectileSettings, Vector3 impactPoint, Vector3 impactNormal, bool isMeleeWeaponBreakingHit, float damageScalar)
	{
		CharacterIdentifier.CharacterFaction characterFaction = CharacterIdentifier.CharacterFaction.Unknown;
		CharacterIdentifier.CharacterFaction characterFaction2 = CharacterIdentifier.CharacterFaction.Unknown;
		bool flag = false;
		IDamageable component = impactGO.GetComponent<IDamageable>();
		if (component != null)
		{
			flag = component.ShouldConsumeHit(projectileSettings.IsArmorPiercing);
		}
		bool flag2 = false;
		bool flag3 = false;
		GameObject gameObject = impactGO;
		if (source != null)
		{
			CharacterIdentifier componentInParent = source.GetComponentInParent<CharacterIdentifier>();
			if (componentInParent != null)
			{
				characterFaction2 = componentInParent.Faction;
				flag2 = GameUtils.IsPlayer(componentInParent.gameObject);
			}
		}
		CharacterIdentifier componentInParent2 = impactGO.GetComponentInParent<CharacterIdentifier>();
		if (componentInParent2 != null)
		{
			flag3 = true;
			characterFaction = componentInParent2.Faction;
			gameObject = componentInParent2.gameObject;
		}
		if (characterFaction2 == characterFaction)
		{
			return null;
		}
		DamageModifierZone modifierZone = null;
		DamageModifierZone[] componentsInChildren = gameObject.GetComponentsInChildren<DamageModifierZone>();
		foreach (DamageModifierZone damageModifierZone in componentsInChildren)
		{
			if (damageModifierZone.IsHit(impactPoint))
			{
				modifierZone = damageModifierZone;
			}
		}
		DamageModifiers damageModifiers = impactGO.GetComponent<DamageModifiers>();
		if (damageModifiers == null && !flag)
		{
			damageModifiers = impactGO.GetComponentInParent<DamageModifiers>();
		}
		Vector2 vector = impactNormal * -1f;
		DamageInstance damageInstance = new DamageInstance().PopulateFromHitSettings(projectileSettings.HitSettings).SetDamageSource(source).SetDamageCategory(DamageCategory.Projectile)
			.SetDirection(vector)
			.SetPosition(impactPoint)
			.SetStatusEffectHitResults(projectileSettings.HitSettings.StatusEffect.GenerateResult(isBlocked: false))
			.ApplyDamageModifiers(damageModifiers)
			.ApplyDamageModifierZone(modifierZone)
			.ScaleDamageBasedOnFaction(characterFaction2);
		if (CHEAT_GOLDEN_GUNS && damageInstance.DamageSource != null && GameUtils.IsPlayer(damageInstance.DamageSource))
		{
			damageInstance.SetHealthDamage(1000000);
		}
		DamageBlock componentInParent3 = impactGO.transform.GetComponentInParent<DamageBlock>();
		if ((bool)componentInParent3 && !componentInParent3.CanTakeDamage(damageInstance))
		{
			return null;
		}
		CharacterHealth componentInParent4 = impactGO.transform.GetComponentInParent<CharacterHealth>();
		if (componentInParent4 != null)
		{
			damageInstance.SetIsKillingBlow(componentInParent4.IsKillingBlow(damageInstance));
		}
		if (isMeleeWeaponBreakingHit)
		{
			damageInstance.ScaleHealthDamage(2f);
		}
		damageInstance.ScaleHealthDamage(damageScalar);
		if (flag)
		{
			component.ApplyDamageInstance(damageInstance);
		}
		else
		{
			DamageUtilities.ApplyDamage(impactGO.transform.GetComponentsInParent<IDamageable>(), damageInstance);
		}
		Rigidbody2D componentInParent5 = impactGO.GetComponentInParent<Rigidbody2D>();
		if (componentInParent5 != null && !flag3)
		{
			Vector2 vector2 = vector;
			vector2.Normalize();
			Vector3 vector3 = projectileSettings.HitSettings.ImpactForce * vector2;
			componentInParent5.AddForceAtPosition(vector3, impactPoint);
		}
		if (flag2 && characterFaction == CharacterIdentifier.CharacterFaction.Monsters)
		{
			GlobalReferences.Instance.EventChannels.Generic.GameSleep.Raise(TimeSlowType.Projectile);
		}
		return damageInstance;
	}
}
