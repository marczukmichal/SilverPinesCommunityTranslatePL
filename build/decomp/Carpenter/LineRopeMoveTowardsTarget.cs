using UnityEngine;

public class LineRopeMoveTowardsTarget : MonoBehaviour
{
	[SerializeField]
	private Transform m_target;

	[SerializeField]
	private GameObjectAnchor m_anchor;

	[SerializeField]
	private float m_forceMultiplier;

	[SerializeField]
	private float m_maxDistance;

	[SerializeField]
	private LineRope m_lineRope;

	private void FixedUpdate()
	{
		Vector2 vector = Vector3.zero;
		bool flag = false;
		if (m_anchor != null)
		{
			if (m_anchor.Item != null)
			{
				vector = m_anchor.Item.transform.position;
				flag = true;
			}
		}
		else if ((bool)m_target)
		{
			vector = m_target.position;
			flag = true;
		}
		if (flag)
		{
			Vector2 vector2 = m_lineRope.EndOfRopeWorldPosition;
			Vector3 vector3 = vector - vector2;
			if (Vector2.Distance(vector, vector2) < m_maxDistance)
			{
				Vector2 vector4 = vector3 * m_forceMultiplier;
				m_lineRope.AddVelocityToRopeEnd(vector4);
			}
		}
	}
}
