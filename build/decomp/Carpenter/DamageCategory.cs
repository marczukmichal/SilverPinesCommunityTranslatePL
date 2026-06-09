using System;

[Flags]
public enum DamageCategory
{
	Unknown = 0,
	DamageCollider = 1,
	Projectile = 2,
	Explosion = 4,
	Parry = 0x10
}
