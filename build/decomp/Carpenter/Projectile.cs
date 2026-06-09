using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
	[SerializeField]
	[HideInInspector]
	private ProjectileSettings m_settings;

	[SerializeField]
	private float m_startingLifeTime = 10f;

	[SerializeField]
	private GameObject m_projectileVisuals;

	[SerializeField]
	private bool m_environmentOnlyCollision;

	private DynamicallySpawnedObject m_dynamicallySpawnedObject;

	private Vector2 m_velocity;

	private float m_angularVelocity;

	private float m_lifeTime;

	private GameObject m_source;

	private CharacterIdentifier.CharacterFaction m_sourceFaction;

	private float m_flatDamageScalar;

	private float m_penetrationDamageScalar;

	private bool m_disableImpactEffects;

	private bool m_projectileDone;

	private bool m_projectileDeflected;

	private List<Collider2D> ignoreColliders;

	private float m_distanceTravelled;

	private TrailRenderer m_trailRenderer;

	private float m_defaultTrailRendererTime;

	private bool m_alreadyHitPassable;

	private void Awake()
	{
		m_trailRenderer = GetComponentInChildren<TrailRenderer>();
		if ((bool)m_trailRenderer)
		{
			m_defaultTrailRendererTime = m_trailRenderer.time;
		}
	}

	private void Start()
	{
		m_dynamicallySpawnedObject = GetComponent<DynamicallySpawnedObject>();
	}

	private void OnEnable()
	{
		m_alreadyHitPassable = false;
	}

	public void SetVisualsActive(bool active)
	{
		if (m_projectileVisuals != null)
		{
			m_projectileVisuals.SetActive(active);
		}
	}

	public void Fire(Vector3 direction, ProjectileSettings settings, GameObject source, float speed, float angularVelocity = 0f, float damageScalar = 1f)
	{
		SetVisualsActive(active: true);
		if (m_trailRenderer != null)
		{
			m_trailRenderer.Clear();
			m_trailRenderer.emitting = true;
			m_trailRenderer.minVertexDistance = 10000000f;
			m_trailRenderer.time = m_defaultTrailRendererTime;
			m_trailRenderer.AddPosition(base.transform.position);
		}
		Rigidbody2D component = GetComponent<Rigidbody2D>();
		if (component != null)
		{
			component.bodyType = RigidbodyType2D.Static;
		}
		base.gameObject.layer = GameLayers.ProjectileLayer;
		m_penetrationDamageScalar = 1f;
		m_flatDamageScalar = damageScalar;
		m_projectileDone = false;
		m_projectileDeflected = false;
		m_distanceTravelled = 0f;
		m_disableImpactEffects = false;
		m_lifeTime = m_startingLifeTime;
		m_settings = settings;
		m_source = source;
		if (source != null)
		{
			CharacterIdentifier component2 = source.GetComponent<CharacterIdentifier>();
			if (component2 != null)
			{
				m_sourceFaction = component2.Faction;
			}
		}
		m_velocity = direction * speed;
		m_angularVelocity = angularVelocity;
		ignoreColliders = new List<Collider2D>(source.GetComponentsInChildren<Collider2D>());
	}

	private void RemoveProjectile()
	{
		SetVisualsActive(active: false);
		m_projectileDone = true;
		m_lifeTime = 1f;
	}

	private void Update()
	{
		m_lifeTime -= Time.deltaTime;
		if (m_lifeTime < 0f)
		{
			if (m_settings.LifeTimeExpiredExplosion != null)
			{
				Explosion.SpawnExplosion(m_settings.LifeTimeExpiredExplosion, base.transform.position, Quaternion.identity, m_source);
			}
			if (m_trailRenderer != null)
			{
				m_trailRenderer.Clear();
			}
			m_dynamicallySpawnedObject.Release();
		}
	}

	private void FixedUpdate()
	{
		if (m_projectileDone)
		{
			return;
		}
		Vector2 vector = Physics2D.gravity * Time.deltaTime * m_settings.GravityScalar;
		m_velocity += vector;
		Vector3 vector2 = m_velocity * Time.deltaTime;
		base.transform.Rotate(0f, 0f, m_angularVelocity * Time.deltaTime);
		float z = base.transform.position.z;
		if (m_source != null)
		{
			z = m_source.transform.position.z;
		}
		float minDepth = z - m_settings.DepthImpactRange;
		float maxDepth = z + m_settings.DepthImpactRange;
		int layerMask = (m_environmentOnlyCollision ? GameLayers.EnvironmentMask : GameLayers.ProjectileMask);
		RaycastHit2D[] array = ((!(m_settings.SizeRadius <= 0f)) ? Physics2D.CircleCastAll(base.transform.position, m_settings.SizeRadius, vector2.normalized, vector2.magnitude, layerMask, minDepth, maxDepth) : Physics2D.LinecastAll(base.transform.position, base.transform.position + vector2, layerMask, minDepth, maxDepth));
		List<RaycastHit2D> list = new List<RaycastHit2D>();
		RaycastHit2D[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			RaycastHit2D item = array2[i];
			if (!(item.collider.gameObject == m_source) && !ignoreColliders.Contains(item.collider))
			{
				CharacterIdentifier componentInParent = item.collider.GetComponentInParent<CharacterIdentifier>();
				if ((!(componentInParent != null) || !(componentInParent.gameObject == m_source)) && (m_settings.HitPlatformEffectors || !item.collider.GetComponent<PlatformEffector2D>()) && item.collider != null)
				{
					list.Add(item);
				}
			}
		}
		List<GameObject> list2 = new List<GameObject>();
		bool flag = false;
		foreach (RaycastHit2D item3 in list)
		{
			if (list2.Contains(item3.collider.gameObject))
			{
				continue;
			}
			float time = m_distanceTravelled + item3.distance;
			float num = m_settings.RangeDamageFalloff.Evaluate(time);
			if (num < float.Epsilon)
			{
				break;
			}
			bool flag2 = false;
			bool flag3 = true;
			IDamageable component = item3.collider.gameObject.GetComponent<IDamageable>();
			IDamageable componentInParent2 = item3.collider.gameObject.GetComponentInParent<IDamageable>();
			CharacterIdentifier componentInParent3 = item3.collider.GetComponentInParent<CharacterIdentifier>();
			if ((bool)componentInParent3)
			{
				if (componentInParent3.Faction == m_sourceFaction)
				{
					continue;
				}
				DamageBlock component2 = componentInParent3.GetComponent<DamageBlock>();
				if (component2 != null && !component2.CanTakeDamage(null))
				{
					continue;
				}
			}
			bool flag4 = false;
			bool flag5 = item3.collider.gameObject.layer == GameLayers.WaterLayer;
			if (!m_projectileDeflected)
			{
				if (component != null)
				{
					flag4 = component.ShouldDeflectHit(m_settings.IsArmorPiercing);
				}
				else if (componentInParent2 != null)
				{
					flag4 = componentInParent2.ShouldDeflectHit(m_settings.IsArmorPiercing);
				}
			}
			if (componentInParent3 != null)
			{
				Collider2D[] componentsInChildren = componentInParent3.GetComponentsInChildren<Collider2D>();
				foreach (Collider2D collider2D in componentsInChildren)
				{
					if (collider2D.gameObject != item3.collider.gameObject)
					{
						list2.Add(collider2D.gameObject);
					}
				}
			}
			if (componentInParent2 != null)
			{
				flag2 = componentInParent2.AllowPassThroughProjectile();
				flag3 = componentInParent2.AllowProjectilePenetration();
			}
			else if (!flag5)
			{
				flag3 = false;
			}
			if (flag2 && m_alreadyHitPassable)
			{
				continue;
			}
			if (flag2)
			{
				m_alreadyHitPassable = true;
			}
			Vector3 impactPoint = item3.point;
			impactPoint.z = base.transform.position.z;
			float num2 = (flag4 ? 0f : (m_penetrationDamageScalar * num));
			num2 *= m_flatDamageScalar;
			bool flag6 = true;
			if (m_settings.PersistAfterImpact && (item3.collider.gameObject.layer == GameLayers.EnvironmentLayer || item3.collider.gameObject.layer == GameLayers.DefaultLayer || item3.collider.gameObject.layer == GameLayers.EnvironmentNoFogOfWarLayer))
			{
				flag6 = false;
			}
			if (flag6)
			{
				ignoreColliders.Add(item3.collider);
			}
			if (!m_disableImpactEffects)
			{
				ProjectileUtils.ProjectileImpact(m_source, item3.collider.gameObject, m_settings, num2, impactPoint, vector2.normalized * -1f, flag2);
				if (m_settings.PersistAfterImpact)
				{
					m_disableImpactEffects = true;
				}
			}
			if (flag4)
			{
				Collider2D[] componentsInChildren = item3.collider.GetComponentsInParent<Collider2D>();
				foreach (Collider2D item2 in componentsInChildren)
				{
					ignoreColliders.Add(item2);
				}
				m_projectileDeflected = true;
				m_velocity.x *= 0f - m_settings.DeflectedShotSpeedScale;
				m_velocity.y += Random.Range(0f - m_velocity.magnitude, m_velocity.magnitude);
				m_trailRenderer.time = m_defaultTrailRendererTime / m_settings.DeflectedShotSpeedScale;
				Vector3 position = item3.point;
				position.z = base.transform.position.z;
				base.transform.position = position;
				if (m_trailRenderer != null)
				{
					m_trailRenderer.AddPosition(base.transform.position);
				}
				flag = true;
				break;
			}
			if (flag3)
			{
				if (!flag2 && !flag5)
				{
					bool flag7 = false;
					float num3 = m_settings.TargetPenetrationChance;
					float num4 = m_settings.TargetPenetration;
					if (m_source != null)
					{
						CharacterInventory component3 = m_source.GetComponent<CharacterInventory>();
						if (component3 != null && component3.Inventory.HasActiveArtifactEffect(GlobalReferences.Instance.ArtifactEffects.Generic.WeaponPenetrationChance, out float floatValue))
						{
							num3 = Mathf.Max(num3, floatValue);
							num4 = Mathf.Max(num3, 0.5f);
						}
					}
					if (num3 < 1f && Random.Range(0f, 1f) > num3)
					{
						flag7 = true;
					}
					if (flag7)
					{
						m_penetrationDamageScalar = 0f;
					}
					else
					{
						m_penetrationDamageScalar *= num4;
					}
				}
			}
			else
			{
				m_penetrationDamageScalar = 0f;
			}
			if (!(m_penetrationDamageScalar < ProjectileUtils.s_minimumDamageScalar))
			{
				continue;
			}
			Vector3 position2 = item3.point;
			position2.z = base.transform.position.z;
			if (m_settings.PersistAfterImpact)
			{
				Vector3 vector3 = item3.normal;
				vector3 *= 0.1f;
				position2 += vector3;
				position2.z -= 0.2f;
			}
			base.transform.position = position2;
			if (m_settings.ImpactAudio != null)
			{
				m_settings.ImpactAudio.Play(base.transform.position);
			}
			if (m_settings.SpawnOnImpactEffect.HasAsset())
			{
				DynamicallySpawnedObject.Spawn(m_settings.SpawnOnImpactEffect, persistent: false, base.transform.position);
			}
			if (m_settings.ResiduePuddle.HasAsset())
			{
				DynamicallySpawnedObject.Spawn(m_settings.ResiduePuddle, persistent: false, base.transform.position);
			}
			if (m_settings.PersistAfterImpact)
			{
				m_projectileDone = true;
				Rigidbody2D component4 = GetComponent<Rigidbody2D>();
				if (component4 != null)
				{
					Vector3 vector4 = Vector3.Reflect(m_velocity, item3.normal) * m_settings.InitialImpactDampening;
					component4.bodyType = RigidbodyType2D.Dynamic;
					component4.linearVelocity = vector4;
					component4.angularVelocity = m_angularVelocity;
					base.gameObject.layer = GameLayers.GameplayElementsLayer;
				}
			}
			else
			{
				RemoveProjectile();
			}
			break;
		}
		if (!m_projectileDone && !flag)
		{
			m_distanceTravelled += vector2.magnitude;
			base.transform.position += vector2;
			if (m_trailRenderer != null)
			{
				m_trailRenderer.AddPosition(base.transform.position);
			}
		}
	}
}
