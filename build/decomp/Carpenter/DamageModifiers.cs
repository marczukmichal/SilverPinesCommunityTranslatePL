using System.Collections.Generic;
using UnityEngine;

public class DamageModifiers : MonoBehaviour
{
	[SerializeField]
	private List<DamageModifier> m_modifiers;

	[Tooltip("If true then hitting this enemy with melee does not count towards a melee weapons max hits per activation count")]
	[SerializeField]
	private bool m_ignoreForMeleeHitCount;

	public IReadOnlyCollection<DamageModifier> Modifiers => m_modifiers.AsReadOnly();

	public bool IgnoreForMeleeHitCount => m_ignoreForMeleeHitCount;

	public static void ApplyDamageModifiers(IReadOnlyCollection<DamageModifier> modifiers, GameObject gameObject, ref int healthDamageAmount, ref bool shouldIgnore, DamageInstance damageInstance)
	{
		List<DamageType> list = new List<DamageType>();
		foreach (DamageModifier modifier in modifiers)
		{
			if (modifier.DamageType != null && !modifier.InvertCheck)
			{
				list.Add(modifier.DamageType);
			}
		}
		float num = 1f;
		foreach (DamageModifier modifier2 in modifiers)
		{
			if (modifier2.AppliesToDamageInstance(gameObject, damageInstance) && ((!modifier2.InvertCheck && !modifier2.AppliesToAllDamageTypes) || !list.Contains(damageInstance.DamageType)))
			{
				if (shouldIgnore)
				{
					shouldIgnore = false;
				}
				healthDamageAmount = Mathf.RoundToInt((float)healthDamageAmount * modifier2.GetHealthDamageMultiplier(damageInstance.DamageType));
				num *= modifier2.GetStatusEffectMultiplier(damageInstance.DamageType);
			}
		}
		foreach (StatusEffectHitResults.StatusEffectHitInstance statusEffect in damageInstance.StatusEffects.m_statusEffects)
		{
			statusEffect.m_amountToAdd = Mathf.RoundToInt((float)statusEffect.m_amountToAdd * num);
		}
	}
}
