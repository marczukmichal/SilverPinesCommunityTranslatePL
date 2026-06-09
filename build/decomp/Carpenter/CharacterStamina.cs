using System;
using UnityEngine;

public class CharacterStamina : MonoBehaviour
{
	public enum StaminaEventType
	{
		None,
		Sprint,
		Jump,
		BlockContinous,
		BlockStart,
		DodgeForward,
		DodgeBackwards,
		MeleeCharge,
		MeleeBigSwing,
		MeleeQuickSwing,
		Swimming,
		StompAttack
	}

	[Serializable]
	public struct StaminaValueSettings
	{
		[Header("Movement")]
		public float m_sprint;

		public float m_jump;

		[Header("Dodge / Evade")]
		public float m_dodgeForward;

		public float m_dodgeBackwards;

		[Header("Melee")]
		public float m_meleeCharge;

		public float m_meleeBigSwing;

		public float m_meleeQuickSwing;

		public float m_meleeStomp;

		[Header("Block")]
		public float m_blockContinous;

		public float m_blockStart;

		public float m_blockDamageToStaminaConversionRate;

		[Header("Other")]
		public float m_swimming;
	}

	[SerializeField]
	private float m_staminaRechargeDelay;

	[SerializeField]
	private float m_staminaExhaustRechargeDelay;

	[SerializeField]
	private float m_staminaRechargeRate;

	[SerializeField]
	private float m_maxStamina = 100f;

	[SerializeField]
	private IntVariable m_maxStaminaUpgradeCount;

	[SerializeField]
	private float m_maxStaminaUpgradeAmount = 10f;

	[SerializeField]
	private float m_minimumStamina = -50f;

	[SerializeField]
	private float m_exhaustStartPoint = -10f;

	[SerializeField]
	private float m_exhaustExitPoint;

	[SerializeField]
	private float m_staminaRechargeRateExhausted;

	[SerializeField]
	private float m_staminaConsumptionScalar = 1f;

	[Header("Reserve")]
	[SerializeField]
	private float m_maxStaminaReserve = 300f;

	[SerializeField]
	private FloatVariable m_staminaReservePersistentVariable;

	[SerializeField]
	private StaminaValueSettings m_settings;

	private bool m_isExhausted;

	private float m_stamina;

	private float m_staminaReserve;

	private float m_staminaRechargeBlockTime;

	private StaminaInfoData m_eventData = new StaminaInfoData();

	private CharacterInventory m_characterInventory;

	private StatusEffectReceiver m_statusEffectReceiver;

	public float MaxStamina
	{
		get
		{
			float num = m_maxStamina;
			if (m_maxStaminaUpgradeCount != null)
			{
				num += m_maxStaminaUpgradeAmount * (float)m_maxStaminaUpgradeCount.Value;
			}
			if (m_statusEffectReceiver != null)
			{
				num += (float)m_statusEffectReceiver.GetMaxStaminaUpgradeAmount();
			}
			if (m_characterInventory != null && m_characterInventory.ArtifactInventory.HasActiveArtifactEffect(GlobalReferences.Instance.ArtifactEffects.Generic.IncreaseMaxStamina, out float floatValue))
			{
				num += floatValue;
			}
			if (m_statusEffectReceiver != null)
			{
				num *= m_statusEffectReceiver.GetStaminaReductionOverride();
			}
			return num;
		}
	}

	public bool IsExhausted => m_isExhausted;

	public float AvailableStamina => m_stamina + m_staminaReserve;

	public float StaminaReserve => m_staminaReserve;

	public void UpgradeMaxStamina()
	{
		if (m_maxStaminaUpgradeCount != null)
		{
			m_maxStaminaUpgradeCount.Value++;
			GlobalReferences.Instance.Achievements.MaxStaminaUpgrades.UnlockIfValueReached(m_maxStaminaUpgradeCount.Value);
		}
	}

	private void Start()
	{
		if (m_staminaReservePersistentVariable != null)
		{
			m_staminaReserve = m_staminaReservePersistentVariable.Value;
		}
		m_characterInventory = GetComponent<CharacterInventory>();
		m_statusEffectReceiver = GetComponent<StatusEffectReceiver>();
		m_stamina = MaxStamina;
		SendStaminaChangedEvent(forceShow: false);
	}

