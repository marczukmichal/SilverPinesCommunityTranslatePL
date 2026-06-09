using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.Serialization;

[DisallowMultipleComponent]
[ShowInDesignerInspector]
public class SimpleHealth : MonoBehaviour, IDamageable, IPersistentComponent
{
	[Serializable]
	private class DamageStates
	{
		[SerializeField]
		public int m_health;

		[FormerlySerializedAs("m_event")]
		[SerializeField]
		public UnityEvent m_persistentEvent;

		[SerializeField]
		public UnityEvent m_transitionEvent;

		[SerializeField]
		public GameObject m_enabledChildObject;

		[Header("Spawned Object")]
		[SerializeField]
		public AssetReferenceGameObject m_spawnedAsset;

		[SerializeField]
		public Vector3 m_spawnedObjectOffset;

		[SerializeField]
		public bool m_matchHitPosition;

		[FormerlySerializedAs("m_spanedObjectRotateWithImpactDirection")]
		[SerializeField]
		public bool m_rotateWithHitDirection;

		[Header("Deprecated")]
		[SerializeField]
		public GameObject m_spawnGameObject;
	}

	[Serializable]
	private class PersistentData
	{
		public int m_storedHealth;
	}

	[SerializeField]
	private int m_health = 100;

	[SerializeField]
	private HitReactEffectSettings m_onHitEffect;

	[SerializeField]
	private AudioEvent m_onTakeDamageAudioEvent;

	[SerializeField]
	private AudioEvent m_onDeadAudioEvent;

	[SerializeField]
	private DamageStates[] m_damageStates;

	[SerializeField]
	private bool m_projectilePassthrough;

	[SerializeField]
	private bool m_canPenetrate = true;

	[SerializeField]
	private ConsumeHitType m_consumeHit;

	[Header("Physics")]
	[SerializeField]
	private Rigidbody2D m_hitReactRigidbody;

	[Header("Damage Categories")]
	[SerializeField]
	private DamageCategory m_onlyCategories;

	[Header("Special")]
	[SerializeField]
	private bool m_ignoreDamageIfSourceIsBehind;

	private int m_activeDamageState = -1;

	private DamageInstance m_previousDamageInstance;

	private PersistentDataObject m_persistentDataObject;

	private PersistentData m_persistentData;

	public UnityAction OnDeadEvent;

	public int Health => m_health;

	public bool IsDead => (float)Health <= 0f;

	bool IDamageable.AllowPassThroughProjectile()
	{
		return m_projectilePassthrough;
	}

	bool IDamageable.AllowProjectilePenetration()
	{
		return m_canPenetrate;
	}

	bool IDamageable.ShouldConsumeMeleeHit()
	{
		if (!m_projectilePassthrough)
		{
			return m_health > 0;
		}
		return false;
	}

	public ConsumeHitType GetConsumeHitType()
	{
		return m_consumeHit;
	}

	public bool ShouldIgnoreDamageCategory(DamageCategory category)
	{
		if (m_onlyCategories != 0 && !m_onlyCategories.HasFlag(category))
		{
			return true;
		}
		return false;
	}

	public bool RequiresPersistentData()
	{
		return true;
	}

	public void ReceiveDataStoreEntry(PersistentDataObject dataEntry)
	{
		m_persistentDataObject = dataEntry;
		m_persistentData = ((m_persistentDataObject.Data != null) ? (m_persistentDataObject.Data as PersistentData) : null);
		if (m_persistentData != null)
		{
			SetHealth(m_persistentData.m_storedHealth, isReload: true);
			return;
		}
		m_persistentData = new PersistentData();
		m_persistentDataObject.Data = m_persistentData;
		m_persistentData.m_storedHealth = m_health;
	}

	private void SetHealth(int value, bool isReload = false)
	{
		int num = Mathf.Max(value, 0);
		if (m_health != num)
		{
			m_health = num;
			EnableDamageState(GetDamageStateIndexForHealth(), isReload);
			if (m_persistentDataObject != null)
			{
				m_persistentData.m_storedHealth = m_health;
			}
			if (num <= 0 && !isReload)
			{
				OnDeadEvent?.Invoke();
			}
		}
	}

