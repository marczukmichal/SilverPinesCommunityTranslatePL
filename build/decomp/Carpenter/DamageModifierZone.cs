using UnityEngine;

public class DamageModifierZone : MonoBehaviour
{
	public enum DamageModifierShape
	{
		Circle
	}

	[SerializeField]
	private DamageModifierShape m_damageModifierShape;

	[SerializeField]
	private float m_radius;

	[SerializeField]
	private DamageModifier m_damageModifier;

	[SerializeField]
	private bool m_countsAsWeakpoint;

	public float Radius => m_radius;

	public bool CountsAsWeakpoint => m_countsAsWeakpoint;

	public DamageModifier DamageModifier => m_damageModifier;

	public bool IsHit(Vector2 impactPosition)
	{
		Vector2 b = base.transform.position;
		if (m_damageModifierShape == DamageModifierShape.Circle)
		{
			return Vector2.Distance(impactPosition, b) < m_radius;
		}
		return false;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(base.transform.position, m_radius);
		Gizmos.color = Color.white;
	}
}
