using UnityEngine;

public class CharacterPrecariousDropDetection : MonoBehaviour
{
	[SerializeField]
	private float m_dropHorizontalOffset = 1f;

	[SerializeField]
	private float m_requiredDropDistance = 3f;

	[SerializeField]
	private float m_positionSnapOffset = 1f;

	private CharacterMovement m_movement;

	private CharacterDirection m_direction;

	private bool m_dropDetected;

	private float m_tempDisableCheckTime;

	public bool DropDetected => m_dropDetected;

	public void TempDisable(float time)
	{
		m_tempDisableCheckTime = Time.time + time;
	}

	private void Awake()
	{
		m_movement = GetComponent<CharacterMovement>();
		m_direction = GetComponent<CharacterDirection>();
	}

	private void Update()
	{
		m_dropDetected = false;
		if (!(Time.time <= m_tempDisableCheckTime) && m_movement.IsGrounded && !m_movement.AttachedStairs)
		{
			_ = (Vector2)m_direction.GetForwardVector();
			GetDropWorldPosition();
			if (m_movement.m_collisions.m_below && DoRaycastTest(GetDropWorldPosition(), m_requiredDropDistance) && FindLedgeDropPosition().HasValue)
			{
				m_dropDetected = true;
			}
		}
	}

	private Vector2 GetDropWorldPosition()
	{
		Vector2 vector = m_direction.GetForwardVector();
		Vector2 result = base.transform.position;
		result.x += vector.x * m_dropHorizontalOffset;
		result.y += 0.25f;
		return result;
	}

	private bool DoRaycastTest(Vector2 position, float distance)
	{
		return !Physics2D.Raycast(position, Vector2.down, distance, GameLayers.CharacterNavigationMask);
	}

	private void OnDrawGizmos()
	{
		if (Application.isPlaying)
		{
			Gizmos.color = (m_dropDetected ? Color.red : Color.green);
			Vector2 dropWorldPosition = GetDropWorldPosition();
			Gizmos.DrawLine(dropWorldPosition, dropWorldPosition + Vector2.down * m_requiredDropDistance);
			Gizmos.color = Color.white;
		}
	}

	private Vector2? FindLedgeDropPosition(Vector2 startPoint, Vector2 direction, float stepSize = 0.01f, float maxDistance = 1f)
	{
		Vector2 value = startPoint;
		for (float num = 0f; num < maxDistance; num += stepSize)
		{
			Vector2 vector = startPoint + direction * num;
			RaycastHit2D raycastHit2D = Physics2D.Raycast(vector, Vector2.down, m_requiredDropDistance, GameLayers.CharacterNavigationMask);
			Debug.DrawRay(vector, Vector2.down * m_requiredDropDistance, raycastHit2D.collider ? Color.green : Color.red);
			if (!raycastHit2D)
			{
				return value;
			}
			value = raycastHit2D.point;
		}
		return null;
	}

	private Vector2? FindLedgeDropPosition()
	{
		Vector2 direction = m_direction.GetForwardVector();
		Vector2 startPoint = base.transform.position;
		startPoint.y += 0.05f;
		direction.x -= m_dropHorizontalOffset * direction.x * -1f;
		return FindLedgeDropPosition(startPoint, direction, 0.05f, m_dropHorizontalOffset * 2f);
	}

	public void SnapToDetectedEdgePosition()
	{
		Vector2 vector = m_direction.GetForwardVector();
		GetDropWorldPosition();
		Vector2 vector2 = vector;
		vector2.x *= -1f;
		Vector2? vector3 = FindLedgeDropPosition();
		if (vector3.HasValue)
		{
			Vector3 position = base.transform.position;
			position.x = vector3.Value.x;
			position.x += vector.x * m_positionSnapOffset;
			base.transform.position = position;
		}
	}
}
