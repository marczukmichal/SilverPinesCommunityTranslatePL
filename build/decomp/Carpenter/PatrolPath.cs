using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class PatrolPath : MonoBehaviour
{
	public class Node
	{
		private Vector3 m_position;

		private float m_waitTime;

		public Vector3 Position => m_position;

		public float WaitTime => m_waitTime;

		public Node(GameObject nodeObject)
		{
			m_position = nodeObject.transform.position;
			m_waitTime = 1f;
		}
	}

	private List<Node> m_nodes;

	private List<Node> Nodes => m_nodes;

	private void Awake()
	{
		RebuildNodes();
	}

	private void RebuildNodes()
	{
		m_nodes = new List<Node>();
		for (int i = 0; i < base.transform.childCount; i++)
		{
			m_nodes.Add(new Node(base.transform.GetChild(i).gameObject));
		}
	}

	public Node GetNodeAtIndex(int index)
	{
		return Nodes[index];
	}

	public int NodeCount()
	{
		return Nodes.Count;
	}

	public int GetNextIndex(int currentIndex)
	{
		return (currentIndex + 1) % Nodes.Count;
	}

	private void OnDrawGizmos()
	{
		RebuildNodes();
		for (int i = 0; i < m_nodes.Count; i++)
		{
			Vector3 position = m_nodes[i].Position;
			Vector3 position2 = m_nodes[(i + 1) % m_nodes.Count].Position;
			GizmoExtensions.DrawToFromArrow(position, position2, 0.1f, 0.1f);
		}
	}
}
