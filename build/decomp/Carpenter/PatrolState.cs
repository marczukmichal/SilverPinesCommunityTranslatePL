using System;
using UnityEngine;

[Serializable]
public class PatrolState
{
	[SerializeField]
	private int m_nodeIndex;

	private Vector2 m_moveDirection;

	private float m_waitTimer;

	public Vector3 MoveDirection => m_moveDirection;

	public PatrolState()
	{
		m_nodeIndex = 0;
		m_moveDirection = Vector2.zero;
		m_waitTimer = 0f;
	}

	public void Update(PatrolPath path, float goalDistance, Vector3 currentPosition)
	{
		PatrolPath.Node nodeAtIndex = path.GetNodeAtIndex(m_nodeIndex);
		float num = Mathf.Abs(currentPosition.x - nodeAtIndex.Position.x);
		Vector2 zero = Vector2.zero;
		if (num < goalDistance)
		{
			m_waitTimer += Time.deltaTime;
			if (m_waitTimer >= nodeAtIndex.WaitTime)
			{
				m_nodeIndex = path.GetNextIndex(m_nodeIndex);
			}
		}
		else
		{
			m_waitTimer = 0f;
			zero.x = nodeAtIndex.Position.x - currentPosition.x;
		}
		m_moveDirection = zero.normalized;
	}
}
