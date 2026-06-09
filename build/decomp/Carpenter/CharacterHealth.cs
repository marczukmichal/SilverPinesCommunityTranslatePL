using System;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class CharacterHealth : MonoBehaviour, IDamageable, IPersistentComponent
{
	public enum StatRecordingType
	{
		None,
		Enemy
	}

	[Serializable]
	private class PersistentData
	{
		public int m_storedHealth;

		public bool m_diedFromWeakpoint;
	}

	[ShowInDesignerInspector]
	[SerializeField]
	private int m_maxHealth = 100;

	[SerializeField]
	private IntVariable m_maxHealthVariable;

	[Tooltip("Min / Max range for starting health. Random value will be selected from this range")]
	[ShowInDesignerInspector]
	[SerializeField]
	private Vector2Int m_startingHealth;

	[SerializeField]
	private IntVariable m_healthVariable;

	[SerializeField]
	private FloatVariable m_healthVariablePercentage;

	[SerializeField]
	public UnityEvent OnDead;

	public UnityAction<DamageInstance> OnDying;

	public UnityAction OnRevive;

	public UnityAction<int> OnTakenDamage;

	[SerializeField]
	private DifficultyDamageScalingMode m_damagesScalingMode;

	[SerializeField]
	private VoidGameEventChannel m_onDeadEventChannel;

	[SerializeField]
	private VoidGameEventChannel m_onTakeDamageEventChannel;

	[Header("Stats")]
	[SerializeField]
	private StatRecordingType m_deathStatType;

	private bool m_diedFromWeakpoint;

	private float m_damageReceivedScalar = 1f;

	[DebugCommand("jesus", "Jesus (player can take hits but can't die)", "jesus <true/false>", typeof(bool), true)]
	private static bool JESUS_PLAYER = false;

	private PersistentDataObject m_persistentDataObject;

	private PersistentData m_persistentData;

	private static readonly int UNDEFINED_HEALTH = int.MaxValue;

	private int m_minimumHealth;

	private int m_currentHealth = UNDEFINED_HEALTH;

	public int MaxHealth
	{
		get
		{
			int num = ((m_maxHealthVariable != null) ? m_maxHealthVariable.Value : m_maxHealth);
			StatusEffectReceiver component = GetComponent<StatusEffectReceiver>();
			if (component != null)
			{
				int maxHealthOverride = component.GetMaxHealthOverride();
				if (maxHealthOverride > 0)
				{
					num = Mathf.Min(maxHealthOverride, num);
				}
			}
			return num;
		}
	}

	public bool DiedFromWeakpoint => m_diedFromWeakpoint;

	public int Health
	{
		get
		{
			return m_currentHealth;
		}
		set
		{
			int num = Mathf.Clamp(value, m_minimumHealth, MaxHealth);
			if (m_currentHealth != num)
			{
				int currentHealth = m_currentHealth;
				m_currentHealth = num;
				if (currentHealth > 0 && m_currentHealth == 0)
				{
					OnDead.Invoke();
					m_onDeadEventChannel?.Raise();
				}
				else if (currentHealth <= 0 && m_currentHealth > 0)
				{
					OnRevive?.Invoke();
				}
				if (m_healthVariable != null)
				{
					m_healthVariable.Value = m_currentHealth;
				}
				if (m_healthVariablePercentage != null)
				{
					m_healthVariablePercentage.Value = HealthPercentage;
				}
				if (m_persistentDataObject != null)
				{
					m_persistentData.m_storedHealth = m_currentHealth;
				}
			}
		}
	}

	public float HealthPercentage
	{
		get
		{
			int num = ((m_maxHealthVariable != null) ? m_maxHealthVariable.Value : m_maxHealth);
			return (float)Health / (float)num;
		}
	}

	public bool IsDead => (float)Health <= 0f;

	public void SetDamageReceivedScalar(float scalar)
	{
		m_damageReceivedScalar = scalar;
	}

	bool IDamageable.ShouldConsumeMeleeHit()
	{
		return !IsDead;
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
			Health = m_persistentData.m_storedHealth;
			m_diedFromWeakpoint = m_persistentData.m_diedFromWeakpoint;
			return;
		}
		m_persistentData = new PersistentData();
		m_persistentDataObject.Data = m_persistentData;
		if (m_healthVariable == null)
		{
			ResetHealth();
		}
		m_persistentData.m_storedHealth = m_currentHealth;
	}

	public void SetMinimumHealth(int minimumHealth)
	{
		m_minimumHealth = minimumHealth;
	}

	private void Start()
	{
		if (m_healthVariable == null && m_persistentDataObject == null && m_currentHealth == UNDEFINED_HEALTH)
		{
			ResetHealth();
		}
		else if (m_healthVariable != null)
		{
			Health = m_healthVariable.Value;
			if (m_healthVariablePercentage != null)
			{
				m_healthVariablePercentage.Value = HealthPercentage;
			}
		}
	}

	private void OnEnable()
	{
		StatusEffectReceiver component = GetComponent<StatusEffectReceiver>();
		if (component != null)
		{
			component.m_onStatusEffectActiveChanged = (UnityAction<StatusEffectInstance, bool>)Delegate.Combine(component.m_onStatusEffectActiveChanged, new UnityAction<StatusEffectInstance, bool>(OnStatusEffectsChanged));
		}
	}

	private void OnStatusEffectsChanged(StatusEffectInstance statusEffect, bool active)
	{
		if (active && statusEffect.Definition.UseMaxHealthOverride)
		{
			Health = Mathf.Max(Health, MaxHealth);
		}
	}

	public void ResetHealth()
	{
		Health = m_startingHealth.GetRandom();
	}

	public void ApplyDamageInstance(DamageInstance instance)
	{
		if (GameDebugCommands.CHEAT_INVINCIBLE && GameUtils.IsPlayer(base.gameObject))
		{
			return;
		}
		int num = instance.HealthDamageAmount;
		if (m_damageReceivedScalar != 1f)
		{
			num = Mathf.RoundToInt((float)num * m_damageReceivedScalar);
		}
		if (instance.HitFlags.HasFlag(HitFlags.NonLethal) && num >= m_currentHealth)
		{
			num = Mathf.Max(0, m_currentHealth - 1);
		}
		int health = Health;
		bool flag = false;
		if (JESUS_PLAYER && GameUtils.IsPlayer(base.gameObject) && num >= Health)
		{
			num = Health - 1;
			flag = true;
		}
		if (GameUtils.IsPlayer(base.gameObject) && num > 0)
		{
			GlobalReferences.Instance.DataStore.Data.m_stats.m_damageTaken += num;
		}
		CharacterInventory component = GetComponent<CharacterInventory>();
		if (component != null)
		{
			if (component.ArtifactInventory.HasActiveArtifactEffect(GlobalReferences.Instance.ArtifactEffects.Generic.DamageReduction, out var floatValue, out var integerValue))
			{
				float num2 = Mathf.Clamp(1f - floatValue, 0.1f, 1f);
				num = Mathf.FloorToInt((float)num * num2);
			}
			if (component.ArtifactInventory.HasActiveArtifactEffect(GlobalReferences.Instance.ArtifactEffects.Generic.DamageTakenIncrease, out var floatValue2, out integerValue))
			{
				float num3 = 1f + floatValue2;
				num = Mathf.FloorToInt((float)num * num3);
			}
		}
		Health -= num;
		if (health != Health)
		{
			_ = Health;
			if (IsDead)
			{
				if (m_deathStatType == StatRecordingType.Enemy)
				{
					GlobalReferences.Instance.DataStore.Data.m_stats.m_kills++;
					switch (instance.DamageCategory)
					{
					case DamageCategory.DamageCollider:
						GlobalReferences.Instance.DataStore.Data.m_stats.m_killsWithMelee++;
						break;
					case DamageCategory.Projectile:
						GlobalReferences.Instance.DataStore.Data.m_stats.m_killsWithGuns++;
						break;
					}
				}
				m_diedFromWeakpoint = instance.IsWeakpointDamage;
				if (m_persistentData != null)
				{
					m_persistentData.m_diedFromWeakpoint = m_diedFromWeakpoint;
				}
				CharacterHitReact component2 = GetComponent<CharacterHitReact>();
				if (component2 != null)
				{
					component2.DoKillEffect(instance, m_diedFromWeakpoint);
				}
				OnDying?.Invoke(instance);
			}
		}
		if (num != 0)
		{
			OnTakenDamage?.Invoke(num);
			if (m_onTakeDamageEventChannel != null)
			{
				m_onTakeDamageEventChannel?.Raise();
			}
		}
		if (flag)
		{
			Health = MaxHealth;
		}
	}

	public void IncreaseCharacterMaxHealth(int maxHealthIncrease)
	{
		if (m_maxHealthVariable != null)
		{
			m_maxHealthVariable.Value += maxHealthIncrease;
		}
		else
		{
			m_maxHealth += maxHealthIncrease;
		}
		Health += maxHealthIncrease;
	}

	public bool IsKillingBlow(DamageInstance damageInstance)
	{
		if (damageInstance.HealthDamageAmount > m_currentHealth)
		{
			return true;
		}
		return false;
	}

	public bool IsWoundedOrLower()
	{
		return HealthPercentage <= 0.5f;
	}
}
