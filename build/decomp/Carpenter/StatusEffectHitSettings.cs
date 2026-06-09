using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct StatusEffectHitSettings
{
	[Serializable]
	public struct Effect
	{
		[Range(0f, 1f)]
		[SerializeField]
		public float m_chanceToApply;

		[SerializeField]
		public StatusEffectDefinition m_statusEffect;

		[SerializeField]
		public int m_amountToAdd;

		[SerializeField]
		public bool m_ignoreBlock;
	}

	[SerializeField]
	public List<Effect> m_statusEffects;

	public StatusEffectHitResults GenerateResult(bool isBlocked)
	{
		StatusEffectHitResults statusEffectHitResults = new StatusEffectHitResults();
		if (m_statusEffects != null)
		{
			foreach (Effect statusEffect in m_statusEffects)
			{
				if (!(!statusEffect.m_ignoreBlock && isBlocked) && (!(statusEffect.m_chanceToApply < 1f) || !(UnityEngine.Random.Range(0f, 1f) > statusEffect.m_chanceToApply)))
				{
					statusEffectHitResults.AddStatusEffect(statusEffect.m_statusEffect, statusEffect.m_amountToAdd);
				}
			}
			return statusEffectHitResults;
		}
		return statusEffectHitResults;
	}
}
