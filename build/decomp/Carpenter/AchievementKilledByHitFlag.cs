using System;
using UnityEngine;
using UnityEngine.Events;

public class AchievementKilledByHitFlag : MonoBehaviour
{
	[SerializeField]
	private AchievementDefinition m_achievementDefinition;

	[SerializeField]
	private HitFlags m_hitFlag;

	private void Start()
	{
		CharacterHealth component = GetComponent<CharacterHealth>();
		component.OnDying = (UnityAction<DamageInstance>)Delegate.Combine(component.OnDying, new UnityAction<DamageInstance>(OnDyingEvent));
	}

	private void OnDyingEvent(DamageInstance killingBlow)
	{
		if (killingBlow.HitFlags.HasFlag(m_hitFlag))
		{
			m_achievementDefinition.UnlockAchievement();
		}
	}
}
