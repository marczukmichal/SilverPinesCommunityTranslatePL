using System.Collections.Generic;
using UnityEngine;

public class PathfindingAgent : MonoBehaviour
{
	[Tooltip("How often per second to update")]
	[SerializeField]
	private float m_updateRate = 2f;

	[SerializeField]
	private float m_reachedTargetDistance = 0.5f;

	private List<PathNodeGraph.NodeData> m_calculatedPath;

	private float m_updateTimer;

	private Vector2 m_goalPosition;

	private bool m_goalIsValid;

	private bool m_isAtEndOfPath;

	private int m_currentTargetNodeIndex;

	private LegacyCharacterMovement m_movement;

	private PathNodeGraph.NodeData m_previousNode;

	public bool HasPath => m_calculatedPath != null;

	public bool IsAtEndOfPath => m_isAtEndOfPath;

	public PathNodeGraph.NodeData PreviousNode => m_previousNode;

	public PathNodeGraph.NodeData CurrentTargetNode
	{
		get
		{
			if (m_calculatedPath == null || m_calculatedPath.Count == 0)
			{
				return null;
			}
			return m_calculatedPath[m_currentTargetNodeIndex];
		}
	}

	public Vector2 NextNavPosition
	{
		get
		{
			if (m_calculatedPath == null || m_calculatedPath.Count == 0)
			{
				return base.transform.position;
			}
			if (m_isAtEndOfPath && CurrentTargetNode.m_type == PathNodeType.Normal)
			{
				return m_goalPosition;
			}
			return CurrentTargetNode.m_position;
		}
	}

	public float PathLength
	{
		get
		{
			if (m_calculatedPath == null || m_calculatedPath.Count == 0)
			{
				return 0f;
			}
			float num = 0f;
			for (int i = 0; i < m_calculatedPath.Count - 1; i++)
			{
				PathNodeGraph.NodeData nodeData = m_calculatedPath[i];
				PathNodeGraph.NodeData nodeData2 = m_calculatedPath[i + 1];
				num += Vector2.Distance(nodeData.m_position, nodeData2.m_position);
			}
			return num;
		}
	}

	private void Start()
	{
		m_movement = GetComponent<LegacyCharacterMovement>();
	}

	private bool ShouldFreezePathState()
	{
		if (m_movement != null && m_movement.AttachedLadder != null)
		{
			return true;
		}
		return false;
	}

	public void SetTarget(Vector2 position)
	{
		m_goalPosition = position;
		m_goalIsValid = true;
	}

	private void Update()
	{
		if (!m_goalIsValid)
		{
			return;
		}
		PathNodeGraph item = GlobalReferences.Instance.Anchors.Generic.PathNodeGraphAnchor.Item;
		if (item == null || ShouldFreezePathState())
		{
			return;
		}
		if (HasPath)
		{
			Vector2 b = base.transform.position;
			float num = Vector2.Distance(CurrentTargetNode.m_position, b);
			if (Mathf.Abs(CurrentTargetNode.m_position.x - b.x) < m_reachedTargetDistance && num < 1f)
			{
				if (m_currentTargetNodeIndex < m_calculatedPath.Count - 1)
				{
					m_previousNode = m_calculatedPath[m_currentTargetNodeIndex];
					m_currentTargetNodeIndex++;
				}
				else
				{
					m_isAtEndOfPath = true;
				}
			}
		}
		m_updateTimer -= Time.deltaTime;
		if (!(m_updateTimer <= 0f))
		{
			return;
		}
		PathNodeGraph.NodeData currentTargetNode = CurrentTargetNode;
		Vector2 vector = base.transform.position;
		Vector2 goalPosition = m_goalPosition;
		vector.y += 0.5f;
		goalPosition.y += 0.5f;
		m_calculatedPath = item.FindPath(vector, goalPosition);
		m_updateTimer = 1f / m_updateRate;
		if (m_calculatedPath == null)
		{
			return;
		}
		if (currentTargetNode != null && m_calculatedPath.Contains(currentTargetNode))
		{
			m_currentTargetNodeIndex = m_calculatedPath.IndexOf(currentTargetNode);
			if (m_currentTargetNodeIndex == m_calculatedPath.Count - 1)
			{
				m_isAtEndOfPath = true;
			}
			else
			{
				m_isAtEndOfPath = false;
			}
			if (m_previousNode == CurrentTargetNode)
			{
				m_previousNode = null;
			}
			return;
		}
		m_isAtEndOfPath = false;
		m_currentTargetNodeIndex = 0;
		if (m_calculatedPath.Count >= 2 && m_calculatedPath[0].m_type == PathNodeType.Normal)
		{
			float num2 = Vector2.Distance(m_calculatedPath[0].m_position, goalPosition);
			if (Vector2.Distance(m_calculatedPath[1].m_position, goalPosition) < num2 && Physics2D.Linecast(vector, m_calculatedPath[1].m_position, GameLayers.CatEnvironmentMask).collider == null)
			{
				m_currentTargetNodeIndex = 1;
			}
		}
	}

	private void OnDrawGizmos()
	{
		if (m_calculatedPath != null)
		{
			Gizmos.color = Color.green;
			for (int i = 0; i < m_calculatedPath.Count - 1; i++)
			{
				PathNodeGraph.NodeData nodeData = m_calculatedPath[i];
				GizmoExtensions.DrawToFromArrow(positionEnd: m_calculatedPath[i + 1].m_position, positionStart: nodeData.m_position);
			}
			if (CurrentTargetNode != null)
			{
				Gizmos.color = Color.cyan;
				Gizmos.DrawSphere(CurrentTargetNode.m_position, 0.25f);
			}
			if (PreviousNode != null)
			{
				Gizmos.color = Color.red;
				Gizmos.DrawSphere(PreviousNode.m_position, 0.25f);
			}
			Gizmos.color = Color.white;
		}
	}
}
