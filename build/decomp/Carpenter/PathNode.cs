using System.Collections.Generic;
using UnityEngine;

public class PathNode : MonoBehaviour
{
	private static readonly float s_connectionDistance = 5f;

	[SerializeField]
	private bool m_shouldAutoConnect = true;

	[SerializeField]
	private List<PathNode> m_forceConnections;

	[SerializeField]
	private PathNodeType m_pathNodeType;

	[SerializeField]
	private GameObject m_associatedLeapObject;

	public bool ShouldAutoConnect => m_shouldAutoConnect;

	public GameObject AssociatedLeapObject => m_associatedLeapObject;

	public PathNodeType NodeType => m_pathNodeType;

	public bool IsConnectedTo(PathNode otherNode)
	{
		if (m_forceConnections.Contains(otherNode))
		{
			return true;
		}
		if (!ShouldAutoConnect || !otherNode.ShouldAutoConnect)
		{
			return false;
		}
		if (Vector2.Distance(base.transform.position, otherNode.transform.position) > s_connectionDistance)
		{
			return false;
		}
		Vector2 direction = otherNode.transform.position - base.transform.position;
		return Physics2D.Raycast(base.transform.position, direction, direction.magnitude, GameLayers.EnvironmentMask).collider == otherNode.GetComponent<Collider2D>();
	}
}
