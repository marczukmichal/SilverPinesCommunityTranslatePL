using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Settings/Projectile Weapon")]
public class ProjectileWeaponSettings : ScriptableObject
{
	public enum TriggerFireMode
	{
		Single,
		Automatic,
		Burst3,
		AutomaticMinBurst3
	}

	[Header("Effects")]
	[SerializeField]
	private AssetReference m_muzzleFlashPrefab;

	[SerializeField]
	private AssetReference m_smokePrefab;

	[SerializeField]
	private CameraShakeSettings m_cameraShakeSettings;

	[SerializeField]
	private AudioEvent m_fireAudioEvent;

	[SerializeField]
	private NoiseImpulseSettings m_fireNoiseImpulse;

	[SerializeField]
	private AudioEvent m_weaponDryAudioEvent;

	[SerializeField]
	private NoiseImpulseSettings m_dryFireNoiseImpulse;

	[Header("Recoil")]
	[SerializeField]
	private float m_aimRecoil;

	[SerializeField]
	private float m_recoilApplicationRateScalar;

	[SerializeField]
	private AnimationCurve m_recoilRecoveryCurve;

	[SerializeField]
	private float m_recoilRecoveryCurveScalar;

	[SerializeField]
	private float m_transformRecoilDistance;

	[SerializeField]
	private float m_transformRecoilDuration;

	[SerializeField]
	private float m_transformRecoilResetDuration;

	[Header("Ammo and Firing")]
	[SerializeField]
	private float m_refireTime;

	[SerializeField]
	private TriggerFireMode m_triggerFireMode;

	[SerializeField]
	private int m_maxAmmo;

	[SerializeField]
	private bool m_discardOnEmpty;

	[Header("Reloading")]
	[SerializeField]
	private int m_autoReloadAmount = -1;

	[SerializeField]
	private ReloadAnimations m_standingReloadAnimations;

	[SerializeField]
	private ReloadAnimations m_crouchedReloadAnimations;

	[SerializeField]
	private AssetReference m_reloadActionViewAsset;

	[Tooltip("First value is how long until the new rounds are added to the weapon, second is how long the full animation is")]
	[SerializeField]
	private Vector2 m_reloadTimes;

	[SerializeField]
	private AudioEvent m_reloadStartAudioEvent;

	[SerializeField]
	private AudioEvent m_reloadAudioEvent;

	[SerializeField]
	private AudioEvent m_reloadCompleteAudioEvent;

	[Tooltip("If this weapon can be active reloaded")]
	[SerializeField]
	private ActiveUseType m_activeReloadType;

	[Tooltip("The window for this weapon during reload time in which the active reload prompt is correct")]
	[SerializeField]
	private Vector2 m_activeReloadWindow;

	[Header("Active Reload Type - SpeedChange")]
	[Tooltip("Multiplier applied to reloading speed when failed active reload")]
	[SerializeField]
	private float m_activeReloadSpeedOnFail = 0.75f;

	[Tooltip("Multiplier applied to reloading speed when successful active reload")]
	[SerializeField]
	private float m_activeReloadSpeedOnSuccess = 2f;

	[Tooltip("Speed applied to character animation on successful reload")]
	[SerializeField]
	private float m_activeReloadAnimationSpeedOnSuccess = 2f;

	[Tooltip("Speed applied to character animation on failed reload")]
	[SerializeField]
	private float m_activeReloadAnimationSpeedOnFailure = 1f;

	[Header("Shell Casings")]
	[SerializeField]
	private ShellCasingEjectionType m_shellCasingEjectionType;

	[Tooltip("How much force is applied to a shell casing when ejected (min/max random range)")]
	[SerializeField]
	private Vector2 m_shellCasingForce;

	[Tooltip("How much random rotation is applied to the force direction (min/max random range)")]
	[SerializeField]
	private Vector2 m_shellCasingForceRotation;

	[Tooltip("How much angular velocity is applied to the casing (min/max random range)")]
	[SerializeField]
	private Vector2 m_shellCasingAngularVelocity;