	private void EnableDamageState(int index, bool isReload = false)
	{
		if (index == m_activeDamageState)
		{
			return;
		}
		for (int i = 0; i < m_damageStates.Length; i++)
		{
			if (m_damageStates[i].m_enabledChildObject != null)
			{
				m_damageStates[i].m_enabledChildObject.SetActive(i == index);
			}
			if (i != index)
			{
				continue;
			}
			if (!isReload)
			{
				if (m_damageStates[i].m_spawnedAsset != null && m_damageStates[i].m_spawnedAsset.HasAsset())
				{
					Vector3 position;
					if (m_damageStates[i].m_matchHitPosition)
					{
						position = m_previousDamageInstance.Position;
						position.z = base.transform.position.z;
						position += m_damageStates[i].m_spawnedObjectOffset;
					}
					else
					{
						position = base.transform.position + m_damageStates[i].m_spawnedObjectOffset;
					}
					Quaternion rotation = Quaternion.identity;
					Vector3 one = Vector3.one;
					if (m_previousDamageInstance != null && m_damageStates[i].m_rotateWithHitDirection)
					{
						float num = Vector2.SignedAngle(Vector2.right, m_previousDamageInstance.Direction);
						if (num > 90f)
						{
							one.x = -1f;
							num += -180f;
						}
						else if (num < -90f)
						{
							one.x = -1f;
							num += 180f;
						}
						rotation = Quaternion.Euler(0f, 0f, num);
					}
					DynamicallySpawnedObject.Spawn(m_damageStates[i].m_spawnedAsset, persistent: false, position, rotation, one);
				}
				else if (m_damageStates[i].m_spawnGameObject != null)
				{
					Debug.LogWarning("Gameobject " + base.gameObject.name + " is still using deprecated non-object pool spawned object! Please use 'spawnedAsset' instead of 'spawnGameObject'", this);
					Vector3 position2 = base.transform.position + m_damageStates[i].m_spawnedObjectOffset;
					Quaternion rotation2 = Quaternion.identity;
					Vector3 one2 = Vector3.one;
					if (m_previousDamageInstance != null && m_damageStates[i].m_rotateWithHitDirection)
					{
						float num2 = Vector2.SignedAngle(Vector2.right, m_previousDamageInstance.Direction);
						if (num2 > 90f)
						{
							one2.x = -1f;
							num2 += -180f;
						}
						else if (num2 < -90f)
						{
							one2.x = -1f;
							num2 += 180f;
						}
						rotation2 = Quaternion.Euler(0f, 0f, num2);
					}
					GameObject obj = UnityEngine.Object.Instantiate(m_damageStates[i].m_spawnGameObject, position2, rotation2);
					obj.transform.localScale = one2;
					ParticleSystem component = obj.GetComponent<ParticleSystem>();
					if (component != null)
					{
						component.Play();
					}
				}
			}
			if (!isReload)
			{
				m_damageStates[i].m_transitionEvent.Invoke();
			}
			m_damageStates[i].m_persistentEvent.Invoke();
		}
		m_activeDamageState = index;
	}

	private int GetDamageStateIndexForHealth()
	{
		int num = int.MaxValue;
		int result = -1;
		for (int i = 0; i < m_damageStates.Length; i++)
		{
			if (m_health <= m_damageStates[i].m_health && m_damageStates[i].m_health < num)
			{
				num = m_damageStates[i].m_health;
				result = i;
			}
		}
		return result;
	}

	private bool ShouldIgnoreDamageInstance(DamageInstance instance)
	{
		if (instance.IsTouchDamage)
		{
			return true;
		}
		if (m_ignoreDamageIfSourceIsBehind)
		{
			if (instance.DamageSource == null)
			{
				return true;
			}
			bool num = instance.DamageSource.transform.position.x > base.transform.position.x;
			bool flag = base.transform.lossyScale.x > 0f;
			if (num != flag)
			{
				return true;
			}
		}
		return false;
	}

	public void ApplyDamageInstance(DamageInstance instance)
	{
		if (ShouldIgnoreDamageInstance(instance))
		{
			return;
		}
		m_previousDamageInstance = instance;
		int healthDamageAmount = instance.HealthDamageAmount;
		int health = Health;
		SetHealth(Health - healthDamageAmount);
		if (Health <= 0 && health > 0)
		{
			if (m_onDeadAudioEvent != null)
			{
				m_onDeadAudioEvent.Play(instance.Position);
			}
		}
		else if (healthDamageAmount > 0 && m_onTakeDamageAudioEvent != null)
		{
			m_onTakeDamageAudioEvent.Play(instance.Position);
		}
		PerformHitReact(instance, m_onHitEffect);
		if (m_hitReactRigidbody != null)
		{
			Vector2 vector = base.transform.position - instance.DamageSource.transform.position;
			vector.y = 0.5f;
			vector.Normalize();
			m_hitReactRigidbody.AddForceAtPosition(vector * instance.ImpactForce, instance.Position);
		}
	}

	public void Kill()
	{
		DamageInstance damageInstance = new DamageInstance();
		damageInstance.SetHealthDamage(m_health);
		ApplyDamageInstance(damageInstance);
	}

	private void PerformHitReact(DamageInstance instance, HitReactEffectSettings effects)
	{
		if (effects == null || instance.HealthDamageAmount <= 0)
		{
			return;
		}
		Quaternion impactEffectRotationForDamageInstance = CharacterHitReact.GetImpactEffectRotationForDamageInstance(instance);
		HitReactEffectSettings.HitReactEffectGroup[] effectGroups = effects.m_effectGroups;
		foreach (HitReactEffectSettings.HitReactEffectGroup hitReactEffectGroup in effectGroups)
		{
			if (hitReactEffectGroup.m_supportedImpactTypes.HasFlag(instance.ImpactType))
			{
				CharacterHitReact.PlayEffectGroup(instance, base.transform, impactEffectRotationForDamageInstance, Vector3.one, hitReactEffectGroup);
			}
		}
	}
}
