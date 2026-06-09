using UnityEngine;

public class CharacterRecoveryPosition : MonoBehaviour
{
	private CharacterMovement m_movement;

	private Vector3 m_recoveryPosition;

	private void Awake()
	{
		m_movement = GetComponent<CharacterMovement>();
	}

	public Vector3 GetRecoveryPosition()
	{
		RecoveryPositionMarker[] array = Object.FindObjectsByType<RecoveryPositionMarker>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
		if (array.Length != 0)
		{
			RecoveryPositionMarker recoveryPositionMarker = null;
			float num = 1000f;
			RecoveryPositionMarker[] array2 = array;
			foreach (RecoveryPositionMarker recoveryPositionMarker2 in array2)
			{
				float num2 = Vector2.Distance(m_recoveryPosition, recoveryPositionMarker2.transform.position);
				if (num2 < num)
				{
					num = num2;
					recoveryPositionMarker = recoveryPositionMarker2;
				}
			}
			return recoveryPositionMarker.transform.position;
		}
		return m_recoveryPosition;
	}

	private void Start()
	{
		SetRecoveryPositionToCurrentPosition();
	}

	private void SetRecoveryPositionToCurrentPosition()
	{
		m_recoveryPosition = base.transform.position;
	}

	private bool IsGroundedOnBothSides()
	{
		Vector2 position = base.transform.position;
		Vector2 position2 = base.transform.position;
		position.x -= 0.5f;
		position2.x += 0.5f;
		if (FloorCheck(position))
		{
			return FloorCheck(position2);
		}
		return false;
	}

	private bool FloorCheck(Vector2 position)
	{
		position.y += 0.25f;
		return Physics2D.Raycast(position, Vector2.down, 0.5f, GameLayers.CharacterNavigationMask);
	}

	public void TryToUpdateRecoveryPosition()
	{
		CharacterMovement.CollisionInfo collisions = m_movement.m_collisions;
		if (collisions.m_below && !collisions.m_overlappingHorizontal && !collisions.m_overlappingVertical && !collisions.m_slidingDownMaxSlope && collisions.m_slopeAngle < 10f && !GameplayWaterBounds.IsNearWater(base.transform.position, 1f) && IsGroundedOnBothSides())
		{
			SetRecoveryPositionToCurrentPosition();
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawSphere(m_recoveryPosition, 0.1f);
		Gizmos.color = Color.white;
	}
}
