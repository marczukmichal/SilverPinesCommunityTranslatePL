using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class DamageCollider : BaseDamageCollider
{
	public enum ArtifactDamageColliderConfiguration
	{
		None,
		PlayerMainMelee,
		PlayerStomp
	}

	[SerializeField]
	protected GameObject m_source;

	[Header("Damage Settings")]
	[FormerlySerializedAs("m_newhitSettings")]
	[SerializeField]
	protected HitSettings m_damageHitSettings;

	[SerializeField]
	protected CameraShakeEventChannel m_cameraShakeEventChannel;

	[SerializeField]
	protected CameraShakeSettings m_cameraShakeOnHit;

	[SerializeField]
	protected UnityEvent m_onHitEvent;

	[SerializeField]
	protected TimeSlowType m_timeSlowOnHitType = TimeSlowType.MeleeLight;

	[SerializeField]
	protected UnityEvent m_onRebound;

	public static readonly float s_reboundColliisionCheckHeight = 0.2f;

	[SerializeField]
	private ArtifactDamageColliderConfiguration m_artifactConfiguration;

	public GameObject Source
	{
		get
		{
			return m_source;
		}
		set
		{
			m_source = value;
		}
	}

	public HitSettings HitSettings => m_damageHitSettings;

	public void SetHitSettings(HitSettings hitsettings)
	{
		m_damageHitSettings = hitsettings;
	}

	protected override void Start()
	{
		base.Start();
		if (!(m_collider != null))
		{
			return;
		}
		Collider2D[] componentsInChildren = m_source.GetComponentsInChildren<Collider2D>();
		foreach (Collider2D collider2D in componentsInChildren)
		{
			if (collider2D != m_collider)
			{
				Physics2D.IgnoreCollision(collider2D, m_collider);
			}
		}
	}

	protected override CharacterIdentifier.CharacterFaction GetFaction()
	{
		if (m_source != null)
		{
			CharacterIdentifier component = m_source.GetComponent<CharacterIdentifier>();
			if (component != null)
			{
				return component.Faction;
			}
		}
		return base.GetFaction();
	}

	protected override HitResultType PerformHit(Collider2D damageCollider, Collider2D otherCollider, CharacterIdentifier.CharacterFaction otherFaction, CharacterIdentifier characterIdentifier)
	{
		Vector2 vector = damageCollider.transform.position;
		Vector2 vector2 = (otherCollider.attachedRigidbody ? otherCollider.attachedRigidbody.worldCenterOfMass : ((Vector2)otherCollider.transform.position));
		if (Source != null)
		{
			vector.x = Source.transform.position.x;
		}
		Vector2 direction;
		if (m_isTouchDamage)
		{
			CharacterMovement componentInParent = otherCollider.GetComponentInParent<CharacterMovement>();
			direction = ((!(componentInParent != null) || !(Mathf.Abs(componentInParent.PreviousVelocity.x) > 0f)) ? (vector2 - vector).normalized : new Vector2(componentInParent.PreviousVelocity.x * -1f, 0f));
		}
		else
		{
			direction = (vector2 - vector).normalized;
		}
		Vector2 position = Vector2.zero;
		if (m_collider != null)
		{
			position = ((!(m_meleeWeapon != null)) ? Physics2D.ClosestPoint(vector, otherCollider) : Physics2D.ClosestPoint(m_meleeWeapon.transform.position, otherCollider));
		}
		bool isBackstab = false;
		CharacterDirection componentInParent2 = otherCollider.GetComponentInParent<CharacterDirection>();
		if (componentInParent2 != null && !componentInParent2.IsTurning && characterIdentifier != null)
		{
			Vector2 vector3 = m_source.transform.position;
			Vector2 vector4 = (Vector2)characterIdentifier.transform.position - vector3;
			if (vector4.x > 0.1f && componentInParent2.CurrentDirection == CharacterDirection.Facing.Right)
			{
				isBackstab = true;
			}
			else if (vector4.x < -0.1f && componentInParent2.CurrentDirection == CharacterDirection.Facing.Left)
			{
				isBackstab = true;
			}
		}
		float healthScalar = 1f;
		float num = 1f;
		if (m_meleeWeapon != null)
		{
			healthScalar = m_meleeWeapon.DamageScale;
			num = m_meleeWeapon.StaggerScale;
		}
		if (m_artifactConfiguration == ArtifactDamageColliderConfiguration.PlayerStomp && Source.GetComponent<CharacterInventory>().ArtifactInventory.HasActiveArtifactEffect(GlobalReferences.Instance.ArtifactEffects.Generic.IncreasedStompDamage, out var floatValue, out var integerValue))
		{
			healthScalar *= floatValue;
		}
		if (m_artifactConfiguration == ArtifactDamageColliderConfiguration.PlayerMainMelee)
		{
			CharacterInventory component = Source.GetComponent<CharacterInventory>();
			if (component.ArtifactInventory.HasActiveArtifactEffect(GlobalReferences.Instance.ArtifactEffects.Generic.IncreasedStaggerFromMelee, out var floatValue2, out integerValue))
			{
				num *= floatValue2;
			}
			float num2 = 1f;
			if (component.ArtifactInventory.HasActiveArtifactEffect(GlobalReferences.Instance.ArtifactEffects.Generic.IncreasedMeleeDamageLowHP, out var floatValue3, out integerValue))
			{
				CharacterHealth component2 = Source.GetComponent<CharacterHealth>();
				if (component2 != null && component2.IsWoundedOrLower())
				{
					num2 += floatValue3;
				}
			}
			if (component.ArtifactInventory.HasActiveArtifactEffect(GlobalReferences.Instance.ArtifactEffects.Generic.IncreasedMeleeDamage, out var floatValue4, out integerValue))
			{
				num2 += floatValue4;
			}
			if (component.ArtifactInventory.HasActiveArtifactEffect(GlobalReferences.Instance.ArtifactEffects.Generic.IncreasedDamage, out var floatValue5, out integerValue))
			{
				num2 += floatValue5;
			}
			if (component.ArtifactInventory.HasActiveArtifactEffect(GlobalReferences.Instance.ArtifactEffects.Generic.IncreasedSlashingWeaponDamage, out float floatValue6) && m_damageHitSettings.DamageType != null && m_damageHitSettings.DamageType.HasFlag(DamageType.DamageTypeFlags.AffectedBySlashingArtifact))
			{
				num2 += floatValue6;
			}
			healthScalar *= num2;
		}
		bool flag = false;
		CharacterMeleeBlock componentInParent3 = otherCollider.GetComponentInParent<CharacterMeleeBlock>();
		if (componentInParent3 != null)
		{
			CharacterMeleeBlock.TryBlockResult tryBlockResult = componentInParent3.TryBlockAttack(m_source.transform.position, Mathf.RoundToInt((float)m_damageHitSettings.HealthDamage * healthScalar), ref healthScalar);
			if (tryBlockResult != 0)
			{
				flag = true;
				if (tryBlockResult == CharacterMeleeBlock.TryBlockResult.Parried)
				{
					DamageInstance damageInstance = new DamageInstance();
					damageInstance.SetHealthDamage(0);
					damageInstance.SetDamageCategory(DamageCategory.Parry);
					DamageUtilities.ApplyDamage(m_source, damageInstance);
				}
			}
		}
		int healthDamage = Mathf.RoundToInt((float)m_damageHitSettings.HealthDamage * healthScalar);
		DamageModifiers damageModifiers = otherCollider.gameObject.GetComponent<DamageModifiers>();
		if (damageModifiers == null)
		{
			damageModifiers = otherCollider.GetComponentInParent<DamageModifiers>();
		}
		DamageInstance damageInstance2 = new DamageInstance().PopulateFromHitSettings(m_damageHitSettings).SetHealthDamage(healthDamage).SetDamageCategory(DamageCategory.DamageCollider)
			.SetDamageSource(m_source)
			.SetDirection(direction)
			.SetPosition(position)
			.SetTouchDamage(m_isTouchDamage)
			.SetIsBackstab(isBackstab)
			.SetIsMeleeBlocked(flag)
			.SetStatusEffectHitResults(m_damageHitSettings.StatusEffect.GenerateResult(flag))
			.ApplyDamageModifiers(damageModifiers)
			.ScaleDamageBasedOnFaction(m_characterFaction)
			.ScaleStaggerMultiplier(num);
		float num3 = 1f;
		if (m_multiHitScaling == DamageMultiHitScaling.PlayerMelee && m_countedHits >= 1)
		{
			num3 = 0.25f;
			CharacterHealth componentInParent4 = otherCollider.GetComponentInParent<CharacterHealth>();
			if (componentInParent4 != null && componentInParent4.IsDead)
			{
				return HitResultType.None;
			}
		}
		damageInstance2.ScaleHealthDamage(num3);
		damageInstance2.ScaleStaggerMultiplier(num3);
		if (m_damageHitSettings.HitFlags.HasFlag(HitFlags.OnlyHitKnockedDown))
		{
			bool flag2 = false;
			CharacterHealth componentInParent5 = otherCollider.GetComponentInParent<CharacterHealth>();
			if (componentInParent5 != null)
			{
				flag2 = componentInParent5.IsDead;
			}
			if (!flag2)
			{
				StatusEffectReceiver componentInParent6 = otherCollider.GetComponentInParent<StatusEffectReceiver>();
				if (componentInParent6 != null && !componentInParent6.IsStatusEffectActive(GlobalReferences.Instance.StatusEffects.Generic.Knockdown))
				{
					return HitResultType.None;
				}
			}
		}
		DamageBlock componentInParent7 = otherCollider.transform.GetComponentInParent<DamageBlock>();
		if ((bool)componentInParent7 && !componentInParent7.CanTakeDamage(damageInstance2))
		{
			componentInParent7.m_onDamageBlockEnabled = (UnityAction<DamageBlock, bool>)Delegate.Combine(componentInParent7.m_onDamageBlockEnabled, new UnityAction<DamageBlock, bool>(base.OnDamageBlockStateChanged));
			if (!m_overlappedCollidersDamageBlock.ContainsKey(componentInParent7))
			{
				m_overlappedCollidersDamageBlock.Add(componentInParent7, otherCollider);
			}
			return HitResultType.None;
		}
		bool flag3 = false;
		if (damageModifiers != null && damageModifiers.IgnoreForMeleeHitCount)
		{
			flag3 = false;
		}
		bool flag4 = false;
		bool flag5 = true;
		bool flag6 = false;
		if (otherCollider.gameObject.layer == GameLayers.WaterLayer)
		{
			flag5 = false;
			flag3 = false;
		}
		bool flag7 = false;
		CharacterHealth componentInParent8 = otherCollider.transform.GetComponentInParent<CharacterHealth>();
		if (componentInParent8 != null)
		{
			flag7 = true;
			damageInstance2.SetIsKillingBlow(componentInParent8.IsKillingBlow(damageInstance2));
		}
		IDamageable[] array = null;
		IDamageable component3 = otherCollider.transform.GetComponent<IDamageable>();
		if (component3 != null)
		{
			flag6 = component3.ShouldDeflectHit(isArmorPiercing: false);
		}
		bool flag8 = false;
		IDamageable[] array2;
		if (component3 != null && component3.ShouldConsumeHit(isArmorPiercing: false))
		{
			if (!m_damaged.Contains(component3))
			{
				flag5 = false;
				flag4 = true;
				array = new IDamageable[1] { component3 };
			}
			else
			{
				array = new IDamageable[0];
			}
			if (component3.ShouldConsumeMeleeHit())
			{
				flag3 = true;
			}
		}
		else
		{
			array = otherCollider.transform.GetComponentsInParent<IDamageable>();
			array2 = array;
			foreach (IDamageable damageable in array2)
			{
				flag5 = false;
				if (!m_damaged.Contains(damageable) && !damageable.ShouldIgnoreDamageCategory(DamageCategory.DamageCollider))
				{
					flag4 = true;
					if (damageable.ShouldConsumeMeleeHit())
					{
						flag3 = true;
					}
					if (damageable.ShouldForceMeleeRebound())
					{
						flag8 = true;
					}
				}
			}
		}
		if (!flag4 || flag6 || flag8)
		{
			if (flag8 || (CanRebound && flag5 && !otherCollider.isTrigger && otherCollider.GetComponent<PlatformEffector2D>() == null))
			{
				DoRebound(damageInstance2, otherCollider.gameObject, m_damageHitSettings.ImpactType);
				return HitResultType.Rebound;
			}
			return HitResultType.None;
		}
		if (flag3)
		{
			m_countedHits++;
		}
		bool flag9 = false;
		bool flag10 = damageInstance2.HealthDamageAmount > 0;
		if (m_meleeWeapon != null)
		{
			if (componentInParent8 != null && !componentInParent8.IsDead)
			{
				flag9 = m_meleeWeapon.WeaponDurabilityChangeEvent(MeleeWeapon.WeaponDurabilityLossType.MeleeHit);
			}
			if (flag10)
			{
				AudioEvent impactAudioEvent = m_meleeWeapon.WeaponItemDefinition.ImpactAudioEvent;
				if (impactAudioEvent != null)
				{
					bool flag11 = false;
					if (m_meleeWeapon.MeleeWeaponItemInstance != null)
					{
						flag11 = m_meleeWeapon.MeleeWeaponItemInstance.IsBroken();
					}
					impactAudioEvent.PlayWithParameteter(damageCollider.transform.position, "WeaponBroken", flag11 ? 1f : 0f);
				}
			}
			if (characterIdentifier != null)
			{
				m_meleeWeapon.AddBloodToWeapon();
			}
		}
		if (flag9)
		{
			damageInstance2.ScaleHealthDamage(2f);
		}
		bool flag12 = false;
		array2 = array;
		foreach (IDamageable damageable2 in array2)
		{
			if (!m_damaged.Contains(damageable2) && !damageable2.ShouldIgnoreDamageCategory(DamageCategory.DamageCollider))
			{
				damageable2.ApplyDamageInstance(damageInstance2);
				m_damaged.Add(damageable2);
				if (damageable2 is NewSideDoor)
				{
					flag12 = true;
				}
			}
		}
		if (flag10 && m_cameraShakeEventChannel != null && (bool)m_cameraShakeOnHit)
		{
			Vector2 vector5 = m_source.transform.position;
			Vector2 vector6 = (Vector2)otherCollider.transform.position - vector5;
			CameraShakeEventData value = new CameraShakeEventData
			{
				m_cameraShakeSettings = m_cameraShakeOnHit,
				m_position = damageCollider.transform.position,
				m_direction = vector6.normalized
			};
			m_cameraShakeEventChannel.Raise(value);
		}
		if (GameUtils.IsPlayer(Source) && otherFaction == CharacterIdentifier.CharacterFaction.Monsters && m_timeSlowOnHitType != 0)
		{
			GlobalReferences.Instance.EventChannels.Generic.GameSleep.Raise(m_timeSlowOnHitType);
		}
		if (CanRebound && (damageInstance2.HealthDamageAmount == 0 || flag9) && m_meleeWeapon != null)
		{
			DoRebound(damageInstance2, otherCollider.gameObject, m_damageHitSettings.ImpactType);
		}
		if (flag7)
		{
			m_onHitEvent.Invoke();
		}
		if (!flag12)
		{
			return HitResultType.HitDamageable;
		}
		return HitResultType.HitDoor;
	}

	protected override bool CheckForRebound()
	{
		bool result = false;
		Vector2 vector = m_source.transform.position;
		vector.y = m_collider.transform.position.y;
		Vector2 vector2 = m_collider.transform.position;
		CapsuleCollider2D capsuleCollider2D = m_collider as CapsuleCollider2D;
		if (capsuleCollider2D != null)
		{
			Vector2 size = capsuleCollider2D.size;
			size.y = s_reboundColliisionCheckHeight;
			RaycastHit2D raycastHit2D = Physics2D.CapsuleCast(vector2, size, CapsuleDirection2D.Horizontal, 0f, Vector2.zero, 0f, GameLayers.EnvironmentMask);
			if (raycastHit2D.collider != null)
			{
				result = true;
				DamageInstance damageInstance = new DamageInstance().PopulateFromHitSettings(m_damageHitSettings).SetDamageSource(m_source).SetDirection(vector2 - vector)
					.SetPosition(raycastHit2D.point);
				DoRebound(damageInstance, raycastHit2D.collider.gameObject, m_damageHitSettings.ImpactType);
			}
			return result;
		}
		Debug.LogError("Trying to check for rebound with non capsule collider, this is not supported!", base.gameObject);
		return false;
	}

	protected override void TriggerRebound(GameObject impactGO, Vector3 impactPosition)
	{
		Vector2 vector = m_source.transform.position;
		vector.y = m_collider.transform.position.y;
		Vector2 vector2 = m_collider.transform.position;
		DamageInstance damageInstance = new DamageInstance().PopulateFromHitSettings(m_damageHitSettings).SetDamageSource(m_source).SetDirection(vector2 - vector)
			.SetPosition(impactPosition);
		DoRebound(damageInstance, impactGO, m_damageHitSettings.ImpactType);
	}

	protected void DoRebound(DamageInstance damageInstance, GameObject impactGO, ImpactType impactType)
	{
		if (m_meleeWeapon != null)
		{
			AudioEvent reboundAudioEvent = m_meleeWeapon.WeaponItemDefinition.ReboundAudioEvent;
			if (reboundAudioEvent != null)
			{
				reboundAudioEvent.Play(base.transform.position);
			}
		}
		Vector2 normalized = (damageInstance.Direction * -1f).normalized;
		float z = Vector2.SignedAngle(Vector2.up, normalized);
		Quaternion rotation = Quaternion.Euler(0f, 0f, z);
		Vector3 impactPoint = damageInstance.Position;
		impactPoint.z = base.transform.position.z;
		ProjectileUtils.PerformImpactEffect(impactGO, impactType, impactPoint, rotation, spawnBulletHole: false);
		m_onRebound?.Invoke();
	}

	public virtual Color GetDebugColor()
	{
		Color red = Color.red;
		if (m_damageHitSettings.DamageType != null && m_damageHitSettings.DamageType.IgnoredByDefault)
		{
			red.g = 1f;
			red.r = 0f;
		}
		if (m_isTouchDamage)
		{
			red.b = 1f;
		}
		red.a = 0.5f;
		return red;
	}
}
