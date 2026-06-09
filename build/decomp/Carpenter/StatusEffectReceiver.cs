using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StatusEffectReceiver : MonoBehaviour, IDamageable
{
	[Serializable]
	public class StatusEffectOverrideSettings
	{
		[SerializeField]
		private StatusEffectDefinition m_statusEffect;

		[SerializeField]
		private float m_multiplier = 1f;

		[SerializeField]
		private int m_inactiveReductionRatePerTick;

		[SerializeField]
		private int m_activeReductionRatePerTick;

		public StatusEffectDefinition StatusEffect => m_statusEffect;

		public float Multiplier => m_multiplier;

		public int InactiveReductionRatePerTick => m_inactiveReductionRatePerTick;

		public int ActiveReductionRatePerTick => m_activeReductionRatePerTick;
	}

	[SerializeField]
	private Transform m_effectParentTransform;

	[SerializeField]
	private StatusEffectInstanceGameEventChannel m_statusEffectUpdatedEventChannel;

	[SerializeField]
	private StatusEffectsVariable m_statusEffectsVariable;

	[SerializeField]
	private SpriteRenderer m_visualsSpriteRenderer;

	private List<StatusEffectInstance> m_statusEffectInstances;

	private bool m_ticksPaused;

	public UnityAction<StatusEffectInstance, bool> m_onStatusEffectActiveChanged;

	public UnityAction<StatusEffectInstance, int> m_onStatusEffectAmountChanged;

	[SerializeField]
	private StatusEffectOverrideSettings[] m_overrideSettings;

	public SpriteRenderer VisualsSpriteRenderer => m_visualsSpriteRenderer;

	public Transform EffectParent => m_effectParentTransform;

	private void Start()
	{
		if (m_statusEffectsVariable != null && m_statusEffectsVariable.Value != null && m_statusEffectsVariable.Value.Count > 0)
		{
			m_statusEffectInstances = m_statusEffectsVariable.Value;
			foreach (StatusEffectInstance statusEffectInstance in m_statusEffectInstances)
			{
				bool flag = false;
				StatusEffectOverrideSettings[] overrideSettings = m_overrideSettings;
				foreach (StatusEffectOverrideSettings statusEffectOverrideSettings in overrideSettings)
				{
					if (statusEffectOverrideSettings.StatusEffect.Type == statusEffectInstance.Definition.Type)
					{
						statusEffectInstance.SetOverrideSettings(statusEffectOverrideSettings);
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					statusEffectInstance.SetOverrideSettings(null);
				}
				statusEffectInstance.OnStatusEffectActiveStateChanged = OnStatusEffectActiveChanged;
				statusEffectInstance.AttachToParentReceiver(this);
				if (!statusEffectInstance.Definition.SaveState)
				{
					statusEffectInstance.RemoveEffect();
				}
			}
		}
		else
		{
			m_statusEffectInstances = new List<StatusEffectInstance>();
		}
		if (m_statusEffectsVariable != null)
		{
			m_statusEffectsVariable.SetValue(m_statusEffectInstances);
		}
	}

	private StatusEffectInstance GetEffectInstance(StatusEffectDefinition definition)
	{
		foreach (StatusEffectInstance statusEffectInstance2 in m_statusEffectInstances)
		{
			if (statusEffectInstance2.Definition.Type == definition.Type)
			{
				return statusEffectInstance2;
			}
		}
		StatusEffectInstance statusEffectInstance = new StatusEffectInstance(definition, 0);
		statusEffectInstance.AttachToParentReceiver(this);
		StatusEffectOverrideSettings[] overrideSettings = m_overrideSettings;
		foreach (StatusEffectOverrideSettings statusEffectOverrideSettings in overrideSettings)
		{
			if (statusEffectOverrideSettings.StatusEffect.Type == definition.Type)
			{
				statusEffectInstance.SetOverrideSettings(statusEffectOverrideSettings);
			}
		}
		m_statusEffectInstances.Add(statusEffectInstance);
		statusEffectInstance.OnStatusEffectActiveStateChanged = (UnityAction<StatusEffectInstance, bool>)Delegate.Combine(statusEffectInstance.OnStatusEffectActiveStateChanged, new UnityAction<StatusEffectInstance, bool>(OnStatusEffectActiveChanged));
		statusEffectInstance.CheckForStateChange();
		return statusEffectInstance;
	}

	private void OnStatusEffectActiveChanged(StatusEffectInstance statusEffectInstance, bool active)
	{
		m_onStatusEffectActiveChanged?.Invoke(statusEffectInstance, active);
		OnStatusEffectUpdated(statusEffectInstance);
	}

	private void OnStatusEffectUpdated(StatusEffectInstance statusEffectInstance)
	{
		if (m_statusEffectUpdatedEventChannel != null)
		{
			m_statusEffectUpdatedEventChannel.Raise(statusEffectInstance);
		}
	}

	public void ApplyDamageInstance(DamageInstance instance)
	{
		if (instance.StatusEffects == null)
		{
			return;
		}
		foreach (StatusEffectHitResults.StatusEffectHitInstance statusEffect in instance.StatusEffects.m_statusEffects)
		{
			ApplyStatusEffect(statusEffect.m_statusEffect, statusEffect.m_amountToAdd);
		}
	}

	public void ApplyStatusEffect(StatusEffectDefinition definition, int amount)
	{
		StatusEffectInstance effectInstance = GetEffectInstance(definition);
		if (amount <= 0 || !effectInstance.IsActive || definition.CanReceiveBuildUpWhileActive)
		{
			effectInstance.ApplyStatusEffectAmount(amount);
			OnStatusEffectUpdated(effectInstance);
			m_onStatusEffectAmountChanged?.Invoke(effectInstance, amount);
		}
	}

	public void RemoveStatusEffect(StatusEffectDefinition definition)
	{
		StatusEffectInstance effectInstance = GetEffectInstance(definition);
		effectInstance.RemoveEffect();
		OnStatusEffectUpdated(effectInstance);
	}

	public void ClearAll()
	{
		foreach (StatusEffectInstance statusEffectInstance in m_statusEffectInstances)
		{
			statusEffectInstance.Amount = 0f;
		}
	}

	public void GetDebugInfo(ref string infoString)
	{
		if (m_statusEffectInstances == null)
		{
			return;
		}
		foreach (StatusEffectInstance statusEffectInstance in m_statusEffectInstances)
		{
			if (statusEffectInstance.Amount > 0f || statusEffectInstance.IsActive)
			{
				string text = statusEffectInstance.Definition.name + ": " + statusEffectInstance.Amount + " - " + statusEffectInstance.IsActive + "\n";
				infoString += text;
			}
		}
	}

	public void Update()
	{
		if (m_ticksPaused)
		{
			return;
		}
		foreach (StatusEffectInstance statusEffectInstance in m_statusEffectInstances)
		{
			if (statusEffectInstance.Amount > 0f)
			{
				statusEffectInstance.UpdateStatusEffect();
				OnStatusEffectUpdated(statusEffectInstance);
			}
		}
	}

	public void SetTicksPaused(bool paused)
	{
		m_ticksPaused = paused;
	}

	public bool IsStatusEffectActive(StatusEffectDefinition definition)
	{
		if (definition == null)
		{
			return false;
		}
		foreach (StatusEffectInstance statusEffectInstance in m_statusEffectInstances)
		{
			if (statusEffectInstance != null && !(statusEffectInstance.Definition == null) && statusEffectInstance.Definition.Type == definition.Type && statusEffectInstance.IsActive)
			{
				return true;
			}
		}
		return false;
	}

	public float GetStatusEffectAmount(StatusEffectDefinition definition)
	{
		foreach (StatusEffectInstance statusEffectInstance in m_statusEffectInstances)
		{
			if (statusEffectInstance.Definition.Type == definition.Type && statusEffectInstance.IsActive)
			{
				return statusEffectInstance.Amount;
			}
		}
		return 0f;
	}

	public bool IsAnyVisualStatusEffectActive()
	{
		foreach (StatusEffectInstance statusEffectInstance in m_statusEffectInstances)
		{
			if (statusEffectInstance.Amount > 0f && statusEffectInstance.Definition.ShowAlertPopup)
			{
				return true;
			}
		}
		return false;
	}

	public int GetMaxStaminaUpgradeAmount()
	{
		int num = 0;
		foreach (StatusEffectInstance statusEffectInstance in m_statusEffectInstances)
		{
			if (statusEffectInstance.IsActive && statusEffectInstance.Definition.HasStaminaBoost)
			{
				num += statusEffectInstance.Definition.MaxStaminaIncrease;
			}
		}
		return num;
	}

	public float GetStaminaRecoveryRateIncrease()
	{
		float num = 0f;
		foreach (StatusEffectInstance statusEffectInstance in m_statusEffectInstances)
		{
			if (statusEffectInstance.IsActive && statusEffectInstance.Definition.HasStaminaBoost)
			{
				num += statusEffectInstance.Definition.StaminaRecoveryRateMultiplier;
			}
		}
		return num;
	}

	public bool IsPreventingCriticalHealthMovement()
	{
		if (m_statusEffectInstances == null)
		{
			return false;
		}
		foreach (StatusEffectInstance statusEffectInstance in m_statusEffectInstances)
		{
			if (statusEffectInstance.IsActive && statusEffectInstance.Definition.PreventCriticalHealthMovement)
			{
				return true;
			}
		}
		return false;
	}

	public int GetMaxHealthOverride()
	{
		if (m_statusEffectInstances == null)
		{
			return -1;
		}
		foreach (StatusEffectInstance statusEffectInstance in m_statusEffectInstances)
		{
			if (statusEffectInstance.IsActive && statusEffectInstance.Definition.UseMaxHealthOverride)
			{
				return statusEffectInstance.Definition.MaxHealthOverride;
			}
		}
		return -1;
	}

	public float GetStaminaReductionOverride()
	{
		if (m_statusEffectInstances == null)
		{
			return 1f;
		}
		foreach (StatusEffectInstance statusEffectInstance in m_statusEffectInstances)
		{
			if (statusEffectInstance.IsActive && statusEffectInstance.Definition.UseStaminaReductionPercent)
			{
				return statusEffectInstance.Definition.StaminaReductionPercent;
			}
		}
		return 1f;
	}
}
