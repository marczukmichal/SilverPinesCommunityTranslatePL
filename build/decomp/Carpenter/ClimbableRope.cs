using UnityEngine;
using UnityEngine.Events;

public class ClimbableRope : MonoBehaviour
{
	public struct RopeState
	{
		public bool m_isAtTop;

		public bool m_isAtBottom;

		public bool m_isNearBottom;
	}

	[Tooltip("Offset from the collider size which makes the player drop off earlier")]
	[SerializeField]
	private float m_bottomExitOffset = 1f;

	[SerializeField]
	private float m_grabbedForce = 250f;

	[SerializeField]
	private LineRope m_lineRope;

	[SerializeField]
	private UnityEvent m_onExitRope;

	private BoxCollider2D m_collider;

	private Rigidbody2D[] m_ropeRigidbodies;

	public Bounds RopeBounds => m_collider.bounds;

	public Vector3 TopBound
	{
		get
		{
			Vector3 center = m_collider.bounds.center;
			center.y += m_collider.bounds.extents.y;
			return center;
		}
	}

	public Vector3 BottomBound
	{
		get
		{
			Vector3 center = m_collider.bounds.center;
			center.y -= m_collider.bounds.extents.y;
			center.y += m_bottomExitOffset;
			return center;
		}
	}

	private void Awake()
	{
		m_collider = GetComponent<BoxCollider2D>();
		m_ropeRigidbodies = GetComponentsInChildren<Rigidbody2D>();
	}

	public RopeState GetRopeState(CharacterStance characterStance)
	{
		RopeState result = default(RopeState);
		Vector3 bottomBound = BottomBound;
		Vector3 topBound = TopBound;
		Bounds standingBounds = characterStance.GetStandingBounds();
		result.m_isAtBottom = bottomBound.y >= standingBounds.min.y;
		result.m_isAtTop = topBound.y <= standingBounds.max.y;
		return result;
	}

	public void ApplyRopeForce(Vector3 position)
	{
		if (m_lineRope != null)
		{
			Vector2 vector = Vector2.right * (m_grabbedForce * Random.Range(-1f, 1f));
			m_lineRope.AddCollisionVelocityFromPosition(vector, position);
		}
	}

	public float GetXForPosition(Vector3 position)
	{
		if (m_lineRope != null)
		{
			return m_lineRope.GetClosestWorldPositionOnRope(position).x;
		}
		return position.x;
	}

	public void ApplyRopeSnapToLegsPosition(Vector3 legPosition)
	{
		if (m_lineRope != null)
		{
			m_lineRope.ApplyRopeSnapToLegsPosition(legPosition);
		}
	}

	public void ExitRope()
	{
		if (m_lineRope != null)
		{
			m_lineRope.ClearRopeSnapToLegsPosition();
		}
		m_onExitRope.Invoke();
	}
}
