using System;

[Flags]
public enum HitFlags
{
	None = 0,
	SupressReaction = 1,
	DontTriggerDamageBlock = 2,
	IgnoreDamageBlock = 4,
	OnlyHitKnockedDown = 8,
	NonLethal = 0x10,
	IsBottle = 0x400
}
