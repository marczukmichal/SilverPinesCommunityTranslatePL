public interface IDamageable
{
	void ApplyDamageInstance(DamageInstance instance);

	bool AllowPassThroughProjectile()
	{
		return false;
	}

	bool AllowProjectilePenetration()
	{
		return true;
	}

	bool ShouldDeflectHit(bool isArmorPiercing)
	{
		return false;
	}

	bool ShouldForceMeleeRebound()
	{
		return false;
	}

	bool ShouldConsumeMeleeHit()
	{
		return false;
	}

	bool ShouldIgnoreDamageCategory(DamageCategory damageCategory)
	{
		return false;
	}

	ConsumeHitType GetConsumeHitType()
	{
		return ConsumeHitType.Never;
	}

	bool ShouldConsumeHit(bool isArmorPiercing)
	{
		return GetConsumeHitType() switch
		{
			ConsumeHitType.Never => false, 
			ConsumeHitType.NotArmorPiercing => !isArmorPiercing, 
			ConsumeHitType.Always => true, 
			_ => false, 
		};
	}
}