	private void Update()
	{
		float stamina = m_stamina;
		bool isExhausted = m_isExhausted;
		if (m_staminaRechargeBlockTime <= 0f)
		{
			float num = (m_isExhausted ? m_staminaRechargeRateExhausted : m_staminaRechargeRate);
			if (m_characterInventory != null && m_characterInventory.ArtifactInventory.HasActiveArtifactEffect(GlobalReferences.Instance.ArtifactEffects.Generic.FasterStaminaRecovery, out var floatValue, out var _))
			{
				num *= 1f + floatValue;
			}
			if (m_statusEffectReceiver != null)
			{
				num += m_statusEffectReceiver.GetStaminaRecoveryRateIncrease();
			}
			m_stamina += num * Time.deltaTime;
			if (AvailableStamina >= m_exhaustExitPoint)
			{
				m_isExhausted = false;
			}
			m_stamina = Mathf.Clamp(m_stamina, m_minimumStamina, MaxStamina);
		}
		else
		{
			m_staminaRechargeBlockTime -= Time.deltaTime;
		}
		if (stamina != m_stamina || isExhausted != m_isExhausted)
		{
			SendStaminaChangedEvent(forceShow: false);
		}
	}

	private void SendStaminaChangedEvent(bool forceShow)
	{
		m_eventData.m_isExhausted = m_isExhausted;
		m_eventData.m_baseCurrentStamina = Mathf.Max(m_stamina, 0f);
		m_eventData.m_availableStamina = Mathf.Max(AvailableStamina, 0f);
		m_eventData.m_maxStamina = MaxStamina;
		m_eventData.m_totalMaxStamina = MaxStamina + m_staminaReserve;
		m_eventData.m_staminaReserve = m_staminaReserve;
		m_eventData.m_exhaustionRecoveryStamina = m_exhaustExitPoint / MaxStamina;
		m_eventData.m_forceShow = forceShow;
		GlobalReferences.Instance.EventChannels.Stamina.StaminaInfo.Raise(m_eventData);
	}

	private float GetMeleeWeaponScalar()
	{
		float num = 1f;
		if (m_characterInventory.Inventory.EquippedMeleeItem is MeleeWeaponItemInstance meleeWeaponItemInstance)
		{
			num = meleeWeaponItemInstance.WeaponDefinition.StaminaUseScalar;
		}
		if (m_characterInventory != null && m_characterInventory.ArtifactInventory.HasActiveArtifactEffect(GlobalReferences.Instance.ArtifactEffects.Generic.ReducedMeleeStaminaCost, out var floatValue, out var _))
		{
			num *= 1f - floatValue;
		}
		return num;
	}

