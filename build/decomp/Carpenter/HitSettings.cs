using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class HitSettings
{
	[FormerlySerializedAs("m_damage")]
	[SerializeField]
	private int m_healthDamage;

	[SerializeField]
	private DamageType m_damageType;

	[SerializeField]
	private float m_staggerMultiplier = 1f;

	[SerializeField]
	private ImpactType m_impactType;

	[SerializeField]
	private ImpactReactDirection m_impactReactDireaction;

	[SerializeField]
	private HitFlags m_hitFlags;

	[SerializeField]
	private StatusEffectHitSettings m_statusEffects;

	public int HealthDamage => m_healthDamage;

	public float ImpactForce => m_impactType switch
	{
		ImpactType.Large => 10000f, 
		ImpactType.Medium => 5000f, 
		ImpactType.Small => 1000f, 
		_ => 0f, 
	};

	public DamageType DamageType => m_damageType;

	public float StaggerMultiplier => m_staggerMultiplier;

	public ImpactType ImpactType => m_impactType;

	public ImpactReactDirection ImpactReactDirection => m_impactReactDireaction;

	public HitFlags HitFlags => m_hitFlags;

	public StatusEffectHitSettings StatusEffect => m_statusEffects;
}