	[Tooltip("Random Angular rotation applied to starting rotation (min/max random range)")]
	[SerializeField]
	private Vector2 m_shellCasingStartRotation;

	[Tooltip("Delay before we spawn the shell casing")]
	[SerializeField]
	private float m_shellCasingDelay;

	[SerializeField]
	private AudioEvent m_shellEjectAudioEvent;

	[Header("Wall Blocked Behavior")]
	[SerializeField]
	private float m_wallBlockedDistance = 0.5f;

	[Header("Character Movement")]
	[SerializeField]
	private float m_movementBlockOnFireTime = 0.25f;

	[Header("Firearm Action / Pump Action")]
	[SerializeField]
	private bool m_useAction;

	[SerializeField]
	private float m_pumpDistance;

	[SerializeField]
	private float m_pumpInTime;

	[SerializeField]
	private float m_pumpOutTime;

	[SerializeField]
	private float m_pumpDelay;

	[SerializeField]
	private float m_pumpMidActionDelay;

	[FormerlySerializedAs("m_pumpFSMEvent")]
	[SerializeField]
	private string m_actionFSMEvent;

	[SerializeField]
	private float m_actionFSMTime;

	[FormerlySerializedAs("m_pumpAudioEvent")]
	[SerializeField]
	private AudioEvent m_actionAudioEvent;

	[SerializeField]
	private float m_actionBlockFireTime;

	[Header("Weapon Sway")]
	[SerializeField]
	private float m_weaponSwayFrequency = 1f;

	[SerializeField]
	private float m_weaponSwayScale;

	[SerializeField]
	private float m_weaponSwayFrequency2 = 1f;

	[SerializeField]
	private float m_weaponSwayScale2;

	[SerializeField]
	private float m_weaponSwayStabilisationDuration = 1f;

	[SerializeField]
	private float m_weaponSwayMaxStability = 0.5f;

	[Header("Magazine")]
	[SerializeField]
	private AssetReference m_ejectedMagazine;

	[SerializeField]
	private AudioEvent m_ejectMagazineAudioEvent;

	[Header("Animation")]
	[SerializeField]
	private AnimationClip m_aimStartAnimOverride;

	[SerializeField]
	private AnimationClip m_aimStartArmAnimOverride;

	[SerializeField]
	private AnimationClip m_aimEndAnimOverride;

	[SerializeField]
	private AnimationClip m_aimEndArmAnimOverride;

	[SerializeField]
	private AnimationClip m_aimBlockedAnimOverride;

	[Header("Aim Override")]
	[SerializeField]
	private bool m_useMinMaxAimOverride;

	[SerializeField]
	private Vector2 m_minMaxAimOverride;

	public AssetReference MuzzleFlashPrefab => m_muzzleFlashPrefab;

	public AssetReference SmokePrefab => m_smokePrefab;

	public CameraShakeSettings CameraShakeSettings => m_cameraShakeSettings;

	public AudioEvent FireAudioEvent => m_fireAudioEvent;

	public NoiseImpulseSettings FireNoiseImpulseSettings => m_fireNoiseImpulse;

	public AudioEvent WeaponDryAudioEvent => m_weaponDryAudioEvent;

	public NoiseImpulseSettings DryFireNoiseImpulse => m_dryFireNoiseImpulse;

	public float AimRecoil => m_aimRecoil;

	public float RecoilApplicationRateScalar => m_recoilApplicationRateScalar;

	public AnimationCurve RecoilRecoveryCurve => m_recoilRecoveryCurve;

	public float RecoilRecoveryCurveScalar => m_recoilRecoveryCurveScalar;

	public float TransformRecoilDistance => m_transformRecoilDistance;

	public float TransformRecoilDuration => m_transformRecoilDuration;

	public float TransformRecoilResetDuration => m_transformRecoilResetDuration;

	public float RefireTime => m_refireTime;

	public TriggerFireMode FireMode => m_triggerFireMode;

