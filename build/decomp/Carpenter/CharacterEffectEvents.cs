using System;
using System.Collections;
using PowerTools;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;

public class CharacterEffectEvents : MonoBehaviour, IDamageable
{
	[Header("Movement")]
	[Tooltip("If checked, then surface sounds will override the selected footstep sounds")]
	[SerializeField]
	private bool m_useSurfaceSounds = true;

	[SerializeField]
	private AudioEvent m_jumpAudio;

	[SerializeField]
	private AudioEvent m_landAudio;

	[SerializeField]
	private AudioEvent m_knockedDownAudio;

	[SerializeField]
	private AudioEvent m_climbAudio;

	[SerializeField]
	private AudioEvent m_ladderAudio;

	[SerializeField]
	private AudioEvent m_wallImpactAudio;

	[Header("Footsteps")]
	[SerializeField]
	private bool m_useFootstepEffectAreas;

	[SerializeField]
	private bool m_useAnimFootPosition;

	[SerializeField]
	private AudioEvent m_footstepAudio;

	[SerializeField]
	private AudioEvent m_lightFootstepAudio;

	[SerializeField]
	private AudioEvent m_heavyFootstepAudio;

	[SerializeField]
	private CameraShakeSettings m_footstepCameraShake;

	[SerializeField]
	private AssetReference m_footstepImpactEffect;

	[Header("Grunts And Stuff")]
	[SerializeField]
	private AudioEvent m_takeHealthDamageAudio;

	[SerializeField]
	private AudioEvent m_hitWeakpointDamageAudio;

	[SerializeField]
	private AudioEvent m_deadAudio;

	[Header("Attached Surface Effect")]
	[SerializeField]
	private Transform m_attachSurfaceEffectParent;

	[Header("Wetness")]
	[SerializeField]
	private ParticleSystem m_bodyWetDripsEffect;

	[SerializeField]
	private ParticleSystem m_feetWetDripsEffect;

	private CharacterMovement m_characterMovement;

	private ParticleSystem m_activeStandingInParticleSystem;

	private SurfaceType m_activeWaterSurface;

	[Header("Components")]
	[SerializeField]
	private SpriteAnimNodes m_spriteAnimNodes;

	private float m_accumulatedWaterMovement;

	private Vector3 GetFootPosition()
	{
		if (m_useAnimFootPosition)
		{
			return m_spriteAnimNodes.GetPosition(SpriteAnimNodeType.FootPosition);
		}
		return base.transform.position;
	}

	private void Awake()
	{
		m_characterMovement = GetComponent<CharacterMovement>();
	}

	private void OnEnable()
	{
		if (m_characterMovement != null)
		{
			CharacterMovement characterMovement = m_characterMovement;
			characterMovement.m_onWaterSurfaceChanged = (UnityAction<SurfaceType>)Delegate.Combine(characterMovement.m_onWaterSurfaceChanged, new UnityAction<SurfaceType>(OnWaterSurfaceChanged));
		}
		CharacterHealth component = GetComponent<CharacterHealth>();
		if (component != null)
		{
			component.OnDying = (UnityAction<DamageInstance>)Delegate.Combine(component.OnDying, new UnityAction<DamageInstance>(PlayDeadEvent));
		}
	}

	private void OnDisable()
	{
		CharacterHealth component = GetComponent<CharacterHealth>();
		if (component != null)
		{
			component.OnDying = (UnityAction<DamageInstance>)Delegate.Remove(component.OnDying, new UnityAction<DamageInstance>(PlayDeadEvent));
		}
		if (m_characterMovement != null)
		{
			CharacterMovement characterMovement = m_characterMovement;
			characterMovement.m_onWaterSurfaceChanged = (UnityAction<SurfaceType>)Delegate.Remove(characterMovement.m_onWaterSurfaceChanged, new UnityAction<SurfaceType>(OnWaterSurfaceChanged));
		}
	}

	private IEnumerator DetachActiveStandingInParticleSystem(ParticleSystem particleSystem)
	{
		particleSystem.Stop();
		particleSystem.transform.SetParent(null);
		yield return new WaitUntil(() => !particleSystem.IsAlive());
		UnityEngine.Object.Destroy(particleSystem.gameObject);
	}

	private void OnWaterSurfaceChanged(SurfaceType waterSurface)
	{
		if (m_activeStandingInParticleSystem != null)
		{
			StartCoroutine(DetachActiveStandingInParticleSystem(m_activeStandingInParticleSystem));
			m_activeStandingInParticleSystem = null;
		}
		if (waterSurface != null && waterSurface.Surface.StandingInEffectPrefab != null)
		{
			Transform parent = ((m_attachSurfaceEffectParent != null) ? m_attachSurfaceEffectParent : base.transform);
			m_activeStandingInParticleSystem = UnityEngine.Object.Instantiate(waterSurface.Surface.StandingInEffectPrefab, parent);
			m_activeStandingInParticleSystem.transform.localPosition = new Vector3(0f, 0f, 0f);
			Vector3 position = m_activeStandingInParticleSystem.transform.position;
			position.y = waterSurface.SurfaceHeight;
			m_activeStandingInParticleSystem.transform.position = position;
		}
		if (waterSurface != null && m_characterMovement.PreviousVelocity.y < -3f)
		{
			PlayLandEffectEvent();
		}
		m_accumulatedWaterMovement = 0f;
		m_activeWaterSurface = waterSurface;
	}