	public void ConsumeStamina(StaminaEventType staminaEventType, float scalar = 1f)
	{
		int integerValue;
		switch (staminaEventType)
		{
		case StaminaEventType.Sprint:
		{
			if (m_characterInventory != null && m_characterInventory.ArtifactInventory.HasActiveArtifactEffect(GlobalReferences.Instance.ArtifactEffects.Generic.ReducedSprintStaminaCost, out var floatValue3, out integerValue))
			{
				scalar *= 1f - floatValue3;
			}
			ConsumeStamina(m_settings.m_sprint * scalar);
			break;
		}
		case StaminaEventType.Jump:
			ConsumeStamina(m_settings.m_jump * scalar);
			break;
		case StaminaEventType.BlockContinous:
		{
			if (m_characterInventory != null && m_characterInventory.ArtifactInventory.HasActiveArtifactEffect(GlobalReferences.Instance.ArtifactEffects.Generic.ReducedBlockStaminaCost, out var floatValue4, out integerValue))
			{
				scalar *= 1f - floatValue4;
			}
			ConsumeStamina(m_settings.m_blockContinous * scalar);
			break;
		}
		case StaminaEventType.BlockStart:
		{
			if (m_characterInventory != null && m_characterInventory.ArtifactInventory.HasActiveArtifactEffect(GlobalReferences.Instance.ArtifactEffects.Generic.ReducedBlockStaminaCost, out var floatValue6, out integerValue))
			{
				scalar *= 1f - floatValue6;
			}
			ConsumeStamina(m_settings.m_blockStart * scalar);
			break;
		}
		case StaminaEventType.DodgeForward:
		{
			if (m_characterInventory != null && m_characterInventory.ArtifactInventory.HasActiveArtifactEffect(GlobalReferences.Instance.ArtifactEffects.Generic.ReducedDodgeStaminaCost, out var floatValue2, out integerValue))
			{
				scalar *= 1f - floatValue2;
			}
			ConsumeStamina(m_settings.m_dodgeForward * scalar);
			break;
		}
		case StaminaEventType.DodgeBackwards:
		{
			if (m_characterInventory != null && m_characterInventory.ArtifactInventory.HasActiveArtifactEffect(GlobalReferences.Instance.ArtifactEffects.Generic.ReducedDodgeStaminaCost, out var floatValue5, out integerValue))
			{
				scalar *= 1f - floatValue5;
			}
			ConsumeStamina(m_settings.m_dodgeBackwards * scalar);
			break;
		}
		case StaminaEventType.MeleeCharge:
			ConsumeStamina(m_settings.m_meleeCharge * GetMeleeWeaponScalar() * scalar);
			break;
		case StaminaEventType.MeleeBigSwing:
			ConsumeStamina(m_settings.m_meleeBigSwing * GetMeleeWeaponScalar() * scalar);
			break;
		case StaminaEventType.MeleeQuickSwing:
			ConsumeStamina(m_settings.m_meleeQuickSwing * GetMeleeWeaponScalar() * scalar);
			break;
		case StaminaEventType.Swimming:
		{
			if (m_characterInventory != null && m_characterInventory.ArtifactInventory.HasActiveArtifactEffect(GlobalReferences.Instance.ArtifactEffects.Generic.ReducedSwimmingStaminaCost, out var floatValue, out integerValue))
			{
				scalar *= 1f - floatValue;
			}
			ConsumeStamina(m_settings.m_swimming * scalar);
			break;
		}
		case StaminaEventType.StompAttack:
			ConsumeStamina(m_settings.m_meleeStomp * scalar);
			break;
		}
	}

	public float GetBlockDamageMultiplier()
	{
		return m_settings.m_blockDamageToStaminaConversionRate * GetMeleeWeaponScalar();
	}

	public void ConsumeStamina(float staminaAmount)
	{
		if (GameDebugCommands.CHEAT_INFINITE_STAMINA)
		{
			return;
		}
		staminaAmount *= m_staminaConsumptionScalar;
		m_stamina -= staminaAmount;
		if (m_stamina < 0f && m_staminaReserve > 0f)
		{
			float num = 0f - m_stamina;
			m_staminaReserve -= num;
			m_staminaReserve = Mathf.Max(0f, m_staminaReserve);
			if (m_staminaReservePersistentVariable != null)
			{
				m_staminaReservePersistentVariable.Value = m_staminaReserve;
			}
			m_stamina = 0f;
		}
		m_stamina = Mathf.Clamp(m_stamina, m_minimumStamina, MaxStamina);
		if (AvailableStamina <= m_exhaustStartPoint)
		{
			m_isExhausted = true;
		}
		float num2 = (m_isExhausted ? m_staminaExhaustRechargeDelay : m_staminaRechargeDelay);
		if (m_characterInventory != null && m_characterInventory.ArtifactInventory.HasActiveArtifactEffect(GlobalReferences.Instance.ArtifactEffects.Generic.StaminaRecoveryDelay, out var floatValue, out var _))
		{
			num2 *= floatValue;
		}
		m_staminaRechargeBlockTime = Mathf.Max(num2, m_staminaRechargeBlockTime);
		SendStaminaChangedEvent(forceShow: true);
	}

	public void AddStaminaReserve(float amount)
	{
		m_staminaReserve += amount;
		m_staminaReserve = Mathf.Min(m_maxStaminaReserve, m_staminaReserve);
		if (m_staminaReservePersistentVariable != null)
		{
			m_staminaReservePersistentVariable.Value = m_staminaReserve;
		}
		SendStaminaChangedEvent(forceShow: true);
	}
}
