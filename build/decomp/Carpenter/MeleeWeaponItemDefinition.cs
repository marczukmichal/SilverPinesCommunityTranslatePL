using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "NewItem", menuName = "Items/Melee Weapon Item Definition")]
public class MeleeWeaponItemDefinition : ItemDefinition
{
	[Serializable]
	public struct MeleeDamageSettings
	{
		public Vector2 m_quickAttackColliderSize;

		public Vector2 m_quickAttackColliderOffset;

		public Vector2 m_heavyAttackColliderSize;

		public Vector2 m_heavyAttackColliderOffset;

		public float m_allowedDepthDistance;

		[FormerlySerializedAs("m_hitSettings")]
		public HitSettings m_normalHitSettings;

		public HitSettings m_brokenHitSettings;
	}

	[Header("Melee Weapon Settings")]
	[SerializeField]
	private MeleeDamageSettings m_damageSettings;

	[Header("Melee Move Set")]
	[SerializeField]
	private MeleeMoveSetSettings m_meleeMoveSet;

	[SerializeField]
	private float m_meleeAnimationSpeed = 1f;

	[Header("Stamina")]
	[SerializeField]
	private float m_staminaUseScalar = 1f;

	[Header("Charge Settings")]
	[SerializeField]
	private MeleeChargeSettings[] m_chargeStates;

	[Header("Block Settings")]
	[SerializeField]
	private AudioEvent m_blockAudioEvent;

	[Header("Durability Settings")]
	[Tooltip("Setting to 0 means weapon is indestructible")]
	[SerializeField]
	private int m_maxDurability;

	[SerializeField]
	private int m_durabilityPips = 3;

	[SerializeField]
	private float m_durabilityLossOnHit;

	[SerializeField]
	private float m_durabilityLossOnBlock;

	[SerializeField]
	private float m_durabilityLossOnParry;

	[SerializeField]
	private float m_durabilityLossOnThrownHit;

	[SerializeField]
	private AssetReference m_weaponDestroyedEffectPrefab;

	[Header("Throwing Settings")]
	[SerializeField]
	private ProjectileSettings m_thrownProjectileSettings;

	[Header("Weapon Prefab")]
	[SerializeField]
	private GameObject m_meleeWieldedPrefab;

	[SerializeField]
	private GameObject m_meleeBrokenWieldedPrefab;

	[Header("Audio")]
	[SerializeField]
	private AudioEvent m_swingAudioEvent;

	[SerializeField]
	private AudioEvent m_impactAudioEvent;

	[SerializeField]
	private AudioEvent m_reboundAudioEvent;

	[SerializeField]
	private AudioEvent m_weaponBreakAudioEvent;

	public MeleeDamageSettings DamageSettings => m_damageSettings;

	public MeleeMoveSetSettings MoveSet => m_meleeMoveSet;

	public float MeleeAnimationSpeed => m_meleeAnimationSpeed;

	public float StaminaUseScalar => m_staminaUseScalar;

	public MeleeChargeSettings[] ChargeStates => m_chargeStates;

	public AudioEvent BlockAudioEvent => m_blockAudioEvent;

	public int MaxDurability => m_maxDurability;

	public bool HasDurability => m_maxDurability > 0;

	public int DurabilityPerPip => Mathf.CeilToInt(m_maxDurability / m_durabilityPips);

	public float DurabilityLossOnHit => m_durabilityLossOnHit;

	public float DurabilityLossOnBlock => m_durabilityLossOnBlock;

	public float DurabilityLossOnParry => m_durabilityLossOnParry;

	public float DurabilityLossOnThrownHit => m_durabilityLossOnThrownHit;

	public AssetReference WeaponDestroyedEffectPrefab => m_weaponDestroyedEffectPrefab;

	public ProjectileSettings ThrownProjectileSettings => m_thrownProjectileSettings;

	public GameObject MeleeWieldedPrefab => m_meleeWieldedPrefab;

	public GameObject MeleeBrokenWieldedPrefab => m_meleeBrokenWieldedPrefab;

	public AudioEvent SwingAudioEvent => m_swingAudioEvent;

	public AudioEvent ImpactAudioEvent => m_impactAudioEvent;

	public AudioEvent ReboundAudioEvent => m_reboundAudioEvent;

	public AudioEvent WeaponBreakAudioEvent => m_weaponBreakAudioEvent;

	public override bool CanEquip(Inventory inventory)
	{
		return true;
	}

	public override bool CanBeCombinedWithAnything()
	{
		return true;
	}
}
