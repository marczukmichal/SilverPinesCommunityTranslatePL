using System;
using UnityEngine;

[DisallowMultipleComponent]
public class AIWanderStrategy : BaseAIStrategy, IPersistentComponent
{
	[Serializable]
	private class PersistentData
	{
		public Vector3 m_startingPosition;

		public Vector3 m_currentGoal;
	}

	[SerializeField]
	private float m_goalDistance = 1f;

	[ShowInDesignerInspector]
	[SerializeField]
	private float m_maxWanderDistance = 10f;

	[SerializeField]
	private CapsuleCollider2D m_movementCollider;

	[ShowInDesignerInspector]
	[SerializeField]
	private Vector2 m_minMaxWaitTime = new Vector2(3f, 6f);

	[SerializeField]
	private Transform m_overrideHomeBase;

	private float m_waitTimer;

	private Vector2 m_moveDirection = Vector2.zero;

	private Vector3 m_localStartingPosition = Vector3.zero;

	private Vector3 m_localGoal = Vector3.zero;

	private CharacterDirection m_characterDirection;

	private CharacterMovement m_characterMovement;

	private AISenses m_aiSenses;

	private bool m_isMovingRight = true;

	private PersistentDataObject m_persistentDataObject;

	private PersistentData m_persistentData;

	public override Vector2 MoveDirection => m_moveDirection;

	public bool RequiresPersistentData()
	{
		return true;
	}

	public void ReceiveDataStoreEntry(PersistentDataObject dataEntry)
	{
		m_persistentDataObject = dataEntry;
		m_persistentData = ((m_persistentDataObject.Data != null) ? (m_persistentDataObject.Data as PersistentData) : null);
		if (m_persistentData != null)
		{
			SetMoveDirectionFlag();
			return;
		}
		m_persistentData = new PersistentData();
		m_persistentDataObject.Data = m_persistentData;
		if (m_overrideHomeBase != null)
		{
			m_persistentData.m_startingPosition = m_overrideHomeBase.position;
		}
		else
		{
			m_persistentData.m_startingPosition = base.transform.position;
		}
		SelectNewWanderPoint(keepDirection: true);
	}

	private void Start()
	{
		m_localStartingPosition = base.transform.position;
		m_characterDirection = GetComponent<CharacterDirection>();
		m_characterMovement = GetComponent<CharacterMovement>();
		m_aiSenses = GetComponent<AISenses>();
	}

	public override void EnableStrategy()
	{
		base.EnableStrategy();
		SelectNewWanderPoint(keepDirection: true);
	}

	private void SelectNewWanderPoint(bool keepDirection)
	{
		m_waitTimer = m_minMaxWaitTime.GetRandom();
		Vector3 vector = ((m_persistentData != null) ? m_persistentData.m_startingPosition : m_localStartingPosition);
		if (keepDirection && m_characterDirection != null)
		{
			float maxInclusive;
			float minInclusive;
			if (m_characterDirection.CurrentDirection == CharacterDirection.Facing.Left)
			{
				maxInclusive = m_localStartingPosition.x;
				minInclusive = m_localStartingPosition.x - m_maxWanderDistance * 0.5f;
			}
			else
			{
				minInclusive = m_localStartingPosition.x;
				maxInclusive = m_localStartingPosition.x + m_maxWanderDistance * 0.5f;
			}
			vector.x = UnityEngine.Random.Range(minInclusive, maxInclusive);
		}
		else
		{
			vector.x += UnityEngine.Random.Range(m_maxWanderDistance * -0.5f, m_maxWanderDistance * 0.5f);
		}
		if (m_persistentData != null)
		{
			m_persistentData.m_currentGoal = vector;
		}
		else
		{
			m_localGoal = vector;
		}
		SetMoveDirectionFlag();
	}

	private void SetMoveDirectionFlag()
	{
		m_isMovingRight = ((m_persistentData != null) ? m_persistentData.m_currentGoal : m_localGoal).x > base.transform.position.x;
	}

	public override void UpdateStrategy()
	{
		Vector3 position = base.transform.position;
		Vector3 vector = ((m_persistentData != null) ? m_persistentData.m_currentGoal : m_localGoal);
		float num = Mathf.Abs(position.x - vector.x);
		Vector2 zero = Vector2.zero;
		if (num < m_goalDistance || AIUtilities.IsInFrontOfWall(m_movementCollider, m_characterDirection.GetForwardVector(), m_characterMovement.CollisionMask))
		{
			m_waitTimer -= Time.deltaTime;
			bool flag = true;
			if (m_aiSenses != null && m_aiSenses.CurrentTargetState != 0 && m_aiSenses.IsLookingAtTargetSuspicious())
			{
				flag = false;
			}
			if (m_waitTimer <= 0f && flag)
			{
				SelectNewWanderPoint(keepDirection: false);
			}
		}
		zero.x = vector.x - position.x;
		if ((zero.x < 0f && m_isMovingRight) || (zero.x > 0f && !m_isMovingRight))
		{
			m_moveDirection = Vector2.zero;
		}
		else
		{
			m_moveDirection = zero.normalized;
		}
	}

	private void OnDrawGizmos()
	{
		if (Application.isPlaying)
		{
			GizmoExtensions.DrawToFromArrow(base.transform.position, (m_persistentData != null) ? m_persistentData.m_currentGoal : m_localGoal, 0.1f, 0.1f);
		}
	}
}