	private void Update()
	{
		if (m_activeStandingInParticleSystem != null && m_activeWaterSurface != null)
		{
			Vector3 position = m_activeStandingInParticleSystem.transform.position;
			position.y = m_activeWaterSurface.SurfaceHeight;
			m_activeStandingInParticleSystem.transform.position = position;
			m_accumulatedWaterMovement += Mathf.Abs(m_characterMovement.PreviousVelocity.x * Time.deltaTime);
			float num = 0.5f;
			if (m_characterMovement.ActiveMovementSettings != null)
			{
				num = m_characterMovement.ActiveMovementSettings.DistanceToTriggerSplashEffect;
			}
			if (num > 0f && m_accumulatedWaterMovement > num)
			{
				m_accumulatedWaterMovement = 0f;
				SpawnSurfaceParticleEffect(base.transform.position, m_activeWaterSurface.Surface.MovementSplashEffectPrefab, m_activeWaterSurface.GetComponent<Collider2D>());
			}
		}
	}

	public void PlayFootstepEffectEvent()
	{
		PlayFootstepEffect(m_footstepAudio);
	}

	public void PlayLightFootstepEffectEvent()
	{
		PlayFootstepEffect(m_lightFootstepAudio);
	}

	public void PlayHeavyFootstepEffectEvent()
	{
		PlayFootstepEffect(m_heavyFootstepAudio);
	}

	private void PlayFootstepEffect(AudioEvent audioEvent)
	{
		Vector3 footPosition = GetFootPosition();
		SurfaceSettings surfaceSettings = null;
		if (m_useFootstepEffectAreas)
		{
			FootstepEffectArea active = FootstepEffectArea.GetActive(footPosition);
			if (active != null)
			{
				active.PlayEffect(footPosition);
			}
		}
		if (m_characterMovement != null)
		{
			surfaceSettings = m_characterMovement.CurrentSurfaceSettings;
		}
		if (surfaceSettings != null)
		{
			if (m_useSurfaceSounds && surfaceSettings.FootstepAudioEvent != null)
			{
				audioEvent = surfaceSettings.FootstepAudioEvent;
			}
			SpawnSurfaceParticleEffect(footPosition, surfaceSettings.FootstepEffectPrefab, m_characterMovement.CurrentSurfaceCollider);
			if (m_footstepImpactEffect != null)
			{
				SpawnSurfaceParticleEffect(footPosition, m_footstepImpactEffect, m_characterMovement.CurrentSurfaceCollider);
			}
		}
		PlayAudioEventWithSurfaceType(audioEvent, footPosition, surfaceSettings);
		if ((bool)m_footstepCameraShake)
		{
			CameraShakeEventData value = new CameraShakeEventData
			{
				m_cameraShakeSettings = m_footstepCameraShake,
				m_position = base.transform.position,
				m_direction = UnityEngine.Random.onUnitSphere
			};
			GlobalReferences.Instance.EventChannels.Generic.CameraShake.Raise(value);
		}
	}

	private void PlayAudioEventWithSurfaceType(AudioEvent audioEvent, Vector3 position, SurfaceSettings surfaceSettings)
	{
		if (!(audioEvent == null))
		{
			bool useOcclusion = !GameUtils.IsPlayer(base.gameObject);
			if (surfaceSettings != null && !string.IsNullOrEmpty(surfaceSettings.AudioSurfaceParameterLabel))
			{
				audioEvent.PlayWithParameteterLabel(position, "SurfaceType", surfaceSettings.AudioSurfaceParameterLabel, useOcclusion);
			}
			else
			{
				audioEvent.Play(position, useOcclusion);
			}
		}
	}

	public void PlayWallImpactEffect()
	{
		Vector2 origin = base.transform.position;
		origin.y += 1f;
		Vector2 direction = GetComponent<CharacterDirection>().GetForwardVector();
		RaycastHit2D raycastHit2D = Physics2D.Raycast(origin, direction, 1f, GameLayers.CharacterNavigationMask);
		AudioEvent audioEvent = m_wallImpactAudio;
		if (!raycastHit2D)
		{
			return;
		}
		SurfaceType component = raycastHit2D.collider.GetComponent<SurfaceType>();
		SurfaceSettings surfaceSettings = GlobalReferences.Instance.DefaultSurfaceSettings;
		if (component != null)
		{
			surfaceSettings = component.Surface;
		}
		if (surfaceSettings != null)
		{
			if ((bool)surfaceSettings.RunIntoAudioEvent)
			{
				audioEvent = surfaceSettings.RunIntoAudioEvent;
			}
			Vector3 position = raycastHit2D.point;
			position.z = base.transform.position.z;
			PlayAudioEventWithSurfaceType(audioEvent, position, surfaceSettings);
		}
	}

