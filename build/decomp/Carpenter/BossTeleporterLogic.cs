using System.Collections;
using UnityEngine;

public class BossTeleporterLogic : MonoBehaviour
{
	[Header("Target")]
	[SerializeField]
	private Transform m_target;

	[Header("X Limits")]
	[SerializeField]
	private float m_minX = -10f;

	[SerializeField]
	private float m_maxX = 10f;

	[Header("Floor Boundary")]
	[SerializeField]
	private float m_floorBoundary;

	[Header("Teleport Points")]
	[SerializeField]
	private Transform m_rightSideTopPosition;

	[SerializeField]
	private Transform m_rightSideBottomPosition;

	[SerializeField]
	private Transform m_leftSideTopPosition;

	[SerializeField]
	private Transform m_leftSideBottomPosition;

	[Header("Cooldown")]
	[SerializeField]
	private float m_cooldown = 1.5f;

	private bool m_teleportEnabled = true;

	private void Update()
	{
		if (m_teleportEnabled && !(m_target == null))
		{
			float x = m_target.position.x;
			if (x < m_minX)
			{
				TeleportLeftSide();
			}
			else if (x > m_maxX)
			{
				TeleportRightSide();
			}
		}
	}

	private void TeleportLeftSide()
	{
		Teleport(m_leftSideTopPosition, m_leftSideBottomPosition, CharacterDirection.Facing.Right);
	}

	private void TeleportRightSide()
	{
		Teleport(m_rightSideTopPosition, m_rightSideBottomPosition, CharacterDirection.Facing.Left);
	}

	private void Teleport(Transform topTeleportPosition, Transform bottomTeleportPosition, CharacterDirection.Facing newFacing)
	{
		GameObject item = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item;
		Transform transform = topTeleportPosition;
		if (item != null && item.transform.position.y < m_floorBoundary)
		{
			transform = bottomTeleportPosition;
		}
		m_target.position = transform.position;
		CharacterDirection component = m_target.GetComponent<CharacterDirection>();
		if (component != null)
		{
			CharacterDirection.Facing facing3 = (component.CurrentDirection = (component.DesiredDirection = newFacing));
		}
		CharacterMovement component2 = m_target.GetComponent<CharacterMovement>();
		if (component2 != null)
		{
			component2.SetPreviousVelocity(Vector2.zero);
			component2.SnapToGround();
		}
		StartCoroutine(TeleportCooldown());
	}

	private IEnumerator TeleportCooldown()
	{
		m_teleportEnabled = false;
		yield return new WaitForSeconds(m_cooldown);
		m_teleportEnabled = true;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawLine(new Vector3(m_minX, 100f, 0f), new Vector3(m_minX, -100f, 0f));
		Gizmos.color = Color.red;
		Gizmos.DrawLine(new Vector3(m_maxX, 100f, 0f), new Vector3(m_maxX, -100f, 0f));
		Gizmos.color = Color.magenta;
		Gizmos.DrawLine(new Vector3(m_minX, m_floorBoundary, 0f), new Vector3(m_maxX, m_floorBoundary, 0f));
		Gizmos.color = Color.white;
	}
}
