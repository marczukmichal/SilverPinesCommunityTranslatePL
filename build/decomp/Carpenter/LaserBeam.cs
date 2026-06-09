using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LaserBeam : MonoBehaviour
{
	[SerializeField]
	private float m_maxDistance;

	[SerializeField]
	private float m_minDistance;

	[SerializeField]
	private LayerMask m_layerMask;

	[SerializeField]
	private float m_onHitOverlap;

	[SerializeField]
	private LineRenderer m_lineRenderer;

	private void Reset()
	{
		m_lineRenderer = GetComponent<LineRenderer>();
	}

	private void OnEnable()
	{
		m_lineRenderer.positionCount = 0;
		m_lineRenderer.enabled = false;
	}

	private void Update()
	{
		Vector3 position = base.transform.position;
		Vector3 vector = position;
		Vector3 forward = base.transform.forward;
		vector += forward * m_minDistance;
		RaycastHit2D raycastHit2D = Physics2D.Raycast(vector, forward, m_maxDistance - m_minDistance, m_layerMask);
		Vector3 position2;
		if ((bool)raycastHit2D.collider)
		{
			position2 = raycastHit2D.point;
			position2 += m_onHitOverlap * forward;
		}
		else
		{
			position2 = vector + (m_maxDistance - m_minDistance) * forward;
		}
		position2.z = position.z;
		m_lineRenderer.positionCount = 2;
		m_lineRenderer.SetPosition(0, position);
		m_lineRenderer.SetPosition(1, position2);
		m_lineRenderer.enabled = true;
	}
}
