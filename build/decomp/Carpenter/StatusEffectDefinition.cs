using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "NewStatusEffectDefinition", menuName = "Status Effects/Status Effect Definition")]
public class StatusEffectDefinition : AddressableScriptableObject<StatusEffectDefinition>
{
	[Header("Enum")]
	[SerializeField]
	private StatusEffectType m_type;

	[Header("Activation Settings")]
	[Tooltip("How much buildup is required to activate this status effect")]
	[SerializeField]
	private int m_activationAmount;

	[Tooltip("How much buildup amount is allowed")]
	[SerializeField]
	private int m_maxAmount;

	[Header("Status Amount Reduction Rate")]
	[SerializeField]
	private float m_reductionRateTimePerTick = 0.1f;

	[SerializeField]
	private float m_inactiveReductionRatePerTick;

	[SerializeField]
	private float m_activeReductionRatePerTick;

	[SerializeField]
	private bool m_canReceiveBuildUpWhileActive;

	[Header("Visual Effects")]
	[SerializeField]
	private AssetReferenceGameObject m_visualEffectPrefabReference;

	[Header("Active Effects")]
	[SerializeField]
	private float m_effectTickRate = 2f;

	[Header("Activate Damage")]
	[SerializeField]
	private bool m_hasActivateDamage;

	[Tooltip("This is damage that is applied ONCE when a status becomes active")]
	[SerializeField]
	private HitSettings m_statusActiveDamage;

	[Header("Damage Over Time")]
	[SerializeField]
	private bool m_hasDamageOverTime;

	[SerializeField]
	private HitSettings m_damageOverTimeHitSettings;

	[Header("Health Regeneration")]
	[SerializeField]
	private bool m_hasHealthRegeneration;

	[SerializeField]
	private int m_healthRegenerationPerTick;

	[Header("Stamina Boost")]
	[SerializeField]
	private bool m_hasStaminaBoost;

	[SerializeField]
	private int m_maxStaminaIncrease;

	[SerializeField]
	private float m_staminaRecoveryRateIncrease;

	[Header("Movement")]
	[SerializeField]
	private bool m_preventCriticalHealthMovement;

	[Header("Health")]
	[SerializeField]
	private bool m_useMaxHealthOverride;

	[SerializeField]
	private int m_maxHealthOverride;

	[Header("Stamina Reduction")]
	[SerializeField]
	private bool m_useStaminaReductionPercent;

	[SerializeField]
	private float m_staminaReductionPercent;

	[Header("UI")]
	[SerializeField]
	private bool m_showAlertPopup;

	[SerializeField]
	private bool m_showInHealthOverview;

	[SerializeField]
	private bool m_hideBarWhileActive;

	[FormerlySerializedAs("m_textName")]
	[SerializeField]
	private LocalizedString m_activeName;

	[SerializeField]
	private LocalizedString m_inactiveName;

	[SerializeField]
	private Sprite m_statusEffectIcon;

	[SerializeField]
	private Color m_inactiveFillColor = Color.yellow;

	[SerializeField]
	private Color m_activeFillColor = Color.red;

	[SerializeField]
	private DynamicHintEvent m_hintToTrigger;

	[Header("Misc")]
	[SerializeField]
	private bool m_saveState = true;

	public StatusEffectType Type => m_type;

	public int ActivationAmount => m_activationAmount;

	public int MaxAmount => m_maxAmount;

	public float ReducationRateTimePerTick => m_reductionRateTimePerTick;

	public float InactiveReductionRatePerTick => m_inactiveReductionRatePerTick;

	public float ActiveReductionRatePerTick => m_activeReductionRatePerTick;

	public bool CanReceiveBuildUpWhileActive => m_canReceiveBuildUpWhileActive;

	public AssetReferenceGameObject VisualEffectPrefabReference => m_visualEffectPrefabReference;

	public float EffectTickRate => Mathf.Max(m_effectTickRate, 0.02f);

	public bool HasActivateDamage => m_hasActivateDamage;

	public HitSettings StatusActiveDamage => m_statusActiveDamage;

	public bool HasDamageOverTime => m_hasDamageOverTime;

	public HitSettings DamageOverTimeHitSettings => m_damageOverTimeHitSettings;

	public bool HasHealthRegeneration => m_hasHealthRegeneration;

	public int HealthRegenerationPerTick => m_healthRegenerationPerTick;

	public bool HasStaminaBoost => m_hasStaminaBoost;

	public int MaxStaminaIncrease => m_maxStaminaIncrease;

	public float StaminaRecoveryRateMultiplier => m_staminaRecoveryRateIncrease;

	public bool PreventCriticalHealthMovement => m_preventCriticalHealthMovement;

	public bool UseMaxHealthOverride => m_useMaxHealthOverride;

	public int MaxHealthOverride => m_maxHealthOverride;

	public bool UseStaminaReductionPercent => m_useStaminaReductionPercent;

	public float StaminaReductionPercent => m_staminaReductionPercent;

	public bool ShowAlertPopup => m_showAlertPopup;

	public bool ShowInHealthOverview => m_showInHealthOverview;

	public bool HideBarWhileActive => m_hideBarWhileActive;

	public LocalizedString ActiveName => m_activeName;

	public LocalizedString InactiveName
	{
		get
		{
			if (!m_inactiveName.IsEmpty)
			{
				return m_inactiveName;
			}
			return m_activeName;
		}
	}

	public Sprite StatusEffectIcon => m_statusEffectIcon;

	public Color InactiveFillColor => m_inactiveFillColor;

	public Color ActiveFillColor => m_activeFillColor;

	public DynamicHintEvent HintToTrigger => m_hintToTrigger;

	public bool SaveState => m_saveState;
}
