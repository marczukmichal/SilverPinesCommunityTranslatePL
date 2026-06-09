using UnityEngine;

public class PersistentPositionBlocker : MonoBehaviour
{
	[SerializeField]
	private float m_radius;

	[SerializeField]
	private Vector2 m_preferedLocation;

	public Vector3 GetPreferredLocation()
	{
		return base.transform.TransformPoint(m_preferedLocation);
	}

	private void OnDrawGizmosSelected()
	{
		Color magenta = Color.magenta;
		magenta.a = 0.1f;
		Gizmos.color = magenta;
		Gizmos.DrawWireSphere(base.transform.position, m_radius);
		Gizmos.color = Color.magenta;
		GizmoExtensions.DrawToFromArrow(base.transform.position, GetPreferredLocation());
		Gizmos.color = Color.white;
	}

	public bool ContainsPosition(Vector2 position)
	{
		return Vector2.Distance(base.transform.position, position) < m_radius;
	}
}
