using UnityEngine;

public class BackgroundPhysicsObject : MonoBehaviour, IDamageable
{
	public bool AllowPassThroughProjectile()
	{
		return true;
	}

	public bool ShouldConsumeMeleeHit()
	{
		return false;
	}

	public void ApplyDamageInstance(DamageInstance instance)
	{
	}
}
