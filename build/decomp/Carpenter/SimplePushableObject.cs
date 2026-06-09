using UnityEngine;
using UnityEngine.Events;

public class SimplePushableObject : BasePushableObject
{
	[SerializeField]
	private float m_slowRate = 10f;

	[SerializeField]
	private float m_maxMoveLeftDistance = 5f;

	[SerializeField]
	private float m_maxMoveRightDistance = 5f;

	[SerializeField]
	private UnityEvent m_onFullyExtendedLeft;

	[SerializeField]
	private UnityEvent m_onFullyExtendedRight;

	private float m_currentMoveSpeed;

	private float m_minPosition;

	private float m_maxPosition;

	protected override void Awake()
	{
		base.Awake();
		m_minPosition = base.transform.position.x - m_maxMoveLeftDistance;
		m_maxPosition = base.transform.position.x + m_maxMoveRightDistance;
	}

	public override void DisableMovement()
	{
		m_currentMoveSpeed = 0f;
	}

	public override float GetCurrentMoveSpeed()
	{
		return m_currentMoveSpeed;
	}

	public override void SetMoveSpeed(float speed)
	{
		m_currentMoveSpeed = 0f - speed;
	}

	public override void TrySlowDown(float basePushSpeed)
	{
		m_currentMoveSpeed = Mathf.MoveTowards(m_currentMoveSpeed, 0f, Time.deltaTime * m_slowRate);
	}

	private void FixedUpdate()
	{
		if (Mathf.Abs(m_currentMoveSpeed) > Mathf.Epsilon)
		{
			Vector3 position = base.transform.position;
			position.x += m_currentMoveSpeed * Time.deltaTime;
			if (position.x < m_minPosition)
			{
				position.x = m_minPosition;
				m_onFullyExtendedLeft.Invoke();
			}
			else if (position.x > m_maxPosition)
			{
				position.x = m_maxPosition;
				m_onFullyExtendedRight.Invoke();
			}
			base.transform.position = position;
		}
	}

	private void OnDrawGizmos()
	{
		Vector3 position = base.transform.position;
		Vector3 position2 = base.transform.position;
		position.x -= m_maxMoveLeftDistance;
		position2.x += m_maxMoveRightDistance;
		Gizmos.color = Color.red;
		Gizmos.DrawLine(base.transform.position, position);
		Gizmos.color = Color.green;
		Gizmos.DrawLine(base.transform.position, position2);
	}
}
