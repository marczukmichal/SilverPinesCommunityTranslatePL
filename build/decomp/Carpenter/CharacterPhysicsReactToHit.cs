using UnityEngine;

public class CharacterPhysicsReactToHit : MonoBehaviour, IDamageable
{
	[SerializeField]
	private Rigidbody2D m_rigidbody2D;

	[SerializeField]
	private float m_scalar = 1f;

	public void ApplyDamageInstance(DamageInstance instance)
	{
		Vector2 direction = instance.Direction;
		direction *= instance.ImpactForce * m_scalar;
		m_rigidbody2D.AddForce(direction);
	}
}
