using System.Collections.Generic;
using UnityEngine;

public class StatusEffectHitResults
{
	public class StatusEffectHitInstance
	{
		[SerializeField]
		public StatusEffectDefinition m_statusEffect;

		[SerializeField]
		public int m_amountToAdd;
	}

	[SerializeField]
	public List<StatusEffectHitInstance> m_statusEffects;

	public StatusEffectHitResults()
	{
		m_statusEffects = new List<StatusEffectHitInstance>();
	}

	public void AddStatusEffect(StatusEffectDefinition statusEffect, int amount)
	{
		m_statusEffects.Add(new StatusEffectHitInstance
		{
			m_statusEffect = statusEffect,
			m_amountToAdd = amount
		});
	}
}