	public int MaxAmmo => m_maxAmmo;

	public bool DiscardOnEmpty => m_discardOnEmpty;

	public int AutoReloadAmount => m_autoReloadAmount;

	public ReloadAnimations StandingReloadAnimations => m_standingReloadAnimations;

	public ReloadAnimations CrouchedReloadAnimations => m_crouchedReloadAnimations;

	public AssetReference ReloadActionViewAsset => m_reloadActionViewAsset;

	public float AddAmmoReloadTime => m_reloadTimes.x;

	public float ReloadAnimationTime => m_reloadTimes.y;

	public AudioEvent ReloadStartAudioEvent => m_reloadStartAudioEvent;

	public AudioEvent ReloadAudioEvent => m_reloadAudioEvent;

	public AudioEvent ReloadCompleteAudioEvent => m_reloadCompleteAudioEvent;

	public ActiveUseType ActiveReloadType => m_activeReloadType;

	public Vector2 ActiveReloadWindow => m_activeReloadWindow;

	public float ActiveReloadSpeedOnFail => m_activeReloadSpeedOnFail;

	public float ActiveReloadSpeedOnSuccess => m_activeReloadSpeedOnSuccess;

	public float ActiveReloadAnimationSpeedOnSuccess => m_activeReloadAnimationSpeedOnSuccess;

	public float ActiveReloadAnimationSpeedOnFailure => m_activeReloadAnimationSpeedOnFailure;

	public ShellCasingEjectionType ShellCasingEjectionType => m_shellCasingEjectionType;

	public Vector2 ShellCasingForce => m_shellCasingForce;

	public Vector2 ShellCasingForceRotation => m_shellCasingForceRotation;

	public Vector2 ShellCasingAngularVelocity => m_shellCasingAngularVelocity;

	public Vector2 ShellCasingStartRotation => m_shellCasingStartRotation;

	public float ShellCasingDelay => m_shellCasingDelay;

	public AudioEvent ShellEjectAudioEvent => m_shellEjectAudioEvent;

	public float WallBlockedDistance => m_wallBlockedDistance;

	public float MovementBlockOnFireTime => m_movementBlockOnFireTime;

	public bool UseAction => m_useAction;

	public float PumpDistance => m_pumpDistance;

	public float PumpInTime => m_pumpInTime;

	public float PumpOutTime => m_pumpOutTime;

	public float PumpDelay => m_pumpDelay;

	public float PumpMidActionDelay => m_pumpMidActionDelay;

	public string PumpFSMEvent => m_actionFSMEvent;

	public float ActionFSMTime => m_actionFSMTime;

	public AudioEvent ActionAudioEvent => m_actionAudioEvent;

	public float ActionBlockFireTime => m_actionBlockFireTime;

	public float WeaponSwayFrequency => m_weaponSwayFrequency;

	public float WeaponSwayScale => m_weaponSwayScale;

	public float WeaponSwayFrequency2 => m_weaponSwayFrequency2;

	public float WeaponSwayScale2 => m_weaponSwayScale2;

	public float WeaponSwayStabilisationDuration => m_weaponSwayStabilisationDuration;

	public float WeaponSwayMaxStability => m_weaponSwayMaxStability;

	public AssetReference EjectedMagazine => m_ejectedMagazine;

	public AudioEvent EjectMagazineAudioEvent => m_ejectMagazineAudioEvent;

	public AnimationClip AimStartAnimOverride => m_aimStartAnimOverride;

	public AnimationClip AimStartArmAnimOverride => m_aimStartArmAnimOverride;

	public AnimationClip AimEndAnimOverride => m_aimEndAnimOverride;

	public AnimationClip AimEndArmAnimOverride => m_aimEndArmAnimOverride;

	public AnimationClip AimBlockedAnimOverride => m_aimBlockedAnimOverride;

	public bool UseMinMaxAimOverride => m_useMinMaxAimOverride;

	public Vector2 MinMaxAimOverride => m_minMaxAimOverride;
}
