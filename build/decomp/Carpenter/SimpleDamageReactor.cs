using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;

[DisallowMultipleComponent]
[ShowInDesignerInspector]
public class SimpleDamageReactor : MonoBehaviour, IDamageable
{
	[Header("Spawned Effect")]
	[SerializeField]
	private AssetReferenceGameObject m_spawnAsset;

	[SerializeField]
	private Vector3 m_spawnedObjectOffset;

	[Header("Settings")]
	[SerializeField]
	private bool m_respondToMelee = true;

	[SerializeField]
	private bool m_respondToProjectiles = true;

	[SerializeField]
	private bool m_respondToExplosions = true;

	[SerializeField]
	private int m_minimumDamage = 1;

	[SerializeField]
	private float m_minRetriggerTime = 0.1f;

	[SerializeField]
	private bool m_projectilePassthrough = true;

	[SerializeField]
	private bool m_canPenetrate = true;

	[SerializeField]
	private ConsumeHitType m_consumeHit;

	[Header("Events")]
	[SerializeField]
	private UnityEvent m_onHitEvent;

	private float m_lastHitTime;

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
		return !m_projectilePassthrough;
	}

	public ConsumeHitType GetConsumeHitType()
	{
		return m_consumeHit;
	}

	public void ApplyDamageInstance(DamageInstance damageInstance)
	{
		if (!damageInstance.IsTouchDamage && damageInstance.HealthDamageAmount >= m_minimumDamage && (damageInstance.DamageCategory != DamageCategory.DamageCollider || m_respondToMelee) && (damageInstance.DamageCategory != DamageCategory.Projectile || m_respondToProjectiles) && (damageInstance.DamageCategory != DamageCategory.Explosion || m_respondToExplosions) && !(Time.time < m_lastHitTime + m_minRetriggerTime))
		{
			TriggerHit(damageInstance);
		}
	}

	private void TriggerHit(DamageInstance damageInstance)
	{
		m_onHitEvent?.Invoke();
		Vector3 position = base.transform.position + m_spawnedObjectOffset;
		Quaternion identity = Quaternion.identity;
		Vector3 one = Vector3.one;
		float num = Vector2.SignedAngle(Vector2.right, damageInstance.Direction);
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
		identity = Quaternion.Euler(0f, 0f, num);
		if (m_spawnAsset != null && m_spawnAsset.HasAsset())
		{
			DynamicallySpawnedObject.Spawn(m_spawnAsset, persistent: false, position, identity, one);
		}
	}
}
