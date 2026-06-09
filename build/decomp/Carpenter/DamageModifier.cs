using System;
using UnityEngine;

[Serializable]
public class DamageModifier
{
	public enum ModifierBehavior
	{
		Normal,
		OnlyFromFront,
		OnlyFromBack
	}

	[SerializeField]
	private bool m_applyToAllDamageTypes;

	[SerializeField]
	[Tooltip("Damage type to check against")]
	private DamageType m_damageType;

	[SerializeField]
	[Tooltip("If true, then this modifier is applied if the damage type isn't present, instead of when the damage type is present")]
	private bool m_invertCheck;

	[SerializeField]
	private float m_healthDamageMultiplier = 1f;

	[SerializeField]
	private float m_statusEffectMultiplier = 1f;

	[SerializeField]
	private ModifierBehavior m_behavior;

	public bool AppliesToAllDamageTypes => m_applyToAllDamageTypes;

	public DamageType DamageType => m_damageType;

	public bool InvertCheck => m_invertCheck;

	public float GetHealthDamageMultiplier(DamageType damageType)
	{
		if (!AppliesToDamageType(damageType))
		{
			return 1f;
		}
		return m_healthDamageMultiplier;
	}

	public float GetStatusEffectMultiplier(DamageType damageType)
	{
		if (!AppliesToDamageType(damageType))
		{
			return 1f;
		}
		return m_statusEffectMultiplier;
	}

	private bool AppliesToDamageType(DamageType damageType)
	{
		if (m_applyToAllDamageTypes)
		{
			return true;
		}
		bool flag = m_damageType == damageType;
		if (m_invertCheck)
		{
			flag = !flag;
		}
		return flag;
	}

	public bool AppliesToDamageInstance(GameObject gameObject, DamageInstance damageInstance)
	{
		bool result = AppliesToDamageType(damageInstance.DamageType);
		if (m_behavior == ModifierBehavior.OnlyFromFront)
		{
			CharacterDirection componentInParent = gameObject.GetComponentInParent<CharacterDirection>();
			if (componentInParent != null && componentInParent.IsFacingDirection(damageInstance.Direction.x))
			{
				result = false;
			}
		}
		else if (m_behavior == ModifierBehavior.OnlyFromBack)
		{
			CharacterDirection componentInParent2 = gameObject.GetComponentInParent<CharacterDirection>();
			if (componentInParent2 != null && !componentInParent2.IsFacingDirection(damageInstance.Direction.x))
			{
				result = false;
			}
		}
		return result;
	}
}
