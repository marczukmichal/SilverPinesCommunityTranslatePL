using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class ProjectileWeapon : MonoBehaviour
{
	[Serializable]
	private struct UpgradeTag
	{
		public string m_tag;

		public GameObject m_gameObject;
	}

	[SerializeField]
	private Transform m_projectileSpawn;

	[SerializeField]
	private Transform m_muzzle;

	[SerializeField]
	private Transform m_ejectionPort;

	[SerializeField]
	private Transform m_pumpSlide;

	[SerializeField]
	private WeaponInfoGameEventChannel m_weaponInfoGameEventChannel;

	[SerializeField]
	private RemoveItemInstanceGameEventChannel m_removeItemEventChannel;

	[SerializeField]
	private Sprite m_frontArmSprite;

	[SerializeField]
	private Vector3 m_frontArmPosition;

	[SerializeField]
	private Sprite m_backArmSprite;

	[SerializeField]
	private Vector3 m_backArmPosition;

	[SerializeField]
	private UpgradeTag[] m_upgrades;

	private ProjectileWeaponItemInstance m_weaponInstance;

	private bool m_firing;

	private int m_burstFireCount;

	private float m_fireBlockedUntilTime;

	private CharacterIdentifier m_owner;

	private CharacterAiming m_characterAiming;

	private CharacterFSMUtilities m_fsmUtility;

	private NoiseSource m_noiseSource;

	private FieldOfViewViewer m_fieldOfView;

	private Vector3 m_localPosition;

	private bool m_shellCasingQueued;

	private Vector3 m_pumpSlideStartingPosition;

	private int m_queuedEjectShellCasingsReload;

	private bool m_allowPumpAction;

	private Transform ProjectileSpawnTransform
	{
		get
		{
			if (m_projectileSpawn != null)
			{
				return m_projectileSpawn;
			}
			if (m_muzzle != null)
			{
				return m_muzzle;
			}
			return base.transform;
		}
	}

	public Sprite FrontArmSprite => m_frontArmSprite;

	public Vector3 FrontArmPosition => m_frontArmPosition;

	public Sprite BacktArmSprite => m_backArmSprite;

	public Vector3 BackArmPosition => m_backArmPosition;

	public ProjectileWeaponItemInstance WeaponItemInstance => m_weaponInstance;

	public bool AllowPumpAction
	{
		get
		{
			if (m_allowPumpAction)
			{
				return !string.IsNullOrEmpty(m_weaponInstance.WeaponSettings.PumpFSMEvent);
			}
			return false;
		}
		set
		{
			m_allowPumpAction = value;
		}
	}

	private void Awake()
	{
		m_owner = GetComponentInParent<CharacterIdentifier>();
		if (m_owner != null)
		{
			m_fsmUtility = m_owner.GetComponent<CharacterFSMUtilities>();
			m_characterAiming = m_owner.GetComponent<CharacterAiming>();
			m_noiseSource = m_owner.GetComponent<NoiseSource>();
			m_fieldOfView = m_owner.GetComponentInChildren<FieldOfViewViewer>();
		}
		if (m_pumpSlide != null)
		{
			m_pumpSlideStartingPosition = m_pumpSlide.localPosition;
		}
	}

	public void Setup(ProjectileWeaponItemInstance weapon)
	{
		m_weaponInstance = weapon;
		m_localPosition = base.transform.localPosition;
		UpdateUpgradeVisibility();
	}

	public void SetFiring(bool firing)
	{
		if (m_firing != firing)
		{
			m_firing = firing;
			if (m_firing)
			{
				m_burstFireCount = 0;
			}
		}
	}

	public void Update()
	{
		bool flag = m_firing;
		if (!flag && m_weaponInstance.WeaponSettings.FireMode == ProjectileWeaponSettings.TriggerFireMode.AutomaticMinBurst3 && m_burstFireCount > 0 && m_burstFireCount < 3)
		{
			flag = true;
		}
		if (flag && m_fireBlockedUntilTime <= Time.time)
		{
			if (m_weaponInstance.RequiresAction)
			{
				StartCoroutine(PerformWeaponLoadAction());
			}
			else if (m_weaponInstance.CanFire())
			{
				Fire();
			}
			else
			{
				DryFailFire();
			}
		}
		if (m_queuedEjectShellCasingsReload > 0)
		{
			StartCoroutine(EjectShallCasingsForReloadCoroutine(m_queuedEjectShellCasingsReload));
			m_queuedEjectShellCasingsReload = 0;
		}
	}

	public void UpdateUpgradeVisibility()
	{
		UpgradeTag[] upgrades = m_upgrades;
		for (int i = 0; i < upgrades.Length; i++)
		{
			UpgradeTag upgradeTag = upgrades[i];
			upgradeTag.m_gameObject.SetActive(m_weaponInstance.HasUpgradeTag(upgradeTag.m_tag));
		}
	}

	private void OnDisable()
	{
		if (m_shellCasingQueued)
		{
			EjectShellCasing();
			m_shellCasingQueued = false;
		}
		m_firing = false;
	}

	public void FireSingleShot()
	{
		if (m_weaponInstance.CanFire())
		{
			Fire();
		}
		else
		{
			DryFailFire();
		}
	}

	private void Fire()
	{
		float num = m_weaponInstance.GetRefireTime();
		CharacterInventory component = m_owner.GetComponent<CharacterInventory>();
		if (component != null && component.ArtifactInventory.HasActiveArtifactEffect(GlobalReferences.Instance.ArtifactEffects.Generic.RateOfFire, out float floatValue))
		{
			num /= 1f + floatValue;
		}
		m_fireBlockedUntilTime = Time.time + num;
		m_burstFireCount++;
		if (m_weaponInstance.WeaponSettings.FireMode == ProjectileWeaponSettings.TriggerFireMode.Single)
		{
			m_firing = false;
		}
		else if (m_weaponInstance.WeaponSettings.FireMode == ProjectileWeaponSettings.TriggerFireMode.Burst3 && m_burstFireCount == 3)
		{
			m_firing = false;
		}
		bool flag = ProjectileSpawnTransform.lossyScale.x < 0f;
		Quaternion rotation = ProjectileSpawnTransform.rotation * Quaternion.Euler(0f, 0f, flag ? 180f : 0f);
		base.transform.DOKill();
		bool flag2 = false;
		if (m_characterAiming != null)
		{
			flag2 = m_characterAiming.WeaponShownOnAnimNode;
		}
		Vector3 localPosition = (flag2 ? Vector3.zero : m_localPosition);
		base.transform.localPosition = localPosition;
		if (m_weaponInstance.WeaponSettings.TransformRecoilDistance > 0f)
		{
			Sequence s = DOTween.Sequence(base.transform);
			s.Append(base.transform.DOLocalMoveX(localPosition.x - m_weaponInstance.WeaponSettings.TransformRecoilDistance, m_weaponInstance.WeaponSettings.TransformRecoilDuration));
			s.Append(base.transform.DOLocalMoveX(localPosition.x, m_weaponInstance.WeaponSettings.TransformRecoilResetDuration));
		}
		if ((bool)m_weaponInstance.WeaponSettings.CameraShakeSettings)
		{
			CameraShakeEventData value = new CameraShakeEventData
			{
				m_cameraShakeSettings = m_weaponInstance.WeaponSettings.CameraShakeSettings,
				m_position = base.transform.position,
				m_direction = m_muzzle.right
			};
			GlobalReferences.Instance.EventChannels.Generic.CameraShake.Raise(value);
		}
		float num2 = 1f;
		int integerValue;
		if (component != null)
		{
			float num3 = 1f;
			if (m_weaponInstance.AmmoCount == 1 && component.ArtifactInventory.HasActiveArtifactEffect(GlobalReferences.Instance.ArtifactEffects.Generic.LastShotMagazineBonusDamage, out float floatValue2))
			{
				num3 += floatValue2;
			}
			if (component.ArtifactInventory.HasActiveArtifactEffect(GlobalReferences.Instance.ArtifactEffects.Generic.IncreasedRangedWeaponDamage, out float floatValue3))
			{
				num3 += floatValue3;
			}
			if (component.ArtifactInventory.HasActiveArtifactEffect(GlobalReferences.Instance.ArtifactEffects.Generic.IncreasedDamage, out var floatValue4, out integerValue))
			{
				num3 += floatValue4;
			}
			num2 *= num3;
		}
		ProjectileUtils.FireProjectile(ProjectileSpawnTransform.position, rotation, m_weaponInstance.LoadedAmmoType.AmmoProjectileSettings, m_owner.gameObject, m_weaponInstance.LoadedAmmoType.FiringSettings, -1f, num2);
		if ((bool)m_weaponInstance.WeaponSettings.FireAudioEvent)
		{
			GlobalReferences.Instance.EventChannels.Audio.PlayAudio.Raise(new PlayAudioEventData(m_weaponInstance.WeaponSettings.FireAudioEvent, m_muzzle.position, useOcclusion: false));
		}
		if ((bool)m_weaponInstance.WeaponSettings.FireNoiseImpulseSettings && (bool)m_noiseSource)
		{
			m_noiseSource.TriggerImpulse(m_weaponInstance.WeaponSettings.FireNoiseImpulseSettings);
		}
		if (m_weaponInstance.WeaponSettings.MuzzleFlashPrefab.HasAsset())
		{
			DynamicallySpawnedObject.Spawn(m_weaponInstance.WeaponSettings.MuzzleFlashPrefab, persistent: false, m_muzzle.position, rotation);
		}
		if (m_weaponInstance.WeaponSettings.SmokePrefab.HasAsset())
		{
			DynamicallySpawnedObject.Spawn(m_weaponInstance.WeaponSettings.SmokePrefab, persistent: false, m_muzzle.position, rotation);
		}
		if (m_weaponInstance.WeaponSettings.ShellCasingEjectionType == ShellCasingEjectionType.OnFire && m_weaponInstance.AmmoProjectileSettings.ShellCasingPrefab.HasAsset() && !AllowPumpAction)
		{
			StartCoroutine(SpawnShellCasingCoroutine());
		}
		if (m_characterAiming != null)
		{
			if ((double)m_weaponInstance.WeaponSettings.AimRecoil > 0.0)
			{
				float num4 = m_weaponInstance.WeaponSettings.AimRecoil;
				if (component != null && component.ArtifactInventory.HasActiveArtifactEffect(GlobalReferences.Instance.ArtifactEffects.Generic.ReducedRecoil, out float floatValue5))
				{
					num4 *= 1f - floatValue5;
				}
				if (component != null && component.ArtifactInventory.HasActiveArtifactEffect(GlobalReferences.Instance.ArtifactEffects.Generic.IncreasedRecoil, out float floatValue6))
				{
					num4 *= 1f + floatValue6;
				}
				m_characterAiming.AddRecoil(num4);
			}
			m_characterAiming.RecordFire();
		}
		bool flag3 = false;
		CharacterInventory component2 = m_owner.GetComponent<CharacterInventory>();
		if (component2 != null)
		{
			flag3 = component2.ArtifactInventory.HasActiveArtifactEffect(GlobalReferences.Instance.ArtifactEffects.Generic.InfiniteAmmo, out var _, out integerValue);
		}
		if (!GameDebugCommands.CHEAT_INFINITE_AMMO && !flag3)
		{
			m_weaponInstance.Fire();
			if (m_weaponInstance.AmmoCount == 0 && m_weaponInstance.WeaponSettings.DiscardOnEmpty)
			{
				m_removeItemEventChannel.Raise(new RemoveItemInstanceEventData
				{
					m_itemInstance = m_weaponInstance,
					m_itemAmount = 1
				});
			}
			if (m_weaponInstance.AmmoCount == 0 && component2 != null && component2.Inventory.CanReloadEquippedItem())
			{
				GlobalReferences.Instance.EventChannels.Hints.DynamicHint.Raise(DynamicHintEvent.ShouldReloadHint);
			}
		}
		SendWeaponInfoUpdateEvent(WeaponInfoData.UpdateType.Fire);
	}

	private void DryFailFire()
	{
		m_fireBlockedUntilTime = Time.time + m_weaponInstance.GetRefireTime();
		m_burstFireCount++;
		if (m_weaponInstance.WeaponSettings.FireMode == ProjectileWeaponSettings.TriggerFireMode.Single)
		{
			m_firing = false;
		}
		else if (m_weaponInstance.WeaponSettings.FireMode == ProjectileWeaponSettings.TriggerFireMode.Burst3 && m_burstFireCount == 3)
		{
			m_firing = false;
		}
		if ((bool)m_weaponInstance.WeaponSettings.WeaponDryAudioEvent)
		{
			GlobalReferences.Instance.EventChannels.Audio.PlayAudio.Raise(new PlayAudioEventData(m_weaponInstance.WeaponSettings.WeaponDryAudioEvent, m_muzzle.position, useOcclusion: false));
		}
		if ((bool)m_weaponInstance.WeaponSettings.DryFireNoiseImpulse && (bool)m_noiseSource)
		{
			m_noiseSource.TriggerImpulse(m_weaponInstance.WeaponSettings.DryFireNoiseImpulse);
		}
		SendWeaponInfoUpdateEvent(WeaponInfoData.UpdateType.DryFire);
	}

	private IEnumerator SpawnShellCasingCoroutine()
	{
		m_shellCasingQueued = true;
		yield return new WaitForSeconds(m_weaponInstance.WeaponSettings.ShellCasingDelay);
		EjectShellCasing();
		m_shellCasingQueued = false;
	}

	private IEnumerator PerformWeaponLoadAction()
	{
		m_fireBlockedUntilTime = Time.time + m_weaponInstance.WeaponSettings.ActionBlockFireTime;
		DOTween.Kill(m_pumpSlide);
		bool hasFSMState = AllowPumpAction && !string.IsNullOrEmpty(m_weaponInstance.WeaponSettings.PumpFSMEvent);
		if (hasFSMState)
		{
			m_fsmUtility.SendEventToCharacter(m_weaponInstance.WeaponSettings.PumpFSMEvent);
		}
		yield return new WaitForSeconds(m_weaponInstance.WeaponSettings.PumpDelay);
		if (m_characterAiming.enabled)
		{
			m_characterAiming.TweenPumpArm(m_weaponInstance.WeaponSettings, m_backArmPosition);
			if ((bool)m_weaponInstance.WeaponSettings.ActionAudioEvent)
			{
				GlobalReferences.Instance.EventChannels.Audio.PlayAudio.Raise(new PlayAudioEventData(m_weaponInstance.WeaponSettings.ActionAudioEvent, m_ejectionPort.position, useOcclusion: false));
			}
			PumpSlideTween();
		}
		m_fireBlockedUntilTime = Time.time + m_weaponInstance.WeaponSettings.ActionBlockFireTime;
		if (!hasFSMState)
		{
			if (m_weaponInstance.WeaponSettings.ShellCasingEjectionType == ShellCasingEjectionType.OnAction)
			{
				StartCoroutine(SpawnShellCasingCoroutine());
			}
			ClearWeaponActionRequirement();
		}
	}

	private void ClearWeaponActionRequirement()
	{
		m_weaponInstance.ClearActionRequirement();
		GlobalReferences.Instance.EventChannels.Generic.WeaponInfo.Raise(new WeaponInfoData
		{
			m_updateType = WeaponInfoData.UpdateType.WeaponAction,
			m_currentAmmoCount = m_weaponInstance.AmmoCount,
			m_maxAmmoCount = m_weaponInstance.GetMaxAmmoCapacity(),
			m_weaponInstance = m_weaponInstance
		});
	}

	private void PumpSlideTween()
	{
		Sequence s = DOTween.Sequence(m_pumpSlide);
		s.Append(m_pumpSlide.DOLocalMoveX(m_pumpSlideStartingPosition.x + m_weaponInstance.WeaponSettings.PumpDistance, m_weaponInstance.WeaponSettings.PumpInTime));
		s.AppendInterval(m_weaponInstance.WeaponSettings.PumpMidActionDelay);
		s.Append(m_pumpSlide.DOLocalMoveX(m_pumpSlideStartingPosition.x, m_weaponInstance.WeaponSettings.PumpOutTime));
	}

	public void PerformPumpFromAnimation(bool ejectShellCasing)
	{
		if ((bool)m_weaponInstance.WeaponSettings.ActionAudioEvent)
		{
			GlobalReferences.Instance.EventChannels.Audio.PlayAudio.Raise(new PlayAudioEventData(m_weaponInstance.WeaponSettings.ActionAudioEvent, m_ejectionPort.position, useOcclusion: false));
		}
		PumpSlideTween();
		if (ejectShellCasing && m_weaponInstance.WeaponSettings.ShellCasingEjectionType == ShellCasingEjectionType.OnAction)
		{
			EjectShellCasing();
		}
		ClearWeaponActionRequirement();
	}

	private void EjectShellCasing()
	{
		if (m_weaponInstance.AmmoProjectileSettings.ShellCasingPrefab.HasAsset())
		{
			bool flag = m_muzzle.lossyScale.x < 0f;
			float flippedScalar = (flag ? (-1f) : 1f);
			Quaternion rotation = m_muzzle.rotation;
			Quaternion quaternion = Quaternion.Euler(0f, 0f, m_weaponInstance.WeaponSettings.ShellCasingStartRotation.GetRandom() * flippedScalar);
			rotation *= quaternion;
			DynamicallySpawnObjectEventData eventData = new DynamicallySpawnObjectEventData(m_weaponInstance.AmmoProjectileSettings.ShellCasingPrefab, persistent: true, m_ejectionPort.position, rotation);
			ref UnityAction<GameObject> onSpawnedAction = ref eventData.m_onSpawnedAction;
			onSpawnedAction = (UnityAction<GameObject>)Delegate.Combine(onSpawnedAction, (UnityAction<GameObject>)delegate(GameObject spawnedObject)
			{
				Rigidbody2D component = spawnedObject.GetComponent<Rigidbody2D>();
				Vector3 vector = m_ejectionPort.transform.up * m_weaponInstance.WeaponSettings.ShellCasingForce.GetRandom();
				vector = Quaternion.Euler(0f, 0f, m_weaponInstance.WeaponSettings.ShellCasingForceRotation.GetRandom() * flippedScalar) * vector;
				component.AddForce(vector);
				component.angularVelocity = m_weaponInstance.WeaponSettings.ShellCasingAngularVelocity.GetRandom() * flippedScalar;
			});
			DynamicallySpawnedObject.Spawn(eventData);
			if ((bool)m_weaponInstance.WeaponSettings.ShellEjectAudioEvent)
			{
				GlobalReferences.Instance.EventChannels.Audio.PlayAudio.Raise(new PlayAudioEventData(m_weaponInstance.WeaponSettings.ShellEjectAudioEvent, m_ejectionPort.position, useOcclusion: false));
			}
		}
	}

	public void EjectMagazine()
	{
		if (m_weaponInstance.WeaponSettings.EjectedMagazine.HasAsset())
		{
			bool flag = m_muzzle.lossyScale.x < 0f;
			float flippedScalar = (flag ? (-1f) : 1f);
			Quaternion rotation = m_muzzle.rotation;
			Quaternion quaternion = Quaternion.Euler(0f, 0f, m_weaponInstance.WeaponSettings.ShellCasingStartRotation.GetRandom() * flippedScalar);
			rotation *= quaternion;
			DynamicallySpawnObjectEventData eventData = new DynamicallySpawnObjectEventData(m_weaponInstance.WeaponSettings.EjectedMagazine, persistent: true, m_ejectionPort.position, rotation);
			ref UnityAction<GameObject> onSpawnedAction = ref eventData.m_onSpawnedAction;
			onSpawnedAction = (UnityAction<GameObject>)Delegate.Combine(onSpawnedAction, (UnityAction<GameObject>)delegate(GameObject spawnedObject)
			{
				Rigidbody2D component = spawnedObject.GetComponent<Rigidbody2D>();
				Vector3 vector = m_ejectionPort.transform.up * m_weaponInstance.WeaponSettings.ShellCasingForce.GetRandom();
				vector = Quaternion.Euler(0f, 0f, m_weaponInstance.WeaponSettings.ShellCasingForceRotation.GetRandom() * flippedScalar) * vector;
				component.AddForce(vector);
				component.angularVelocity = m_weaponInstance.WeaponSettings.ShellCasingAngularVelocity.GetRandom() * flippedScalar;
			});
			DynamicallySpawnedObject.Spawn(eventData);
			if ((bool)m_weaponInstance.WeaponSettings.EjectMagazineAudioEvent)
			{
				GlobalReferences.Instance.EventChannels.Audio.PlayAudio.Raise(new PlayAudioEventData(m_weaponInstance.WeaponSettings.EjectMagazineAudioEvent, m_ejectionPort.position, useOcclusion: false));
			}
		}
	}

	public void EjectShellCasingsForReload()
	{
		m_queuedEjectShellCasingsReload = m_weaponInstance.FiredCount;
	}

	private IEnumerator EjectShallCasingsForReloadCoroutine(int count)
	{
		for (int i = 0; i < count; i++)
		{
			yield return SpawnShellCasingCoroutine();
		}
	}

	private void SendWeaponInfoUpdateEvent(WeaponInfoData.UpdateType type)
	{
		m_weaponInfoGameEventChannel.Raise(new WeaponInfoData
		{
			m_updateType = type,
			m_currentAmmoCount = ((m_weaponInstance != null) ? m_weaponInstance.AmmoCount : 0),
			m_maxAmmoCount = ((m_weaponInstance != null) ? m_weaponInstance.GetMaxAmmoCapacity() : 0),
			m_weaponInstance = m_weaponInstance
		});
	}
}
