using System.Collections.Generic;
using UnityEngine;

public class DamageInstance
{
	private GameObject m_damageSource;

	private DamageCategory m_damageCategory;

	private int m_healthDamageAmount;

	private Vector2 m_direction;

	private Vector2 m_position;

	private float m_impactForce;

	private bool m_isTouchDamage;

	private DamageModifierZone m_hitDamageModifierZone;

	private bool m_isBackstab;

	private DamageType m_damageType;

	private ImpactType m_impactType;

	private ImpactReactDirection m_impactReactDirection;

	private bool m_isMeleeBlocked;

	private bool m_isKillingBlow;

	private HitFlags m_hitFlags;

	private float m_staggerMultiplier = 1f;

	private StatusEffectHitResults m_statusEffects;

	public GameObject DamageSource => m_damageSource;

	public DamageCategory DamageCategory => m_damageCategory;

	public int HealthDamageAmount => m_healthDamageAmount;

	public Vector2 Direction => m_direction;

	public Vector2 Position => m_position;

	public float ImpactForce => m_impactForce;

	public bool IsTouchDamage => m_isTouchDamage;

	public DamageModifierZone DamageModifierZone => m_hitDamageModifierZone;

	public bool IsWeakpointDamage
	{
		get
		{
			if (GameDebugCommands.ALWAYS_WEAKPOINT_HIT)
			{
				return true;
			}
			if (m_hitDamageModifierZone != null)
			{
				return m_hitDamageModifierZone.CountsAsWeakpoint;
			}
			return false;
		}
	}

	public bool IsBackStab => m_isBackstab;

	public DamageType DamageType => m_damageType;

	public ImpactType ImpactType => m_impactType;

	public ImpactReactDirection ImpactReactDirection => m_impactReactDirection;

	public bool IsMeleeBlocked => m_isMeleeBlocked;

	public bool IsKillingBlow => m_isKillingBlow;

	public HitFlags HitFlags => m_hitFlags;

	public float StaggerMultiplier => m_staggerMultiplier;

	public StatusEffectHitResults StatusEffects => m_statusEffects;

	public DamageInstance SetDamageSource(GameObject damageSource)
	{
		m_damageSource = damageSource;
		return this;
	}

	public DamageInstance SetDamageCategory(DamageCategory category)
	{
		m_damageCategory = category;
		return this;
	}

	public DamageInstance SetHealthDamage(int healthDamageAmount)
	{
		m_healthDamageAmount = healthDamageAmount;
		return this;
	}

	public DamageInstance SetDirection(Vector2 direction)
	{
		m_direction = direction;
		return this;
	}

	public DamageInstance SetPosition(Vector2 position)
	{
		m_position = position;
		return this;
	}

	public DamageInstance SetImpactForce(float impactForce)
	{
		m_impactForce = impactForce;
		return this;
	}

	public DamageInstance SetTouchDamage(bool isTouchDamage)
	{
		m_isTouchDamage = isTouchDamage;
		return this;
	}

	public DamageInstance SetIsBackstab(bool isBackStab)
	{
		m_isBackstab = isBackStab;
		return this;
	}

	public DamageInstance SetDamageType(DamageType damageType)
	{
		m_damageType = damageType;
		return this;
	}

	public DamageInstance SetImpactType(ImpactType impactType)
	{
		m_impactType = impactType;
		return this;
	}

	public DamageInstance SetImpactReactDirection(ImpactReactDirection impactReactDirection)
	{
		m_impactReactDirection = impactReactDirection;
		return this;
	}

	public DamageInstance SetIsMeleeBlocked(bool isMeleeBlocked)
	{
		m_isMeleeBlocked = isMeleeBlocked;
		return this;
	}

	public DamageInstance SetIsKillingBlow(bool isKillingBlow)
	{
		m_isKillingBlow = isKillingBlow;
		return this;
	}

	public DamageInstance SetHitFlags(HitFlags hitFlags)
	{
		m_hitFlags = hitFlags;
		return this;
	}

	public DamageInstance SetStaggerMultiplier(float staggerMultiplier)
	{
		m_staggerMultiplier = staggerMultiplier;
		return this;
	}

	public DamageInstance ScaleStaggerMultiplier(float staggerMultiplier)
	{
		m_staggerMultiplier *= staggerMultiplier;
		return this;
	}

	public DamageInstance ScaleDamageBasedOnFaction(CharacterIdentifier.CharacterFaction faction)
	{
		switch (faction)
		{
		case CharacterIdentifier.CharacterFaction.Player:
			m_healthDamageAmount = GameDifficultyManager.ScaleDamageValue(m_healthDamageAmount, DifficultyDamageScalingMode.PlayerDamageDealt);
			break;
		case CharacterIdentifier.CharacterFaction.Monsters:
			m_healthDamageAmount = GameDifficultyManager.ScaleDamageValue(m_healthDamageAmount, DifficultyDamageScalingMode.EnemyDamageDealt);
			break;
		}
		return this;
	}

	public DamageInstance SetStatusEffectHitResults(StatusEffectHitResults statusEffectResults)
	{
		m_statusEffects = statusEffectResults;
		return this;
	}

	public DamageInstance PopulateFromHitSettings(HitSettings hitSettings)
	{
		m_healthDamageAmount = hitSettings.HealthDamage;
		m_impactForce = hitSettings.ImpactForce;
		m_damageType = hitSettings.DamageType;
		m_impactType = hitSettings.ImpactType;
		m_impactReactDirection = hitSettings.ImpactReactDirection;
		m_hitFlags = hitSettings.HitFlags;
		m_staggerMultiplier = hitSettings.StaggerMultiplier;
		return this;
	}

	public DamageInstance ApplyDamageModifiers(DamageModifiers modifiers)
	{
		bool shouldIgnore = false;
		if (m_damageType != null && m_damageType.IgnoredByDefault)
		{
			shouldIgnore = true;
		}
		if (modifiers != null)
		{
			DamageModifiers.ApplyDamageModifiers(modifiers.Modifiers, modifiers.gameObject, ref m_healthDamageAmount, ref shouldIgnore, this);
		}
		if (shouldIgnore)
		{
			m_healthDamageAmount = 0;
		}
		return this;
	}

	public DamageInstance ApplyDamageModifierZone(DamageModifierZone modifierZone)
	{
		m_hitDamageModifierZone = modifierZone;
		bool shouldIgnore = false;
		if (modifierZone != null)
		{
			DamageModifiers.ApplyDamageModifiers(new List<DamageModifier> { modifierZone.DamageModifier }, modifierZone.gameObject, ref m_healthDamageAmount, ref shouldIgnore, this);
			if (m_damageSource != null && modifierZone.CountsAsWeakpoint)
			{
				CharacterInventory component = m_damageSource.GetComponent<CharacterInventory>();
				if (component != null && component.Inventory.HasActiveArtifactEffect(GlobalReferences.Instance.ArtifactEffects.Generic.BetterHeadshots, out float floatValue))
				{
					m_healthDamageAmount = Mathf.RoundToInt((float)m_healthDamageAmount * (1f + floatValue));
					m_staggerMultiplier += floatValue;
				}
			}
		}
		if (shouldIgnore)
		{
			m_healthDamageAmount = 0;
		}
		return this;
	}

	public DamageInstance ScaleHealthDamage(float scale)
	{
		m_healthDamageAmount = Mathf.RoundToInt((float)m_healthDamageAmount * scale);
		return this;
	}

	public bool IsDamagingHit()
	{
		return m_healthDamageAmount > 0;
	}
}
