using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Settings/Projectile")]
public class ProjectileSettings : ScriptableObject
{
	[Header("GameObject")]
	[SerializeField]
	private AssetReference m_projectilePrefab;

	[Header("Settings")]
	[SerializeField]
	private float m_sizeRadius = 0.3f;

	[SerializeField]
	private float m_speed;

	[SerializeField]
	private float m_angularVelocity;

	[SerializeField]
	private float m_gravityScalar = 1f;

	[SerializeField]
	private float m_deflectedShotSpeedScale = 0.2f;

	[SerializeField]
	private bool m_hitPlatformEffectors;

	[Tooltip("How close something needs to be in the depth to be able to hit it")]
	[SerializeField]
	private float m_depthImpactRange = 2f;

	[FormerlySerializedAs("m_newhitSettings")]
	[SerializeField]
	private HitSettings m_damageHitSettings;

	[Header("Shell Casings")]
	[SerializeField]
	private AssetReference m_shellCasingPrefab;

	[Header("Impact Explosion")]
	[SerializeField]
	private ExplosionSettings m_impactExplosion;

	[Header("Life Time Expired Explosion")]
	[SerializeField]
	private ExplosionSettings m_lifetimeExpiredExplosion;

	[Header("Damage Scaling")]
	[Tooltip("How damage falls off over distance")]
	[SerializeField]
	private AnimationCurve m_rangeDamageFalloff;

	[SerializeField]
	private float m_targetPenetration;

	[Tooltip("Chance to trigger target penetration")]
	[SerializeField]
	private float m_targetPenetrationChance = 1f;

	[SerializeField]
	private bool m_isArmorPiercing;

	[Header("Impact Effects")]
	[SerializeField]
	private AssetReference m_spawnOnImpactEffect;

	[SerializeField]
	private AudioEvent m_impactAudio;

	[Header("Residue Left On Impact")]
	[SerializeField]
	private AssetReference m_residuePuddle;

	[Header("Persist After Impact")]
	[SerializeField]
	private bool m_persistAfterImpact;

	[SerializeField]
	private float m_initialImpactDampening = 0.5f;

	public AssetReference ProjectilePrefab => m_projectilePrefab;

	public float SizeRadius => m_sizeRadius;

	public float Speed => m_speed;

	public float AngularVelocity => m_angularVelocity;

	public float GravityScalar => m_gravityScalar;

	public float DeflectedShotSpeedScale => m_deflectedShotSpeedScale;

	public bool HitPlatformEffectors => m_hitPlatformEffectors;

	public float DepthImpactRange => m_depthImpactRange;

	public HitSettings HitSettings => m_damageHitSettings;

	public AssetReference ShellCasingPrefab => m_shellCasingPrefab;

	public ExplosionSettings ImpactExplosion => m_impactExplosion;

	public ExplosionSettings LifeTimeExpiredExplosion => m_lifetimeExpiredExplosion;

	public AnimationCurve RangeDamageFalloff => m_rangeDamageFalloff;

	[Tooltip("How much damage is lost after passing through a target")]
	public float TargetPenetration => m_targetPenetration;

	public float TargetPenetrationChance => m_targetPenetrationChance;

	[Tooltip("If true then this can pierce armor plates on enemies")]
	public bool IsArmorPiercing => m_isArmorPiercing;

	public AssetReference SpawnOnImpactEffect => m_spawnOnImpactEffect;

	public AudioEvent ImpactAudio => m_impactAudio;

	public AssetReference ResiduePuddle => m_residuePuddle;

	public bool PersistAfterImpact => m_persistAfterImpact;

	public float InitialImpactDampening => m_initialImpactDampening;
}