	private void SpawnSurfaceParticleEffect(Vector3 position, AssetReference assetReference, Collider2D collider)
	{
		if (!assetReference.HasAsset())
		{
			return;
		}
		if (collider != null)
		{
			position.y = collider.bounds.max.y;
			position.z -= 0.1f;
		}
		else
		{
			Vector3 vector = position;
			vector.y += 0.5f;
			RaycastHit2D raycastHit2D = Physics2D.Raycast(vector, Vector2.down, 1f, GameLayers.EnvironmentMask);
			if (raycastHit2D.collider != null)
			{
				position.x = raycastHit2D.point.x;
				position.y = raycastHit2D.point.y;
			}
		}
		DynamicallySpawnedObject.Spawn(assetReference, persistent: false, position);
	}

	public void PlayJumpEffectEvent()
	{
		SurfaceSettings surfaceSettings = null;
		if (m_characterMovement != null)
		{
			surfaceSettings = m_characterMovement.CurrentSurfaceSettings;
		}
		PlayAudioEventWithSurfaceType(m_jumpAudio, base.transform.position, surfaceSettings);
	}

	public void PlayLandEffectEvent()
	{
		AudioEvent audioEvent = m_landAudio;
		SurfaceSettings surfaceSettings = null;
		if (m_characterMovement != null)
		{
			surfaceSettings = m_characterMovement.CurrentSurfaceSettings;
			if (surfaceSettings != null)
			{
				if (m_useSurfaceSounds && surfaceSettings.LandAudioEvent != null)
				{
					audioEvent = surfaceSettings.LandAudioEvent;
				}
				SpawnSurfaceParticleEffect(base.transform.position, surfaceSettings.LandEffectPrefab, m_characterMovement.CurrentSurfaceCollider);
			}
		}
		PlayAudioEventWithSurfaceType(audioEvent, base.transform.position, surfaceSettings);
	}

	public void PlayKnockedDownEffectEvent()
	{
		AudioEvent knockedDownAudio = m_knockedDownAudio;
		SurfaceSettings surfaceSettings = null;
		if (m_characterMovement != null)
		{
			surfaceSettings = m_characterMovement.CurrentSurfaceSettings;
		}
		PlayAudioEventWithSurfaceType(knockedDownAudio, base.transform.position, surfaceSettings);
	}

	public void PlayClimbEffectEvent()
	{
		if (m_climbAudio != null)
		{
			m_climbAudio.Play(base.transform.position);
		}
	}

	public void PlayLadderEffectEvent()
	{
		if (m_ladderAudio != null)
		{
			m_ladderAudio.Play(base.transform.position);
		}
	}

	public void PlayTakeHealthDamageEvent(int damage)
	{
		if (m_takeHealthDamageAudio != null)
		{
			m_takeHealthDamageAudio.Play(base.transform.position);
		}
	}

	public void PlayHitWeakpoint(int damage)
	{
		if (m_hitWeakpointDamageAudio != null)
		{
			m_hitWeakpointDamageAudio.Play(base.transform.position);
		}
	}

	public void PlayDeadEvent(DamageInstance damage)
	{
		if (m_deadAudio != null)
		{
			m_deadAudio.Play(base.transform.position);
		}
		OnWaterSurfaceChanged(null);
		if (m_characterMovement != null)
		{
			CharacterMovement characterMovement = m_characterMovement;
			characterMovement.m_onWaterSurfaceChanged = (UnityAction<SurfaceType>)Delegate.Remove(characterMovement.m_onWaterSurfaceChanged, new UnityAction<SurfaceType>(OnWaterSurfaceChanged));
		}
	}

	public void PlayMeleeWeaponSwingEvent()
	{
		CharacterEquipment component = GetComponent<CharacterEquipment>();
		if (component != null && component.EquippedMeleeWeaponItemInstance != null)
		{
			AudioEvent swingAudioEvent = component.EquippedMeleeWeaponItemInstance.WeaponDefinition.SwingAudioEvent;
			if (swingAudioEvent != null)
			{
				swingAudioEvent.Play(base.transform.position);
			}
		}
	}

	public void ApplyDamageInstance(DamageInstance instance)
	{
		CharacterHealth component = GetComponent<CharacterHealth>();
		if ((!(component != null) || !component.IsDead) && instance.IsDamagingHit())
		{
			if (instance.IsWeakpointDamage)
			{
				PlayHitWeakpoint(instance.HealthDamageAmount);
			}
			else
			{
				PlayTakeHealthDamageEvent(instance.HealthDamageAmount);
			}
		}
	}
}
