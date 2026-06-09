using UnityEngine;
using UnityEngine.Events;

public class CharacterTargetedLeap : MonoBehaviour
{
	public enum TargetedLeapType
	{
		None,
		VerticalUpwards,
		Horizontal,
		VerticalDown
	}

	[Header("Vertical Leap")]
	[SerializeField]
	private float m_verticalMaxHeight;

	[SerializeField]
	private float m_verticalMinHeight;

	[SerializeField]
	private float[] m_verticalCheckDistances;

	[Header("Horizontal Leap")]
	[SerializeField]
	private float m_horizontalMaxHeight;

	[SerializeField]
	private float m_horizontalMinHeight;

	[SerializeField]
	private float m_horizontalXDistance;

	[Header("Down Leap")]
	[SerializeField]
	private float m_downMaxHeight;

	[SerializeField]
	private float m_downMinHeight;

	[SerializeField]
	private float[] m_downCheckDistances;

	[Header("Near Edge Check")]
	[SerializeField]
	private float m_nearEdgeForwardDistance;

	[SerializeField]
	private float m_nearEdgeDropDistance;

	[Header("Validity Check")]
	[SerializeField]
	private Vector2 m_sizeBounds;

	private CharacterDirection m_characterDirection;

	private GameObject m_forceLeapTarget;

	private TargetedLeapType m_availableLeapType;

	private Vector2 m_detectedLeapPosition;

	private Vector2 m_storedLeapPosition;

	public UnityAction m_onLeapFinished;

	private bool m_isChecking;

	public TargetedLeapType AvailableLeapType => m_availableLeapType;

	public Vector2 DetectedLeapPosition => m_detectedLeapPosition;

	public Vector2 StoredLeapPosition => m_storedLeapPosition;

	public bool IsChecking
	{
		get
		{
			return m_isChecking;
		}
		set
		{
			m_isChecking = value;
		}
	}

	public void StoreCurrentLeapPosition()
	{
		m_storedLeapPosition = m_detectedLeapPosition;
	}

	private void Start()
	{
		m_characterDirection = GetComponent<CharacterDirection>();
	}

	private void Update()
	{
		IsNearEdge();
		m_availableLeapType = TargetedLeapType.None;
		if (!m_isChecking)
		{
			return;
		}
		Vector2 vector = base.transform.position;
		if (m_forceLeapTarget != null)
		{
			m_detectedLeapPosition = m_forceLeapTarget.transform.position;
			float num = Vector2.Dot(rhs: (m_detectedLeapPosition - vector).normalized, lhs: Vector2.up);
			if (num > 0.5f)
			{
				m_availableLeapType = TargetedLeapType.VerticalUpwards;
			}
			else if (num < -0.5f)
			{
				m_availableLeapType = TargetedLeapType.VerticalDown;
			}
			else
			{
				m_availableLeapType = TargetedLeapType.Horizontal;
			}
		}
		else if (!CheckForVerticalLeap())
		{
			CheckForHorizontalLeap();
		}
	}

	private bool CheckForLeap(float minHeight, float maxHeight, float[] checkDistances)
	{
		Vector2 vector = base.transform.position;
		vector.y += maxHeight;
		foreach (float num in checkDistances)
		{
			Vector2 vector2 = m_characterDirection.GetForwardVector();
			vector.x += vector2.x * num;
			RaycastHit2D raycastHit2D = Physics2D.Raycast(vector, Vector2.down, maxHeight - minHeight, GameLayers.CatEnvironmentMask);
			if (raycastHit2D.collider != null)
			{
				Vector2 point = raycastHit2D.point;
				if (CheckIfPositionIsValid(point))
				{
					m_detectedLeapPosition = point;
					Debug.DrawLine(vector, point, Color.green);
					return true;
				}
			}
			else
			{
				Vector2 vector3 = vector;
				vector3.y -= maxHeight - minHeight;
				Debug.DrawLine(vector, vector3, Color.red);
			}
		}
		return false;
	}

