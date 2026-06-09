using UnityEngine;

public class VisibilityLightSource : MonoBehaviour
{
	[SerializeField]
	private float m_radius = 5f;

	public bool BoundIsLit(Bounds bound)
	{
		return Vector2.Distance(bound.ClosestPoint(base.transform.position), base.transform.position) < m_radius;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.white;
		Gizmos.DrawWireSphere(base.transform.position, m_radius);
	}

	private void OnEnable()
	{
		GlobalReferences.Instance.Sets.Generic.VisibilityLightSourcesSet.Add(this);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.Sets.Generic.VisibilityLightSourcesSet.Remove(this);
	}
}
