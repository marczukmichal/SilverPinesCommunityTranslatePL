using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using PowerTools;
using UnityEngine;
using UnityEngine.Events;

public class CharacterHitReact : MonoBehaviour, IDamageable
{
	[Serializable]
	private struct StaggerSettings
	{
		public float m_requiredStaggerBuildup;

		public string m_fsmEventName;
	}

	[SerializeField]
	private HitReactEffectSettings m_normalHitEffectSettings;

	[SerializeField]
	private HitReactEffectSettings m_weakPointHitEffectSettings;

	[SerializeField]
	private HitReactEffectSettings m_normalKilledEffectSettings;

	[SerializeField]
	private HitReactEffectSettings m_weakPointKilledEffectSettings;

	[SerializeField]
	private SpriteRenderer m_spriteRenderer;

	[SerializeField]
	private SpriteAnim m_spriteAnim;

	[Header("Sprite Hit React")]
	[SerializeField]
	private float m_reactAmplitude;

	[SerializeField]
	private float m_reactDuration = 0.05f;

	[Header("Stagger")]
	[SerializeField]
	private StaggerSettings[] m_staggerSettings;

	[SerializeField]
	private float m_staggerReductionRate = 40f;

	[SerializeField]
	private float m_maxStaggerBuildup = 100f;

	[DebugCommand("debug_hit_reacts", "Replaces hit react effects with a test asset which shows how they are being spawned", "debug_hit_reacts <true/false>", typeof(bool), false)]
	private static bool DEBUG_HIT_REACTS;

	[SerializeField]
	private AutoFaceDirectionOnHit m_autoFaceAttackOnHit;

	private Rigidbody2D m_rigidbody2D;

	private CharacterDirection m_direction;

	private CharacterHealth m_health;

	private CharacterDamageRage m_rage;

	private DamageInstance m_lastHit;

	private float m_lastHitTime;

	public UnityAction OnHit;

	private Dictionary<HitReactEffectSettings.HitReactEffectGroup, float> m_effectFireTimes;

	private bool m_hitReactsBlocked;

	private bool m_statelessHitReactEnabled;

	private float m_staggerBuildUp;

	private float m_staggerReductionBlockTimer;

	public AutoFaceDirectionOnHit AutoFaceAttackOnHit
	{
		get
		{
			return m_autoFaceAttackOnHit;
		}
		set
		{
			m_autoFaceAttackOnHit = value;
		}
	}

	public DamageInstance LastHit => m_lastHit;

	public bool HitReactsEventsBlocked
	{
		get
		{
			return m_hitReactsBlocked;
		}
		set
		{
			m_hitReactsBlocked = value;
		}
	}

	public bool StatelessHitReactEnabled
	{
		get
		{
			if (m_rage != null && m_rage.IsRaging)
			{
				return true;
			}
			return m_statelessHitReactEnabled;
		}
		set
		{
			m_statelessHitReactEnabled = value;
		}
	}

	public float StaggerBuildUp => m_staggerBuildUp;

	public string GetHitReactEvent()
	{
		StaggerSettings[] staggerSettings = m_staggerSettings;
		for (int i = 0; i < staggerSettings.Length; i++)
		{
			StaggerSettings staggerSettings2 = staggerSettings[i];
			if (m_staggerBuildUp > staggerSettings2.m_requiredStaggerBuildup)
			{
				return staggerSettings2.m_fsmEventName;
			}
		}
		return "OnHit";
	}

	private void Awake()
	{
		m_lastHitTime = -1000f;
		m_lastHit = null;
		m_rigidbody2D = GetComponent<Rigidbody2D>();
		m_direction = GetComponent<CharacterDirection>();
		m_effectFireTimes = new Dictionary<HitReactEffectSettings.HitReactEffectGroup, float>();
		m_health = GetComponent<CharacterHealth>();
		m_rage = GetComponent<CharacterDamageRage>();
	}