	private bool CheckIfPositionIsValid(Vector2 position)
	{
		position.y += 0.05f;
		position.y += m_sizeBounds.y * 0.5f;
		Collider2D collider2D = Physics2D.OverlapBox(position, m_sizeBounds, 0f, GameLayers.CatEnvironmentMask);
		DrawDebugBox(position, m_sizeBounds, Color.magenta);
		if (collider2D == null)
		{
			return true;
		}
		return false;
	}

	private void DrawDebugBox(Vector2 position, Vector2 size, Color color)
	{
		Vector2 vector = new Vector2(position.x - size.x * 0.5f, position.y + size.y * 0.5f);
		Vector2 vector2 = new Vector2(position.x + size.x * 0.5f, position.y + size.y * 0.5f);
		Vector2 vector3 = new Vector2(position.x - size.x * 0.5f, position.y - size.y * 0.5f);
		Vector2 vector4 = new Vector2(position.x + size.x * 0.5f, position.y - size.y * 0.5f);
		Debug.DrawLine(vector, vector2, color);
		Debug.DrawLine(vector2, vector4, color);
		Debug.DrawLine(vector4, vector3, color);
		Debug.DrawLine(vector3, vector, color);
	}

	private bool CheckForVerticalLeap()
	{
		if (CheckForLeap(m_verticalMinHeight, m_verticalMaxHeight, m_verticalCheckDistances))
		{
			m_availableLeapType = TargetedLeapType.VerticalUpwards;
			return true;
		}
		return false;
	}

	private bool CheckForHorizontalLeap()
	{
		Vector2 vector = base.transform.position;
		vector.y += m_horizontalMaxHeight;
		Vector2 vector2 = m_characterDirection.GetForwardVector();
		vector.x += vector2.x * m_horizontalXDistance;
		RaycastHit2D raycastHit2D = Physics2D.Raycast(vector, Vector2.down, m_horizontalMaxHeight - m_horizontalMinHeight, GameLayers.CatEnvironmentMask);
		if (raycastHit2D.collider != null)
		{
			Vector2 point = raycastHit2D.point;
			Vector2 point2 = raycastHit2D.point;
			point.x -= m_horizontalXDistance;
			point2.x += m_horizontalXDistance;
			Debug.DrawLine(raycastHit2D.point, point, Color.white);
			Debug.DrawLine(raycastHit2D.point, point2, Color.white);
			Vector2 point3 = raycastHit2D.point;
			Debug.DrawLine(vector, point3, Color.yellow);
			return false;
		}
		Vector2 vector3 = vector;
		vector3.y -= m_horizontalMaxHeight - m_horizontalMinHeight;
		Debug.DrawLine(vector, vector3, Color.red);
		return false;
	}

	private bool IsNearEdge()
	{
		Vector2 vector = base.transform.position;
		vector.y += 0.1f;
		Vector2 vector2 = m_characterDirection.GetForwardVector();
		vector.x += vector2.x * m_nearEdgeForwardDistance;
		RaycastHit2D raycastHit2D = Physics2D.BoxCast(vector, m_sizeBounds, 0f, Vector2.down, m_nearEdgeDropDistance, GameLayers.CatEnvironmentMask);
		_ = m_sizeBounds;
		DrawDebugBox(vector, m_sizeBounds, Color.yellow);
		vector.y -= m_nearEdgeDropDistance;
		DrawDebugBox(vector, m_sizeBounds, Color.yellow);
		if (raycastHit2D.collider == null)
		{
			return true;
		}
		return false;
	}

	public bool CheckForLeapDown()
	{
		if (IsNearEdge() && CheckForLeap(m_downMinHeight, m_downMaxHeight, m_downCheckDistances))
		{
			m_availableLeapType = TargetedLeapType.VerticalDown;
			m_isChecking = false;
			return true;
		}
		return false;
	}

	public void ForceLeapObject(GameObject leapObject)
	{
		m_forceLeapTarget = leapObject;
	}

	public void OnLeapDone()
	{
		m_onLeapFinished?.Invoke();
	}
}
