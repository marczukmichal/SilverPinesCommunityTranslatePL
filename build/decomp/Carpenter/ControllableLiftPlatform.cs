using System;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class ControllableLiftPlatform : MovingPlatform, IPersistentComponent
{
	[Serializable]
	private class PlatformWaypoint
	{
		public Vector3 m_offset;

		public UnityEvent m_onArrivedEvent;

		public UnityEvent m_onDepartedEvent;
	}

	[Serializable]
	private class PersistentData
	{
		public int m_index;
	}

	private Vector3 m_rootPosition;

	private int m_currentIndex;

	private int m_targetIndex;

	private bool m_isMoving;

	public UnityEvent m_onStartMovingEvent;

	public UnityEvent m_onStopMovingEvent;

	[SerializeField]
	private PlatformWaypoint[] m_waypoints;

	[SerializeField]
	private float m_moveSpeed = 10f;

	private PersistentDataObject m_persistentDataObject;

	private PersistentData m_persistentData;

	protected override void Awake()
	{
		base.Awake();
		m_rootPosition = base.transform.position;
	}

	public void MoveToIndex(int index)
	{
		if (base.isActiveAndEnabled && m_targetIndex != index)
		{
			if (m_persistentData != null)
			{
				m_persistentData.m_index = index;
			}
			m_targetIndex = index;
			m_isMoving = true;
			m_waypoints[m_currentIndex].m_onDepartedEvent.Invoke();
			m_onStartMovingEvent.Invoke();
			Vector3 position = base.transform.position;
			Vector3 vector = (m_rootPosition + m_waypoints[m_targetIndex].m_offset - position).normalized * m_moveSpeed;
			m_rigidbody.linearVelocity = vector;
		}
	}

	private void ReloadJumpToIndex(int index)
	{
		m_currentIndex = (m_targetIndex = index);
		m_waypoints[index].m_onArrivedEvent.Invoke();
		m_rigidbody.position = m_waypoints[index].m_offset + m_rootPosition;
	}

	private void FixedUpdate()
	{
		if (m_isMoving)
		{
			Vector2 vector = m_rootPosition + m_waypoints[m_targetIndex].m_offset;
			if (Vector2.Distance(Vector2.MoveTowards(m_rigidbody.position, vector, m_rigidbody.linearVelocity.magnitude * Time.fixedDeltaTime), vector) < 0.001f)
			{
				m_isMoving = false;
				m_currentIndex = m_targetIndex;
				m_waypoints[m_currentIndex].m_onArrivedEvent.Invoke();
				m_onStopMovingEvent.Invoke();
				m_rigidbody.MovePosition(vector);
				m_rigidbody.linearVelocity = Vector2.zero;
			}
		}
	}

	private void OnDrawGizmos()
	{
		if (m_waypoints != null)
		{
			Vector3 vector = m_rootPosition;
			if (m_rootPosition.Equals(Vector3.zero))
			{
				vector = base.transform.position;
			}
			Vector2 vector2 = GetComponent<Collider2D>().bounds.size;
			Vector3 size = new Vector3(vector2.x, vector2.y, 0.5f);
			for (int i = 0; i < m_waypoints.Length; i++)
			{
				Gizmos.DrawWireCube(vector + m_waypoints[i].m_offset, size);
			}
			for (int j = 0; j < m_waypoints.Length - 1; j++)
			{
				Vector3 positionStart = vector + m_waypoints[j].m_offset;
				Vector3 positionEnd = vector + m_waypoints[j + 1].m_offset;
				GizmoExtensions.DrawToFromArrow(positionStart, positionEnd);
			}
		}
	}

	public bool RequiresPersistentData()
	{
		return true;
	}

	public void ReceiveDataStoreEntry(PersistentDataObject dataEntry)
	{
		m_persistentDataObject = dataEntry;
		m_persistentData = ((m_persistentDataObject.Data != null) ? (m_persistentDataObject.Data as PersistentData) : null);
		int index = 0;
		if (m_persistentData != null)
		{
			index = m_persistentData.m_index;
		}
		else
		{
			m_persistentData = new PersistentData();
			m_persistentDataObject.Data = m_persistentData;
		}
		ReloadJumpToIndex(index);
	}
}
