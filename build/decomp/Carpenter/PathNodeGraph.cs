using System;
using System.Collections.Generic;
using UnityEngine;

public class PathNodeGraph : MonoBehaviour
{
	[Serializable]
	public class NodeData
	{
		[SerializeField]
		public Vector2 m_position;

		[SerializeField]
		public List<int> m_connections;

		[SerializeField]
		public PathNodeType m_type;

		[SerializeField]
		public GameObject m_associatedLeapObject;

		[NonSerialized]
		public float m_hCost;

		[NonSerialized]
		public float m_gCost;

		[NonSerialized]
		public NodeData m_parent;
	}

	[SerializeField]
	private List<NodeData> m_nodes = new List<NodeData>();

	public List<NodeData> Nodes => m_nodes;

	private void OnEnable()
	{
		GlobalReferences.Instance.Anchors.Generic.PathNodeGraphAnchor.Set(this);
	}

	private void OnDisable()
	{
		if (GlobalReferences.Instance.Anchors.Generic.PathNodeGraphAnchor.Item == this)
		{
			GlobalReferences.Instance.Anchors.Generic.PathNodeGraphAnchor.Set(null);
		}
	}

	public NodeData GetNodeDataForIndex(int index)
	{
		return m_nodes[index];
	}

	public List<NodeData> FindPath(Vector2 startPosition, Vector2 targetPosition)
	{
		NodeData nodeData = FindNearestNode(startPosition);
		NodeData nodeData2 = FindNearestNode(targetPosition);
		if (nodeData == null || nodeData2 == null)
		{
			return null;
		}
		List<NodeData> list = new List<NodeData>();
		HashSet<NodeData> hashSet = new HashSet<NodeData>();
		nodeData.m_gCost = 0f;
		nodeData.m_hCost = CalculateDistance(nodeData, nodeData2);
		nodeData.m_parent = null;
		list.Add(nodeData);
		while (list.Count > 0)
		{
			NodeData lowestFCostNode = GetLowestFCostNode(list);
			if (lowestFCostNode == nodeData2)
			{
				return GeneratePath(lowestFCostNode);
			}
			list.Remove(lowestFCostNode);
			hashSet.Add(lowestFCostNode);
			for (int i = 0; i < lowestFCostNode.m_connections.Count; i++)
			{
				NodeData nodeData3 = m_nodes[lowestFCostNode.m_connections[i]];
				if (hashSet.Contains(nodeData3))
				{
					continue;
				}
				float num = lowestFCostNode.m_gCost + CalculateDistance(lowestFCostNode, nodeData3);
				if (!list.Contains(nodeData3) || num < nodeData3.m_gCost)
				{
					nodeData3.m_gCost = num;
					nodeData3.m_hCost = CalculateDistance(nodeData3, nodeData2);
					nodeData3.m_parent = lowestFCostNode;
					if (!list.Contains(nodeData3))
					{
						list.Add(nodeData3);
					}
				}
			}
		}
		return null;
	}

	private NodeData FindNearestNode(Vector2 position)
	{
		NodeData result = null;
		float num = float.MaxValue;
		for (int i = 0; i < m_nodes.Count; i++)
		{
			NodeData nodeData = m_nodes[i];
			float num2 = Vector2.Distance(position, nodeData.m_position);
			if (num2 < num && Physics2D.Linecast(position, nodeData.m_position, GameLayers.CatEnvironmentMask).collider == null)
			{
				result = nodeData;
				num = num2;
			}
		}
		return result;
	}

	private float CalculateDistance(NodeData nodeA, NodeData nodeB)
	{
		return Vector2.Distance(nodeA.m_position, nodeB.m_position);
	}

	private NodeData GetLowestFCostNode(List<NodeData> nodeList)
	{
		NodeData nodeData = nodeList[0];
		for (int i = 1; i < nodeList.Count; i++)
		{
			if (nodeList[i].m_gCost + nodeList[i].m_hCost < nodeData.m_gCost + nodeData.m_hCost)
			{
				nodeData = nodeList[i];
			}
		}
		return nodeData;
	}

	private List<NodeData> GeneratePath(NodeData endNode)
	{
		List<NodeData> list = new List<NodeData>();
		for (NodeData nodeData = endNode; nodeData != null; nodeData = nodeData.m_parent)
		{
			list.Add(nodeData);
		}
		list.Reverse();
		return list;
	}
}
