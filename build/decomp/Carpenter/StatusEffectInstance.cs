using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;

[Serializable]
public class StatusEffectInstance
{
	[SerializeField]
	private AssetReference m_statusEffectAssetReference;

	private StatusEffectDefinition m_definition;

	[SerializeField]
	private bool m_active;

	private float m_reductionRateTickTimer;

	private float m_damageOverTimeTickTimer;

	private readonly float s_reductionDelayTime = 1f;

	public UnityAction<StatusEffectInstance, bool> OnStatusEffectActiveStateChanged;

	private StatusEffectReceiver m_parentStatusEffectReceiver;

	private GameObject m_spawnedEffect;

	private StatusEffectReceiver.StatusEffectOverrideSettings m_activeOverrideSettings;

	[SerializeField]
	private float m_amount;

	public StatusEffectDefinition Definition
	{
		get
		{
			if (m_definition == null)
			{
				m_definition = AddressablesContentManager.Instance.GetScriptableObjectAsset(m_statusEffectAssetReference) as StatusEffectDefinition;
			}
			return m_definition;
		}
	}

	public bool IsActive => m_active;

	public float Amount
	{
		get
		{
			return m_amount;
		}
		set
		{
			value = Mathf.Clamp(value, 0f, Definition.MaxAmount);
			if (m_amount != value)
			{
				m_amount = value;
				CheckForStateChange();
			}
		}
	}

	public float PercentageFilled => Mathf.Clamp01(Amount / (float)Definition.MaxAmount);

	public bool IsDefinitionLoaded()
	{
		return m_definition != null;
	}

	public void ApplyStatusEffectAmount(int amount)
	{
		if (m_activeOverrideSettings != null)
		{
			amount = Mathf.RoundToInt(m_activeOverrideSettings.Multiplier * (float)amount);
		}
		Amount += amount;
		m_reductionRateTickTimer = 0f - s_reductionDelayTime;
	}

	public void RemoveEffect()
	{
		Amount = 0f;
	}

	private void SetActive(bool active)
	{
		if (m_active != active)
		{
			m_active = active;
			if (active && Definition.HasActivateDamage)
			{
				ApplyStatusActivateDamage();
			}
			if (active && Definition.HintToTrigger != 0)
			{
				GlobalReferences.Instance.EventChannels.Hints.DynamicHint.Raise(DynamicHintEvent.Poisoned);
			}
			OnStatusEffectActiveStateChanged?.Invoke(this, active);
			UpdateVisuals();
		}
	}

	private void UpdateVisuals()
	{
		if (Definition.VisualEffectPrefabReference == null || !Definition.VisualEffectPrefabReference.HasAsset())
		{
			return;
		}
		if (m_active)
		{
			DynamicallySpawnObjectEventData eventData = default(DynamicallySpawnObjectEventData);
			eventData.m_assetReference = Definition.VisualEffectPrefabReference;
			eventData.m_position = m_parentStatusEffectReceiver.transform.position;
			eventData.m_rotation = Quaternion.identity;
			eventData.m_scale = Vector3.one;
			eventData.m_onSpawnedAction = delegate(GameObject spawnedObject)
			{
				m_spawnedEffect = spawnedObject;
				m_spawnedEffect.transform.SetParent(m_parentStatusEffectReceiver.EffectParent);
				StatusEffectVisuals component2 = m_spawnedEffect.GetComponent<StatusEffectVisuals>();
				if (component2 != null)
				{
					component2.AttachToEffectReceiver(m_parentStatusEffectReceiver);
				}
			};
			DynamicallySpawnedObject.Spawn(eventData);
		}
		else if (m_spawnedEffect != null)
		{
			m_spawnedEffect.transform.SetParent(null);
			StatusEffectVisuals component = m_spawnedEffect.GetComponent<StatusEffectVisuals>();
			if (component != null)
			{
				component.DisableEffects();
			}
			m_spawnedEffect = null;
		}
	}

	public void CheckForStateChange()
	{
		if (!m_active && m_amount >= (float)Definition.ActivationAmount)
		{
			SetActive(active: true);
		}
		else if (m_active && m_amount <= 0f)
		{
			SetActive(active: false);
		}
	}

	public StatusEffectInstance(StatusEffectDefinition definition, int amount)
	{
		m_statusEffectAssetReference = definition.AssetReference;
		m_definition = definition;
		m_amount = amount;
	}

	public void AttachToParentReceiver(StatusEffectReceiver receiverParent)
	{
		m_parentStatusEffectReceiver = receiverParent;
		UpdateVisuals();
	}

	public void UpdateStatusEffect()
	{
		float num = 1f;
		if (m_definition == GlobalReferences.Instance.StatusEffects.Generic.GreenHerbRegeneration)
		{
			CharacterInventory component = m_parentStatusEffectReceiver.GetComponent<CharacterInventory>();
			if (component != null && component.ArtifactInventory.HasActiveArtifactEffect(GlobalReferences.Instance.ArtifactEffects.Generic.FasterHerbHeal, out float floatValue))
			{
				num += 1f + floatValue;
			}
		}
		if (IsActive)
		{
			m_damageOverTimeTickTimer += Time.deltaTime * num;
			while (m_damageOverTimeTickTimer >= Definition.EffectTickRate)
			{
				m_damageOverTimeTickTimer -= Definition.EffectTickRate;
				if (Definition.HasDamageOverTime)
				{
					ApplyDamageOverTimeEffect();
				}
				if (Definition.HasHealthRegeneration)
				{
					ApplyHealthRegeneration();
				}
			}
		}
		m_reductionRateTickTimer += Time.deltaTime * num;
		while (m_reductionRateTickTimer >= Definition.ReducationRateTimePerTick)
		{
			m_reductionRateTickTimer -= Definition.ReducationRateTimePerTick;
			if (IsActive)
			{
				Amount -= ((m_activeOverrideSettings != null) ? ((float)m_activeOverrideSettings.ActiveReductionRatePerTick) : Definition.ActiveReductionRatePerTick);
			}
			else
			{
				Amount -= ((m_activeOverrideSettings != null) ? ((float)m_activeOverrideSettings.InactiveReductionRatePerTick) : Definition.InactiveReductionRatePerTick);
			}
		}
	}

	private void ApplyStatusActivateDamage()
	{
		DamageInstance damageInstance = new DamageInstance().PopulateFromHitSettings(Definition.StatusActiveDamage).SetDamageSource(m_parentStatusEffectReceiver.gameObject).SetPosition(m_parentStatusEffectReceiver.transform.position);
		DamageUtilities.ApplyDamage(m_parentStatusEffectReceiver.gameObject, damageInstance);
	}

	private void ApplyDamageOverTimeEffect()
	{
		DamageInstance damageInstance = new DamageInstance().PopulateFromHitSettings(Definition.DamageOverTimeHitSettings).SetDamageSource(m_parentStatusEffectReceiver.gameObject).SetPosition(m_parentStatusEffectReceiver.transform.position);
		DamageUtilities.ApplyDamage(m_parentStatusEffectReceiver.gameObject, damageInstance);
	}

	private void ApplyHealthRegeneration()
	{
		m_parentStatusEffectReceiver.gameObject.GetComponent<CharacterHealth>().Health += Definition.HealthRegenerationPerTick;
	}

	public void SetOverrideSettings(StatusEffectReceiver.StatusEffectOverrideSettings overrideSettings)
	{
		m_activeOverrideSettings = overrideSettings;
	}
}
