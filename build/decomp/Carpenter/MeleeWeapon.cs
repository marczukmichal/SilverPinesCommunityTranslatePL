using UnityEngine;

public class MeleeWeapon : MonoBehaviour
{
	public enum WeaponDurabilityLossType
	{
		MeleeHit,
		Block,
		Parry
	}

	[SerializeField]
	private MeleeWeaponItemDefinition m_associatedWeaponType;

	[SerializeField]
	private float m_bloodReductionRate = 0.1f;

	private CharacterInventory m_characterInventory;

	private BloodMaterialEffect m_bloodMaterialEffect;

	private MeleeWeaponItemInstance m_meleeWeaponItemInstance;

	private SpriteRenderer m_spriteRenderer;

	private float m_damageScale = 1f;

	private float m_staggerScale = 1f;

	private float m_bloodAmount;

	private int m_shaderPropertyID;

	private float m_lastShownTime;

	private static readonly float s_onHitBloodAmount = 0.25f;

	public MeleeWeaponItemDefinition WeaponItemDefinition => m_associatedWeaponType;

	public MeleeWeaponItemInstance MeleeWeaponItemInstance => m_meleeWeaponItemInstance;

	public float DamageScale
	{
		get
		{
			return m_damageScale;
		}
		set
		{
			m_damageScale = value;
		}
	}

	public float StaggerScale
	{
		get
		{
			return m_staggerScale;
		}
		set
		{
			m_staggerScale = value;
		}
	}

	private void Awake()
	{
		m_bloodMaterialEffect = GetComponentInParent<BloodMaterialEffect>();
		m_characterInventory = GetComponentInParent<CharacterInventory>();
		m_spriteRenderer = GetComponentInChildren<SpriteRenderer>();
		m_shaderPropertyID = Shader.PropertyToID("_OverlayAmount");
	}

	private void OnEnable()
	{
		float num = Time.time - m_lastShownTime;
		m_bloodAmount -= num * m_bloodReductionRate;
		m_bloodAmount = Mathf.Clamp01(m_bloodAmount);
		UpdateBloodAmount();
	}

	private void OnDisable()
	{
		m_lastShownTime = Time.time;
	}

	public void SetItemInstance(MeleeWeaponItemInstance itemInstance)
	{
		m_meleeWeaponItemInstance = itemInstance;
	}

	public bool WeaponDurabilityChangeEvent(WeaponDurabilityLossType lossType)
	{
		bool flag = false;
		if (m_characterInventory != null && WeaponItemDefinition.DurabilityLossOnHit > 0f && m_associatedWeaponType != null)
		{
			float num = 0f;
			switch (lossType)
			{
			case WeaponDurabilityLossType.MeleeHit:
			{
				num = WeaponItemDefinition.DurabilityLossOnHit;
				if (m_characterInventory.ArtifactInventory.HasActiveArtifactEffect(GlobalReferences.Instance.ArtifactEffects.Generic.ReduceMeleeDurabilityLossOnAttacks, out float floatValue2))
				{
					num *= 1f - floatValue2;
				}
				if (m_characterInventory.ArtifactInventory.HasActiveArtifactEffect(GlobalReferences.Instance.ArtifactEffects.Generic.IncreasedMeleeDurabilityLossOnAttacks, out float floatValue3))
				{
					num *= 1f + floatValue3;
				}
				break;
			}
			case WeaponDurabilityLossType.Block:
			{
				num = WeaponItemDefinition.DurabilityLossOnBlock;
				if (m_characterInventory.ArtifactInventory.HasActiveArtifactEffect(GlobalReferences.Instance.ArtifactEffects.Generic.ReduceBlockingMeleeDurabilityLoss, out float floatValue))
				{
					num *= floatValue;
				}
				break;
			}
			case WeaponDurabilityLossType.Parry:
				num = WeaponItemDefinition.DurabilityLossOnParry;
				break;
			}
			MeleeWeaponItemInstance meleeWeaponItemInstance = m_characterInventory.Inventory.EquippedMeleeItem as MeleeWeaponItemInstance;
			float durabilityProportion = meleeWeaponItemInstance.DurabilityProportion;
			flag = m_characterInventory.Inventory.ApplyDurabilityLossToEquippedItem(num, m_associatedWeaponType);
			if (num > 0f || meleeWeaponItemInstance.CurrentDurability <= 0f)
			{
				GlobalReferences.Instance.EventChannels.Generic.MeleeWeaponDurabilityChanged.Raise(new MeleeWeaponDurabilityChangedData
				{
					m_meleeWeapon = meleeWeaponItemInstance,
					m_startingDurabilityProportion = durabilityProportion,
					m_endingDurabilityProportion = meleeWeaponItemInstance.DurabilityProportion
				});
			}
		}
		else if (m_meleeWeaponItemInstance != null)
		{
			flag = m_meleeWeaponItemInstance.RemoveDurability(WeaponItemDefinition.DurabilityLossOnHit);
		}
		if (flag)
		{
			if (WeaponItemDefinition.WeaponDestroyedEffectPrefab != null)
			{
				DynamicallySpawnedObject.Spawn(WeaponItemDefinition.WeaponDestroyedEffectPrefab, persistent: false, base.transform.position, Quaternion.identity);
			}
			AudioEvent weaponBreakAudioEvent = WeaponItemDefinition.WeaponBreakAudioEvent;
			if (weaponBreakAudioEvent != null)
			{
				weaponBreakAudioEvent.Play(base.transform.position);
			}
		}
		return flag;
	}

	public void AddBloodToWeapon()
	{
		m_bloodAmount += s_onHitBloodAmount;
		m_bloodAmount = Mathf.Clamp01(m_bloodAmount);
		UpdateBloodAmount();
		if (m_bloodMaterialEffect != null)
		{
			m_bloodMaterialEffect.ApplyBloodAmount(s_onHitBloodAmount);
		}
	}

	public void SetVisible(bool visible)
	{
		if (m_spriteRenderer != null)
		{
			m_spriteRenderer.enabled = visible;
		}
	}

	private void Update()
	{
		if (m_bloodAmount > 0f)
		{
			m_bloodAmount -= Time.deltaTime * m_bloodReductionRate;
			m_bloodAmount = Mathf.Clamp01(m_bloodAmount);
			UpdateBloodAmount();
		}
	}

	private void UpdateBloodAmount()
	{
		m_spriteRenderer.material.SetFloat(m_shaderPropertyID, m_bloodAmount);
	}
}
