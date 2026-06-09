using System.Collections;
using UnityEngine;

public class Explosion : MonoBehaviour
{
	[SerializeField]
	private CameraShakeEventChannel m_cameraShakeEventChannel;

	[SerializeField]
	private AudioEvent m_audioEvent;

	private ExplosionSettings m_settings;

	private GameObject m_source;

	public void SetExplosionSettings(ExplosionSettings settings, GameObject source)
	{
		m_settings = settings;
		m_source = source;
	}

	public void Explode()
	{
		StartCoroutine(DelayedExplosion());
	}

	public IEnumerator DelayedExplosion()
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		if (m_settings.CameraShake != null)
		{
			CameraShakeEventData value = new CameraShakeEventData
			{
				m_cameraShakeSettings = m_settings.CameraShake,
				m_position = base.transform.position,
				m_direction = Vector3.up
			};
			m_cameraShakeEventChannel.Raise(value);
		}
		if (m_audioEvent != null)
		{
			m_audioEvent.Play(base.transform.position);
		}
		float minDepth = base.transform.position.z - m_settings.MaxDistance;
		float maxDepth = base.transform.position.z + m_settings.MaxDistance;
		RaycastHit2D[] array = Physics2D.CircleCastAll(base.transform.position, m_settings.MaxDistance, Vector2.up, 0.1f, GameLayers.ProjectileMask, minDepth, maxDepth);
		for (int i = 0; i < array.Length; i++)
		{
			RaycastHit2D raycastHit2D = array[i];
			Vector2 point = raycastHit2D.point;
			Vector2 vector = raycastHit2D.normal * -1f;
			DamageModifiers damageModifiers = raycastHit2D.collider.GetComponent<DamageModifiers>();
			if (damageModifiers == null)
			{
				damageModifiers = raycastHit2D.collider.GetComponentInParent<DamageModifiers>();
			}
			CharacterIdentifier componentInParent = raycastHit2D.transform.GetComponentInParent<CharacterIdentifier>();
			CharacterIdentifier.CharacterFaction faction = CharacterIdentifier.CharacterFaction.Unknown;
			if (componentInParent != null)
			{
				faction = componentInParent.Faction;
			}
			DamageInstance damageInstance = new DamageInstance().PopulateFromHitSettings(m_settings.HitSettings).SetDamageSource(m_source).SetDirection(vector)
				.SetDamageCategory(DamageCategory.Explosion)
				.SetPosition(point)
				.SetStatusEffectHitResults(m_settings.HitSettings.StatusEffect.GenerateResult(isBlocked: false))
				.ScaleDamageBasedOnFaction(faction)
				.ApplyDamageModifiers(damageModifiers);
			DamageBlock componentInParent2 = raycastHit2D.transform.GetComponentInParent<DamageBlock>();
			if (!componentInParent2 || componentInParent2.CanTakeDamage(damageInstance))
			{
				CharacterHealth componentInParent3 = raycastHit2D.transform.GetComponentInParent<CharacterHealth>();
				if (componentInParent3 != null)
				{
					damageInstance.SetIsKillingBlow(componentInParent3.IsKillingBlow(damageInstance));
				}
				DamageUtilities.ApplyDamage(raycastHit2D.transform.GetComponentsInParent<IDamageable>(), damageInstance);
				bool flag = componentInParent != null;
				if (raycastHit2D.rigidbody != null && !flag)
				{
					Vector2 vector2 = vector;
					vector2.Normalize();
					Vector3 vector3 = m_settings.HitSettings.ImpactForce * vector2;
					raycastHit2D.rigidbody.AddForceAtPosition(vector3, point);
				}
			}
		}
		if (m_settings.SpawnFireAmount > 0)
		{
			GlobalReferences.Instance.EventChannels.Fire.SpawnFire.Raise(new SpawnFireEventData
			{
				m_position = base.transform.position,
				m_amount = m_settings.SpawnFireAmount,
				m_spread = m_settings.MaxDistance
			});
		}
	}

	private void OnDrawGizmos()
	{
		if (m_settings != null)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(base.transform.position, m_settings.MinDistance);
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(base.transform.position, m_settings.MaxDistance);
			Gizmos.color = Color.white;
		}
	}

	public static void SpawnExplosion(ExplosionSettings explosion, Vector3 position, Quaternion rotation, GameObject source)
	{
		if (explosion.ExplosionAsset != null && explosion.ExplosionAsset.HasAsset())
		{
			DynamicallySpawnObjectEventData eventData = default(DynamicallySpawnObjectEventData);
			eventData.m_assetReference = explosion.ExplosionAsset;
			eventData.m_position = position;
			eventData.m_rotation = rotation;
			eventData.m_scale = Vector3.one;
			eventData.m_onSpawnedAction = delegate(GameObject explosionGO)
			{
				Explosion component = explosionGO.GetComponent<Explosion>();
				component.SetExplosionSettings(explosion, source);
				component.Explode();
			};
			DynamicallySpawnedObject.Spawn(eventData);
		}
		else
		{
			Debug.LogError("Explosion " + explosion.name + " doesn't have an explosion asset set!");
		}
	}
}