	public void ApplyDamageInstance(DamageInstance instance)
	{
		if (!instance.IsDamagingHit() && instance.DamageCategory != DamageCategory.Parry)
		{
			return;
		}
		int num;
		if (instance.HitFlags.HasFlag(HitFlags.SupressReaction) || HitReactsEventsBlocked)
		{
			num = ((instance.DamageCategory == DamageCategory.Parry) ? 1 : 0);
			if (num == 0)
			{
				goto IL_0055;
			}
		}
		else
		{
			num = 1;
		}
		m_lastHitTime = Time.time;
		m_lastHit = instance;
		goto IL_0055;
		IL_0055:
		m_staggerBuildUp += (float)instance.HealthDamageAmount * instance.StaggerMultiplier;
		if (instance.DamageCategory == DamageCategory.Parry)
		{
			m_staggerBuildUp = m_maxStaggerBuildup;
		}
		else
		{
			m_staggerBuildUp = Mathf.Min(m_staggerBuildUp, m_maxStaggerBuildup);
		}
		m_staggerReductionBlockTimer = 1f;
		if (instance.DamageCategory != DamageCategory.Parry)
		{
			TriggerHitEffects(instance, instance.IsWeakpointDamage ? m_weakPointHitEffectSettings : m_normalHitEffectSettings);
		}
		if (num != 0)
		{
			OnHit?.Invoke();
			if (m_direction != null && (m_autoFaceAttackOnHit == AutoFaceDirectionOnHit.Always || (m_autoFaceAttackOnHit == AutoFaceDirectionOnHit.OnlyOnKilled && instance.IsKillingBlow)))
			{
				if (instance.Direction.x < 0f && m_direction.CurrentDirection == CharacterDirection.Facing.Left)
				{
					m_direction.CurrentDirection = CharacterDirection.Facing.Right;
				}
				else if (instance.Direction.x > 0f && m_direction.CurrentDirection == CharacterDirection.Facing.Right)
				{
					m_direction.CurrentDirection = CharacterDirection.Facing.Left;
				}
			}
		}
		if (GameUtils.IsPlayer(base.gameObject))
		{
			GlobalReferences.Instance.EventChannels.Generic.GameSleep.Raise(TimeSlowType.PlayerHit);
		}
	}

	public static Quaternion GetImpactEffectRotationForDamageInstance(DamageInstance instance)
	{
		float num = Vector2.SignedAngle(Vector2.right, instance.Direction);
		float y = 0f;
		if (num > 90f)
		{
			y = 180f;
			num += -180f;
		}
		else if (num < -90f)
		{
			y = 180f;
			num += 180f;
		}
		return Quaternion.Euler(0f, y, num);
	}

	private void TriggerHitEffects(DamageInstance instance, HitReactEffectSettings effects)
	{
		if (effects == null)
		{
			return;
		}
		Quaternion impactEffectRotationForDamageInstance = GetImpactEffectRotationForDamageInstance(instance);
		HitReactEffectSettings.HitReactEffectGroup[] effectGroups = effects.m_effectGroups;
		foreach (HitReactEffectSettings.HitReactEffectGroup hitReactEffectGroup in effectGroups)
		{
			if (hitReactEffectGroup.m_supportedImpactTypes.HasFlag(instance.ImpactType))
			{
				float num = 0f;
				if (m_effectFireTimes.ContainsKey(hitReactEffectGroup))
				{
					num = m_effectFireTimes[hitReactEffectGroup];
				}
				if (num + hitReactEffectGroup.m_minTimeForRefire <= Time.time)
				{
					PlayEffectGroup(instance, base.transform, impactEffectRotationForDamageInstance, Vector3.one, hitReactEffectGroup);
				}
			}
		}
		bool flag = true;
		if (m_health != null && m_health.IsDead)
		{
			flag = false;
		}
		if (flag)
		{
			StartCoroutine(PerformHitReactCoroutine(instance.Direction.x < 0f));
		}
	}

	private IEnumerator PerformHitReactCoroutine(bool isRight)
	{
		bool isStatelessHitReact = StatelessHitReactEnabled;
		if (isStatelessHitReact)
		{
			if (m_spriteAnim != null)
			{
				m_spriteAnim.Speed = 0f;
			}
			if (m_spriteRenderer != null)
			{
				m_spriteRenderer.material.SetFloat("_HitReactRotateAmplitude", m_reactAmplitude);
				m_spriteRenderer.material.SetFloat("_HitReactRotateAmount", isRight ? 1f : (-1f));
				m_spriteRenderer.material.DOFloat(0f, "_HitReactRotateAmount", m_reactDuration);
			}
		}
		if (m_spriteRenderer != null)
		{
			m_spriteRenderer.material.SetFloat("_HitReactActive", 1f);
		}
		yield return new WaitForSeconds(m_reactDuration);
		if (m_spriteRenderer != null)
		{
			m_spriteRenderer.material.SetFloat("_HitReactActive", 0f);
		}
		if (isStatelessHitReact)
		{
			if (m_spriteRenderer != null)
			{
				DOTween.Kill(m_spriteRenderer.material);
				m_spriteRenderer.material.SetFloat("_HitReactRotateAmount", 0f);
			}
			if (m_spriteAnim != null)
			{
				m_spriteAnim.Speed = 1f;
			}
		}
	}

