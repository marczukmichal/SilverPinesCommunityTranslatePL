using UnityEngine;

public class AIFollowStrategy : BaseAIStrategy
{
	[SerializeField]
	private GameObjectAnchor m_gameObjectAnchor;

	[SerializeField]
	private Transform m_moveToTransform;

	[SerializeField]
	private float m_teleportDistance = 10f;

	[Header("Sprint Settings")]
	[SerializeField]
	private float m_sprintStartDistance = 3f;

	[SerializeField]
	private float m_sprintStopDistance = 1f;

	[Header("Follow Settings")]
	[SerializeField]
	private float m_followStartDistance = 1f;

	[SerializeField]
	private float m_followStopDistance = 0.25f;

	[Tooltip("Timer for setting a minimum time the character is stationary before starting to follow again, to prevent oscilattions between following and not following which can occur")]
	[SerializeField]
	private float m_stopFollowMinimumDelayTime = 0.5f;

	[Header("Pathfinder Options")]
	[Tooltip("We only use pathfinding if the target is greater than this distance away or if the pathfinding node is closer to the target than we are")]
	[SerializeField]
	private float m_usePathfindingDistance = 3f;

	[SerializeField]
	private bool m_canJumpUsingSimpleMovement = true;

	private CharacterTargetedLeap m_characterLeap;

	private PathfindingAgent m_pathfindingAgent;

	private Rigidbody2D m_rigidbody;

	private bool m_shouldSprint;

	private bool m_shouldJump;

	private bool m_isFollowing;

	private float m_stopFollowTimer;

	private Vector2 m_moveDirection;

	public override Vector2 MoveDirection
	{
		get
		{
			if (!m_isFollowing)
			{
				return Vector2.zero;
			}
			return m_moveDirection;
		}
	}

	public override bool ShouldSprint => m_shouldSprint;

	public override bool ShouldJump => m_shouldJump;

	public void SetMoveToTransform(Transform transform)
	{
		m_moveToTransform = transform;
	}

	private void Start()
	{
		m_characterLeap = GetComponent<CharacterTargetedLeap>();
		m_pathfindingAgent = GetComponent<PathfindingAgent>();
		m_rigidbody = GetComponent<Rigidbody2D>();
	}

	public override void UpdateStrategy()
	{
		base.UpdateStrategy();
		Transform moveToTransform = m_moveToTransform;
		if (m_gameObjectAnchor != null)
		{
			moveToTransform = m_gameObjectAnchor.Item.transform;
		}
		if (moveToTransform == null)
		{
			return;
		}
		if (m_pathfindingAgent != null)
		{
			m_pathfindingAgent.SetTarget(moveToTransform.position);
		}
		float num = Vector2.Distance(moveToTransform.position, base.transform.position);
		float num2 = Mathf.Abs(moveToTransform.position.x - base.transform.position.x);
		float num3 = 0f;
		if (m_pathfindingAgent != null && m_pathfindingAgent.HasPath)
		{
			num3 = m_pathfindingAgent.PathLength;
		}
		Vector2 vector = base.transform.position;
		if (m_isFollowing)
		{
			if (num2 < m_followStopDistance && num3 < m_usePathfindingDistance)
			{
				m_stopFollowTimer = 0f;
				m_isFollowing = false;
			}
			if (m_shouldSprint)
			{
				if (num < m_sprintStopDistance)
				{
					m_shouldSprint = false;
				}
			}
			else if (num > m_sprintStartDistance)
			{
				m_shouldSprint = true;
			}
			if (num > m_teleportDistance)
			{
				Vector3 position = moveToTransform.position;
				position.y += 0.25f;
				base.transform.position = position;
				m_rigidbody.linearVelocity = Vector2.zero;
			}
			bool flag = true;
			if (m_pathfindingAgent.HasPath)
			{
				Vector2 nextNavPosition = m_pathfindingAgent.NextNavPosition;
				float num4 = Vector2.Distance(moveToTransform.position, nextNavPosition);
				m_shouldJump = false;
				PathNodeGraph.NodeData currentTargetNode = m_pathfindingAgent.CurrentTargetNode;
				PathNodeGraph.NodeData previousNode = m_pathfindingAgent.PreviousNode;
				bool flag2 = true;
				if (previousNode != null && currentTargetNode.m_type == PathNodeType.Jump && previousNode.m_type == PathNodeType.Jump)
				{
					flag2 = false;
				}
				if (!flag2 || num4 < num || num3 > m_usePathfindingDistance)
				{
					if (previousNode != null && currentTargetNode.m_type == PathNodeType.Jump && previousNode.m_type == PathNodeType.Jump)
					{
						if (currentTargetNode.m_associatedLeapObject != null && m_characterLeap != null)
						{
							m_shouldJump = true;
							m_characterLeap.ForceLeapObject(currentTargetNode.m_associatedLeapObject);
						}
						if (currentTargetNode.m_position.y > previousNode.m_position.y)
						{
							m_shouldJump = true;
						}
					}
					if (previousNode != null && currentTargetNode.m_type == PathNodeType.Ladder && previousNode.m_type == PathNodeType.Ladder)
					{
						if (currentTargetNode.m_position.y > previousNode.m_position.y)
						{
							m_moveDirection = Vector2.up;
						}
						else
						{
							m_moveDirection = Vector2.down;
						}
					}
					else
					{
						m_moveDirection = nextNavPosition - vector;
						m_moveDirection.y = 0f;
					}
					m_moveDirection.Normalize();
					flag = false;
				}
			}
			if (!flag)
			{
				return;
			}
			m_moveDirection = moveToTransform.position - base.transform.position;
			m_moveDirection.y = 0f;
			m_moveDirection.Normalize();
			if (m_characterLeap.AvailableLeapType != 0 && m_canJumpUsingSimpleMovement)
			{
				float num5 = Vector2.Distance(moveToTransform.position, m_characterLeap.DetectedLeapPosition);
				float num6 = moveToTransform.position.y - m_characterLeap.DetectedLeapPosition.y;
				if (num5 < num && num6 > -0.1f)
				{
					m_shouldJump = true;
				}
			}
		}
		else
		{
			m_shouldSprint = false;
			m_shouldJump = false;
			m_stopFollowTimer += Time.deltaTime;
			if ((num2 > m_followStartDistance || num3 > m_usePathfindingDistance) && m_stopFollowTimer >= m_stopFollowMinimumDelayTime)
			{
				m_isFollowing = true;
			}
		}
	}

	public override float DrawDebugInfo(Rect boxRect, float yPos)
	{
		Rect position = new Rect(boxRect);
		position.height = 20f;
		position.y = yPos;
		if (m_pathfindingAgent != null)
		{
			GUI.Label(position, "Has Path: " + m_pathfindingAgent.HasPath);
			position.y += position.height;
			if (m_pathfindingAgent.HasPath)
			{
				Transform moveToTransform = m_moveToTransform;
				if (m_gameObjectAnchor != null)
				{
					moveToTransform = m_gameObjectAnchor.Item.transform;
				}
				Vector2 nextNavPosition = m_pathfindingAgent.NextNavPosition;
				GUI.Label(text: "Nav Dist. target: " + Vector2.Distance(moveToTransform.position, nextNavPosition), position: position);
				position.y += position.height;
				float num = Vector2.Distance(moveToTransform.position, base.transform.position);
				float num2 = Mathf.Abs(moveToTransform.position.x - base.transform.position.x);
				GUI.Label(position, "distanceFromTarget: " + num);
				position.y += position.height;
				GUI.Label(position, "distanceFromTargetHorizontal: " + num2);
				position.y += position.height;
			}
		}
		return position.y;
	}
}
