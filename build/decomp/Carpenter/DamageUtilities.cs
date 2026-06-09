using UnityEngine;

public class DamageUtilities
{
	public static void ApplyDamage(GameObject gameObject, DamageInstance damageInstance)
	{
		ApplyDamage(gameObject.GetComponentsInChildren<IDamageable>(), damageInstance);
	}

	public static void ApplyDamage(IDamageable[] damageables, DamageInstance damageInstance)
	{
		foreach (IDamageable damageable in damageables)
		{
			if (!damageable.ShouldIgnoreDamageCategory(damageInstance.DamageCategory))
			{
				damageable.ApplyDamageInstance(damageInstance);
			}
		}
	}
}
