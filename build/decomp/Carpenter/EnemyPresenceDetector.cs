using System;
using UnityEngine;
using UnityEngine.Events;

public class EnemyPresenceDetector : MonoBehaviour
{
	[SerializeField]
	private Vector2 m_regularRange;

	[SerializeField]
	private Vector2 m_artifactEnabledRange;

	[SerializeField]
	private AnimationCurve m_effectDistanceCurve;

	[SerializeField]
	private FloatVariable m_enemyPresence;

	[SerializeField]
	private float m_fallRate = 0.1f;

	private GameObject m_player;

	private CharacterHealth m_health;

	private CharacterInventory m_inventory;

	private float m_damageBump;

	private float m_currentTargetValue;

	[Header("Damage Bump")]
	[SerializeField]
	private float m_damageBumpScalar = 0.01f;

	[SerializeField]
	private float m_damageBumpReductionRate = 0.01f;

	[SerializeField]
	private float m_persistentLowHealthScalar = 2f;

	[SerializeField]
	private float m_aboutToDieHealthPercentage = 0.25f;

	private void OnEnable()
	{
		m_enemyPresence.Value = 0f;
		GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Register(AttachToPlayer);
		AttachToPlayer(GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Unregister(AttachToPlayer);
	}

	private void AttachToPlayer(GameObject player)
	{
		m_player = player;
		if (player != null)
		{
			m_health = player.GetComponentInParent<CharacterHealth>();
			CharacterHealth health = m_health;
			health.OnTakenDamage = (UnityAction<int>)Delegate.Combine(health.OnTakenDamage, new UnityAction<int>(OnTakenDamage));
			m_inventory = player.GetComponentInParent<CharacterInventory>();
		}
		else
		{
			m_health = null;
			m_inventory = null;
		}
	}

	private void OnTakenDamage(int damage)
	{
		if (m_enemyPresence.Value > 0.01f)
		{
			m_damageBump += (float)damage * m_damageBumpScalar;
			m_damageBump = Mathf.Clamp(m_damageBump, 0f, 0.4f);
		}
	}

	public void Update()
	{
		if (m_player != null)
		{
			m_currentTargetValue = CheckForEnemies();
		}
		float value = m_enemyPresence.Value;
		value = ((!(m_currentTargetValue > value)) ? Mathf.MoveTowards(value, m_currentTargetValue, Time.deltaTime * m_fallRate) : m_currentTargetValue);
		m_enemyPresence.Value = value;
	}

	private float GetEnemyPresenceJuiceAmount(EnemyPresence.EnemyPresenceType enemyType)
	{
		return enemyType switch
		{
			EnemyPresence.EnemyPresenceType.Normal => 0.3f, 
			EnemyPresence.EnemyPresenceType.Large => 0.6f, 
			EnemyPresence.EnemyPresenceType.Small => 0.25f, 
			EnemyPresence.EnemyPresenceType.Boss => 1f, 
			_ => 0f, 
		};
	}

	private float CheckForEnemies()
	{
		float num = 0f;
		Vector2 vector = (m_inventory.ArtifactInventory.HasActiveArtifactEffect(GlobalReferences.Instance.ArtifactEffects.Generic.NearbyEnemyWarning) ? m_artifactEnabledRange : m_regularRange);
		bool flag = false;
		foreach (EnemyPresence item in GlobalReferences.Instance.Sets.Generic.EnemyPresenceSet)
		{
			if (item.EnemyType == EnemyPresence.EnemyPresenceType.Boss && !item.IsDead())
			{
				return 1f;
			}
			if (item.IsKnown() && !item.IsDead())
			{
				float time = Vector3.Distance(item.RootTransform.position, m_player.transform.position).Remap(vector.x, vector.y, 1f, 0f);
				if (item.IsAlert())
				{
					flag = true;
				}
				float num2 = GetEnemyPresenceJuiceAmount(item.EnemyType) * m_effectDistanceCurve.Evaluate(time);
				if (!item.HasDirectLineOfSight())
				{
					num2 *= 0.4f;
				}
				num += num2;
			}
		}
		if (m_damageBump > 0f)
		{
			m_damageBump = Mathf.MoveTowards(m_damageBump, 0f, Time.deltaTime * m_damageBumpReductionRate);
		}
		num += m_damageBump;
		float healthPercentage = m_health.HealthPercentage;
		if (healthPercentage < m_aboutToDieHealthPercentage)
		{
			num *= m_persistentLowHealthScalar;
		}
		num = (flag ? Mathf.Max(num, 0.21f) : Mathf.Min(num, 0.19f));
		if (healthPercentage > m_aboutToDieHealthPercentage)
		{
			num = Mathf.Min(num, 0.8f);
		}
		return Mathf.Clamp01(num);
	}
}