	private static void SpawnDecal(HitReactEffectSettings.HitReactDecal[] decals, Vector3 position, Quaternion rotation, Vector3 scale)
	{
		float num = 0f;
		HitReactEffectSettings.HitReactDecal[] array = decals;
		foreach (HitReactEffectSettings.HitReactDecal hitReactDecal in array)
		{
			num += hitReactDecal.m_weight;
		}
		float num2 = UnityEngine.Random.Range(0f, num);
		float num3 = 0f;
		array = decals;
		foreach (HitReactEffectSettings.HitReactDecal hitReactDecal2 in array)
		{
			num3 += hitReactDecal2.m_weight;
			if (num3 > num2)
			{
				Vector3 position2 = position;
				position2.z += hitReactDecal2.m_depthOffset;
				DynamicallySpawnedObject.Spawn(hitReactDecal2.m_prefabAssetReference, persistent: true, position2, rotation, scale);
				break;
			}
		}
	}

	public static void PlayEffectGroup(DamageInstance instance, Transform characterTransform, Quaternion rotation, Vector3 scale, HitReactEffectSettings.HitReactEffectGroup effectGroup)
	{
		if (effectGroup.m_effects != null)
		{
			float num = 0f;
			HitReactEffectSettings.HitReactEffect[] effects = effectGroup.m_effects;
			foreach (HitReactEffectSettings.HitReactEffect hitReactEffect in effects)
			{
				num += hitReactEffect.m_weight;
			}
			float num2 = UnityEngine.Random.Range(0f, num);
			float num3 = 0f;
			effects = effectGroup.m_effects;
			foreach (HitReactEffectSettings.HitReactEffect hitReactEffect2 in effects)
			{
				num3 += hitReactEffect2.m_weight;
				if (num3 > num2)
				{
					Vector3 position = instance.Position;
					position.z = characterTransform.position.z;
					position += new Vector3(instance.Direction.x, instance.Direction.y, 0f) * hitReactEffect2.m_offsetDistance;
					position.z -= 0.1f;
					DynamicallySpawnedObject.Spawn(DEBUG_HIT_REACTS ? GlobalReferences.Instance.TestImpactVFX : hitReactEffect2.m_reactEffectAsset, persistent: false, position, rotation, scale);
					break;
				}
			}
		}
		Vector3 vector = instance.Position;
		vector.z = characterTransform.position.z - 0.1f;
		if (effectGroup.m_wallDecals != null)
		{
			Quaternion identity = Quaternion.identity;
			SpawnDecal(effectGroup.m_wallDecals, vector, identity, scale);
		}
		if (effectGroup.m_floorDecals != null)
		{
			RaycastHit2D raycastHit2D = Physics2D.Raycast(vector, Vector2.down, 1.5f, GameLayers.EnvironmentMask);
			if ((bool)raycastHit2D)
			{
				Quaternion identity2 = Quaternion.identity;
				identity2 *= Quaternion.Euler(90f, UnityEngine.Random.Range(0f, 360f), 0f);
				Vector3 position2 = raycastHit2D.point;
				position2.y += 0.1f;
				SpawnDecal(effectGroup.m_floorDecals, position2, identity2, scale);
			}
		}
		if (effectGroup.m_gibletPieces == null)
		{
			return;
		}
		for (int j = 0; j < effectGroup.m_gibletPiecesToSpawn; j++)
		{
			float num4 = 0f;
			HitReactEffectSettings.HitReactGibletPiece[] gibletPieces = effectGroup.m_gibletPieces;
			foreach (HitReactEffectSettings.HitReactGibletPiece hitReactGibletPiece in gibletPieces)
			{
				num4 += hitReactGibletPiece.m_weight;
			}
			float num5 = UnityEngine.Random.Range(0f, num4);
			float num6 = 0f;
			gibletPieces = effectGroup.m_gibletPieces;
			foreach (HitReactEffectSettings.HitReactGibletPiece hitReactGibletPiece2 in gibletPieces)
			{
				num6 += hitReactGibletPiece2.m_weight;
				if (!(num6 > num5))
				{
					continue;
				}
				Vector3 position3 = instance.Position;
				position3.z = characterTransform.position.z;
				position3.z += UnityEngine.Random.Range(-0.5f, 0.5f);
				Quaternion rotation2 = Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(0f, 360f));
				DynamicallySpawnObjectEventData eventData = new DynamicallySpawnObjectEventData(hitReactGibletPiece2.m_assetReference, persistent: true, position3, rotation2, scale);
				ref UnityAction<GameObject> onSpawnedAction = ref eventData.m_onSpawnedAction;
				onSpawnedAction = (UnityAction<GameObject>)Delegate.Combine(onSpawnedAction, (UnityAction<GameObject>)delegate(GameObject spawnedObject)
				{
					Rigidbody2D component = spawnedObject.GetComponent<Rigidbody2D>();
					if (component != null)
					{
						float num7 = ((UnityEngine.Random.Range(0f, 1f) > 0.8f) ? UnityEngine.Random.Range(-0.25f, 0.25f) : UnityEngine.Random.Range(0.5f, 1f));
						Vector2 force = instance.Direction * instance.ImpactForce * num7;
						component.AddForce(force);
					}
				});
				DynamicallySpawnedObject.Spawn(eventData);
				break;
			}
		}
	}

	public void DoKillEffect(DamageInstance instance, bool isWeakpoint)
	{
		TriggerHitEffects(instance, isWeakpoint ? m_weakPointKilledEffectSettings : m_normalKilledEffectSettings);
	}

	public bool ApplyLastDamageImpactForce(bool forceLookAtHitSource)
	{
		if (m_lastHit == null)
		{
			Debug.LogWarning("Tried to apply a last hit but none are set!");
			return false;
		}
		if (m_lastHit.ImpactReactDirection == ImpactReactDirection.ForceLookAtSource)
		{
			forceLookAtHitSource = true;
		}
		if (m_direction != null)
		{
			if (forceLookAtHitSource || m_autoFaceAttackOnHit == AutoFaceDirectionOnHit.Always || (m_autoFaceAttackOnHit == AutoFaceDirectionOnHit.OnlyOnKilled && m_lastHit.IsKillingBlow))
			{
				if (m_lastHit.Direction.x < 0f && m_direction.CurrentDirection == CharacterDirection.Facing.Left)
				{
					m_direction.CurrentDirection = CharacterDirection.Facing.Right;
				}
				else if (m_lastHit.Direction.x > 0f && m_direction.CurrentDirection == CharacterDirection.Facing.Right)
				{
					m_direction.CurrentDirection = CharacterDirection.Facing.Left;
				}
			}
			else if (m_lastHit.ImpactReactDirection == ImpactReactDirection.AlongCharacterDirection)
			{
				m_direction.CurrentDirection = m_lastHit.DamageSource.GetComponent<CharacterDirection>().CurrentDirection;
			}
			else if (m_lastHit.ImpactReactDirection == ImpactReactDirection.OppositeCharacterDirection)
			{
				m_direction.CurrentDirection = CharacterDirection.GetOppositeDirection(m_lastHit.DamageSource.GetComponent<CharacterDirection>().CurrentDirection);
			}
		}
		return true;
	}

	public bool HasBeenHit()
	{
		return Time.time - m_lastHitTime < 0.2f;
	}

	public void ClearHitFlag()
	{
		m_lastHitTime = -1f;
	}

	public bool LastHitWasBlocked()
	{
		if (m_lastHit != null)
		{
			return m_lastHit.IsMeleeBlocked;
		}
		return false;
	}

	public void ClearStagger()
	{
		m_staggerBuildUp = 0f;
	}

	private void Update()
	{
		if (m_staggerReductionBlockTimer > 0f)
		{
			m_staggerReductionBlockTimer -= Time.deltaTime;
		}
		else if (m_staggerBuildUp > 0f)
		{
			m_staggerBuildUp -= Time.deltaTime * m_staggerReductionRate;
			m_staggerBuildUp = Mathf.Max(0f, m_staggerBuildUp);
		}
	}
}
