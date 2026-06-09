using System;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Ladder : MonoBehaviour
{
	[Flags]
	public enum LadderUser
	{
		None = 0,
		Player = 1,
		Crawler = 2,
		Cat = 4
	}

	public struct LadderState
	{
		public bool m_isAtTop;

		public bool m_isAtBottom;

		public bool m_isNearBottom;
	}

	[Tooltip("Offset from the collider size which makes the player drop off earlier")]
	[SerializeField]
	private float m_bottomExitOffset;

	[Tooltip("Offset from the collider size which makes the player climb up earlier")]
	[SerializeField]
	private float m_topExitOffset;

	private BoxCollider2D m_collider;

	private Vector3 m_calculatedTopBound;

	private Vector3 m_calculatedBottomBound;

	[SerializeField]
	private LadderUser m_validUsers;

	public Bounds LadderBounds
	{
		get
		{
			Vector3 calculatedBottomBound = m_calculatedBottomBound;
			calculatedBottomBound.x = Mathf.Min(m_collider.bounds.min.x, calculatedBottomBound.x);
			Vector3 calculatedTopBound = m_calculatedTopBound;
			calculatedTopBound.x = Mathf.Max(m_collider.bounds.max.x, calculatedTopBound.x);
			Bounds result = default(Bounds);
			result.SetMinMax(calculatedBottomBound, calculatedTopBound);
			return result;
		}
	}

	public Vector3 TopBound
	{
		get
		{
			Vector3 calculatedTopBound = m_calculatedTopBound;
			calculatedTopBound.y += m_topExitOffset;
			return calculatedTopBound;
		}
	}

	public Vector3 BottomBound
	{
		get
		{
			Vector3 calculatedBottomBound = m_calculatedBottomBound;
			calculatedBottomBound.y += m_bottomExitOffset;
			return calculatedBottomBound;
		}
	}

	public bool CanUseLadder(LadderUser user)
	{
		return m_validUsers.HasFlag(user);
	}

	public LadderState GetLadderState(Vector3 position, float characterHeightForLadderBottom = 0f, float characterHeightForLadderTop = 1.5f)
	{
		LadderState result = default(LadderState);
		Vector3 bottomBound = BottomBound;
		Vector3 topBound = TopBound;
		result.m_isAtBottom = bottomBound.y >= position.y + characterHeightForLadderBottom;
		result.m_isNearBottom = bottomBound.y + 1f >= position.y + characterHeightForLadderBottom;
		result.m_isAtTop = topBound.y <= position.y + characterHeightForLadderTop;
		return result;
	}

	private void Awake()
	{
		m_collider = GetComponent<BoxCollider2D>();
	}

	private void Start()
	{
		CalculateBounds();
	}

	private void CalculateBounds()
	{
		if (m_collider == null)
		{
			m_collider = GetComponent<BoxCollider2D>();
		}
		if (m_collider == null)
		{
			Debug.LogError("Ladder " + base.gameObject.name + " is missing a BoxCollider2D component!");
			return;
		}
		Vector3 center = m_collider.bounds.center;
		center.y += m_collider.bounds.extents.y;
		RaycastHit2D raycastHit2D = Physics2D.Raycast(center, Vector2.down, 2f, GameLayers.EnvironmentMask);
		if (raycastHit2D.collider != null)
		{
			m_calculatedTopBound = raycastHit2D.point;
		}
		else
		{
			m_calculatedTopBound = center;
		}
		Vector3 center2 = m_collider.bounds.center;
		center2.y -= m_collider.bounds.extents.y;
		center2.y += 0.5f;
		RaycastHit2D raycastHit2D2 = Physics2D.Raycast(center2, Vector2.down, 1f, GameLayers.EnvironmentMask);
		if (raycastHit2D2.collider != null)
		{
			m_calculatedBottomBound = raycastHit2D2.point;
		}
		else
		{
			m_calculatedBottomBound = center2;
		}
		m_calculatedTopBound.z = (m_calculatedBottomBound.z = base.transform.position.z);
	}

	public void RefreshBounds()
	{
		CalculateBounds();
	}
}
