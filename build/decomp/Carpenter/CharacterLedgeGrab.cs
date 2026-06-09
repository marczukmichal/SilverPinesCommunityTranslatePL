using Shapes;
using UnityEngine;

public class CharacterLedgeGrab : MonoBehaviour
{
	public enum DetectedLedgeType
	{
		None,
		Climb,
		Vault,
		ClimbDown,
		ClimbDownPlatform
	}

	public struct DetectedLedge
	{
		public Vector3 m_initialHitPosition;

		public Vector3 m_detectedLedgePosition;

		public DetectedLedgeType m_ledgeType;

		public float m_ledgeHeightOffset;

		public float m_exitHeightOffsetUp;

		public float m_exitHeightOffsetDown;

		public Collider2D m_collidable;

		public bool m_canSwing;

		public bool CanSwingFromMovememnt(CharacterMovement movement)
		{
			float offGroundTimer = movement.OffGroundTimer;
			if (m_canSwing)
			{
				return offGroundTimer > 0.2f;
			}
			return false;
		}
	}

	[Header("Character Settings")]
	[SerializeField]
	private Vector2 m_bottomOffset;

	[SerializeField]
	private float m_height;

	[SerializeField]
	private Vector2 m_vaultBoxSize;

	[SerializeField]
	private Vector2 m_vaultOffset;

	[Header("Climb Up Settings")]
	[SerializeField]
	private float m_climbUpHeightMin;

	[SerializeField]
	private float m_climbUpHeightMax;

	[SerializeField]
	private float m_climbUpForwardAmount;

	[SerializeField]
	private int m_climbUpOffsetIterations = 5;

	[SerializeField]
	private float m_climbUpOffsetAmount = -0.15f;

	[Header("Climb Down Settings")]
	[SerializeField]
	private float m_climbDownCornerOffsetFar = 0.5f;

	[SerializeField]
	private float m_climbDownCornerOffsetClose;

	[SerializeField]
	private float m_climbDownHeightMin;

	[SerializeField]
	private float m_climbDownHeightMax;

	[SerializeField]
	private float m_climbDownForwardAmount;

	[SerializeField]
	private Vector2 m_climbDownValidateSize;

	[SerializeField]
	private Vector2 m_climbDownValidateOffsetUp;

	[SerializeField]
	private Vector2 m_climbDownValidateOffsetDown;

	[SerializeField]
	private Vector2 m_climbDownValidateOffsetForward;

	[Header("Swing Hang")]
	[SerializeField]
	private float m_swingHangYOffset = 0.25f;

	[SerializeField]
	private Vector2 m_swingHangCheckBoxSize;

	[Header("Raycast")]
	[SerializeField]
	private float m_raycastDistance = 1f;

	[SerializeField]
	private int m_wallEdgeDetectionIterations = 20;

	[SerializeField]
	private float m_validWallDotProduct;

	[Header("Exit Settings")]
	[SerializeField]
	private float m_exitCharacterHeight;

	[SerializeField]
	private float m_exitCharacterWidth;

	[Header("Exit Location Validation Size")]
	[SerializeField]
	private Vector2 m_exitLocationValidationSize;

	[Header("Flashlight")]
	[SerializeField]
	private ActiveItemListener m_flashlightItemListener;

	private Vector2 m_offsetOverride;

	private bool m_overrideEnabled;

	private DetectedLedge m_detectedForwardLedge;

	private DetectedLedge m_detectedClimbUpLedge;

	private DetectedLedge m_detectedClimbDownLedge;

	private CharacterDirection m_direction;

	private DetectedLedge m_activeLedge;

	private Transform m_activeLedgeTransform;

	private float m_lastLedgeGrabTime;

	private LedgeGrabLevelTransition m_savedLedgeGrabLevelTransition;

	[DebugCommand("ledge_debug", "Ledge debug gui", "ledge_debug <true/false>", typeof(bool), false)]
	public static bool LEDGE_DEBUG;

	private Vector2 BottomOffset
	{
		get
		{
			Vector2 result = ((!m_overrideEnabled) ? m_bottomOffset : m_offsetOverride);
			result.x *= m_direction.GetForwardVector().x;
			return result;
		}
	}

	public DetectedLedge DetectedForwardLedge => m_detectedForwardLedge;

	public DetectedLedge DetectedClimbUpLedge => m_detectedClimbUpLedge;

	public DetectedLedge DetectedClimbDownLedge => m_detectedClimbDownLedge;

	public DetectedLedge ActiveLedge => m_activeLedge;

	public Transform ActiveLedgeTransform => m_activeLedgeTransform;

	public LedgeGrabLevelTransition SavedLedgeGrabLevelTransition
	{
		get
		{
			return m_savedLedgeGrabLevelTransition;
		}
		set
		{
			m_savedLedgeGrabLevelTransition = value;
		}
	}

	public void SetOffsetOverride(Vector2 offsetOverride)
	{
		m_offsetOverride = offsetOverride;
		m_overrideEnabled = true;
	}

	public void ClearOffsetOverride()
	{
		m_overrideEnabled = false;
	}

	public void SetActiveLedge(DetectedLedge ledge)
	{
		m_activeLedge = ledge;
		m_activeLedgeTransform.SetParent(null);
		m_activeLedgeTransform.position = ledge.m_detectedLedgePosition;
		m_activeLedgeTransform.SetParent(ledge.m_collidable.transform);
	}

	private void Awake()
	{
		m_direction = GetComponent<CharacterDirection>();
		GameObject gameObject = new GameObject("CharacterLedgeGrab Active Ledge Transform Helper");
		m_activeLedgeTransform = gameObject.transform;
	}

	private void OnDestroy()
	{
		if (m_activeLedgeTransform != null)
		{
			Object.Destroy(m_activeLedgeTransform.gameObject);
		}
	}

	private void Update()
	{
		if (Time.time - m_lastLedgeGrabTime < 0.5f)
		{
			ClearLedgeGrabs();
		}
		else
		{
			CheckForLedges();
		}
	}

	public bool CheckLedgeValidity(DetectedLedge ledge)
	{
		if (GameUtils.IsCurrentLevelDark())
		{
			bool flag = false;
			if (m_flashlightItemListener != null)
			{
				ItemInstance activeItemInstance = m_flashlightItemListener.ActiveItemInstance;
				if (activeItemInstance != null && activeItemInstance.Activated)
				{
					flag = true;
				}
			}
			if (!flag && !FieldOfViewArea.IsBoundsVisible(new Bounds(ledge.m_detectedLedgePosition, Vector3.one * 0.1f)))
			{
				return false;
			}
		}
		if (Stairs.DoesPositionOverlapWithStairs(ledge.m_detectedLedgePosition))
		{
			return false;
		}
		return true;
	}

	private bool IsValidWall(RaycastHit2D hit, Vector3 forwardDirection)
	{
		if (hit.collider == null)
		{
			return false;
		}
		if (hit.collider.GetComponent<Climbable>() == null)
		{
			return false;
		}
		return Vector2.Dot(hit.normal, forwardDirection) < 0f - m_validWallDotProduct;
	}

	private void ClearLedgeGrabs()
	{
		m_detectedForwardLedge.m_ledgeType = DetectedLedgeType.None;
		m_detectedClimbUpLedge.m_ledgeType = DetectedLedgeType.None;
		m_detectedClimbDownLedge.m_ledgeType = DetectedLedgeType.None;
	}

	private void CheckForLedges()
	{
		CheckForForwardLedge();
		CheckForClimbUpLedge();
		CheckForClimbDownLedge();
	}

	private void CheckIfCanSwing(ref DetectedLedge detectedForwardLedge)
	{
		Vector2 vector = detectedForwardLedge.m_detectedLedgePosition;
		vector.y -= m_swingHangYOffset;
		vector.y -= m_swingHangCheckBoxSize.y * 0.5f;
		Collider2D collider2D = Physics2D.OverlapBox(vector, m_swingHangCheckBoxSize, 0f, GameLayers.ClimbableMask);
		if (LEDGE_DEBUG)
		{
			GameDebugDrawer.TempDebugDrawCube(vector, m_swingHangCheckBoxSize, 1f, (collider2D == null) ? Color.green : Color.red);
		}
		detectedForwardLedge.m_canSwing = collider2D == null;
	}

	private Vector2 GetForwardLedgeRaycastOrigin(Vector2 forwardDirection)
	{
		Vector2 result = base.transform.position;
		result += BottomOffset;
		result.y += m_height * 0.5f;
		return result;
	}

	private bool ValidateIfLedgeIsClearOfObstacles(Vector2 ledgePosition, Collider2D newFloorCollider)
	{
		ledgePosition.y += 0.05f;
		ledgePosition.y += m_exitLocationValidationSize.y * 0.5f;
		Collider2D[] array = Physics2D.OverlapBoxAll(ledgePosition, m_exitLocationValidationSize, 0f, GameLayers.ClimbableMask);
		for (int i = 0; i < array.Length; i++)
		{
			if (!(array[i] == newFloorCollider))
			{
				return false;
			}
		}
		return true;
	}

	private void CheckForForwardLedge()
	{
		Vector2 vector = m_direction.GetForwardVector();
		Vector2 vector2 = base.transform.position;
		vector2 += BottomOffset;
		RaycastHit2D hit = Physics2D.BoxCast(GetForwardLedgeRaycastOrigin(vector), new Vector2(m_raycastDistance, m_height), 0f, vector, m_raycastDistance, GameLayers.ClimbableMask);
		m_detectedForwardLedge.m_ledgeType = DetectedLedgeType.None;
		if (!IsValidWall(hit, vector))
		{
			return;
		}
		m_detectedForwardLedge.m_initialHitPosition = hit.point;
		Vector2 vector3 = vector2;
		vector3.y += m_height;
		RaycastHit2D hit2 = Physics2D.Raycast(vector3, vector, m_raycastDistance, GameLayers.ClimbableMask);
		vector2.y = hit.point.y;
		if (IsValidWall(hit2, vector))
		{
			return;
		}
		if (!SearchForLedgeYPosition(vector3, vector2, out var ledgeYPos, out var _))
		{
			ledgeYPos = hit.point.y;
		}
		Vector2 vector4 = default(Vector2);
		vector4.x = vector3.x;
		vector4.y = ledgeYPos;
		Vector2 far = vector4;
		far += vector * 0.5f;
		far.y += m_raycastDistance * 0.5f;
		Vector2 close = vector4;
		close -= vector * 0.5f;
		close.y += m_raycastDistance * 0.5f;
		if (!SearchForLedgeXPosition(far, close, out var ledgeXPos, out var bestCollider2))
		{
			return;
		}
		vector4.x = ledgeXPos;
		if (ValidateIfLedgeIsClearOfObstacles(vector4, bestCollider2))
		{
			m_detectedForwardLedge.m_detectedLedgePosition = vector4;
			Vector2 vector5 = vector4;
			Vector2 vaultOffset = m_vaultOffset;
			vaultOffset.x *= vector.x;
			if (Physics2D.OverlapBox(vector5 + vaultOffset, m_vaultBoxSize, 0f, GameLayers.ClimbableMask) == null)
			{
				m_detectedForwardLedge.m_ledgeType = DetectedLedgeType.Vault;
				CalculateExitHeightOffsets(offset: new Vector2(m_exitCharacterWidth * 3f * vector.x, 0f), ledge: ref m_detectedForwardLedge);
				m_detectedForwardLedge.m_canSwing = false;
			}
			else
			{
				m_detectedForwardLedge.m_ledgeType = DetectedLedgeType.Climb;
				CalculateExitHeightOffsets(ref m_detectedForwardLedge, Vector2.zero);
				CheckIfCanSwing(ref m_detectedForwardLedge);
			}
			m_detectedForwardLedge.m_collidable = bestCollider2;
			m_detectedForwardLedge.m_ledgeHeightOffset = vector4.y - (base.transform.position.y + BottomOffset.y);
		}
	}

	private void CalculateExitHeightOffsets(ref DetectedLedge ledge, Vector2 offset)
	{
		float num = 0.1f;
		Vector2 vector = (Vector2)ledge.m_detectedLedgePosition + offset;
		Vector2 size = new Vector2(m_exitCharacterWidth, num * 0.01f);
		Vector2 origin = vector;
		origin.y += num;
		RaycastHit2D raycastHit2D = Physics2D.BoxCast(origin, size, 0f, Vector2.up, m_exitCharacterHeight - num, GameLayers.ClimbableMask);
		if (raycastHit2D.collider != null)
		{
			ledge.m_exitHeightOffsetUp = raycastHit2D.distance + num;
		}
		else
		{
			ledge.m_exitHeightOffsetUp = m_exitCharacterHeight;
		}
		Vector2 origin2 = vector;
		origin2.y -= num;
		RaycastHit2D raycastHit2D2 = Physics2D.BoxCast(origin2, size, 0f, Vector2.down, m_exitCharacterHeight - num, GameLayers.ClimbableMask);
		if (raycastHit2D2.collider != null)
		{
			ledge.m_exitHeightOffsetDown = raycastHit2D2.distance + num;
		}
		else
		{
			ledge.m_exitHeightOffsetDown = m_exitCharacterHeight;
		}
	}

	private bool IsValidPlatform(RaycastHit2D hit)
	{
		if (hit.collider == null)
		{
			return false;
		}
		Climbable component = hit.collider.GetComponent<Climbable>();
		if (component == null)
		{
			return false;
		}
		if (component.Mode != Climbable.ClimbableMode.ClimbThrough)
		{
			return false;
		}
		return Vector2.Dot(hit.normal, Vector2.down) < -0.9f;
	}

	private void CheckForClimbUpLedge()
	{
		Vector2 vector = m_direction.GetForwardVector();
		Vector2 vector2 = base.transform.position;
		vector2 += vector * m_climbUpForwardAmount;
		Vector2 vector3 = vector2;
		vector2.y += m_climbUpHeightMax;
		vector3.y += m_climbUpHeightMin;
		for (int i = 0; i < m_climbUpOffsetIterations; i++)
		{
			Vector2 start = vector2;
			start.x += (float)i * m_climbUpOffsetAmount * vector.x;
			Vector2 end = vector3;
			end.x += (float)i * m_climbUpOffsetAmount * vector.x;
			RaycastHit2D hit = Physics2D.Linecast(start, end, GameLayers.ClimbableMask);
			m_detectedClimbUpLedge.m_ledgeType = DetectedLedgeType.None;
			if (IsValidPlatform(hit))
			{
				start.x += 0.5f * vector.x;
				end.x += 0.5f * vector.x;
				RaycastHit2D hit2 = Physics2D.Linecast(start, end, GameLayers.ClimbableMask);
				if (IsValidPlatform(hit2) && ValidateIfLedgeIsClearOfObstacles(hit.point, hit2.collider))
				{
					m_detectedClimbUpLedge.m_ledgeType = DetectedLedgeType.Climb;
					m_detectedClimbUpLedge.m_detectedLedgePosition = (m_detectedClimbUpLedge.m_initialHitPosition = hit.point);
					m_detectedClimbUpLedge.m_ledgeHeightOffset = hit.point.y - (base.transform.position.y + BottomOffset.y);
					m_detectedClimbUpLedge.m_collidable = hit.collider;
					CalculateExitHeightOffsets(ref m_detectedClimbUpLedge, Vector2.zero);
					break;
				}
			}
		}
	}

	private bool ValidateClimbDown(Vector2 climbPosition, Vector2 forwardDirection, bool isPlatform)
	{
		Vector2 climbDownValidateOffsetUp = m_climbDownValidateOffsetUp;
		climbDownValidateOffsetUp.x *= forwardDirection.x;
		Vector2 point = climbPosition + climbDownValidateOffsetUp;
		Vector2 point2 = climbPosition;
		Vector2 climbDownValidateOffsetDown = m_climbDownValidateOffsetDown;
		climbDownValidateOffsetDown.x *= forwardDirection.x;
		point2 += climbDownValidateOffsetDown;
		if (Physics2D.OverlapBox(point, m_climbDownValidateSize, 0f, GameLayers.ClimbableMask) == null && Physics2D.OverlapBox(point2, m_climbDownValidateSize, 0f, GameLayers.ClimbableMask) == null)
		{
			if (!isPlatform)
			{
				Vector2 climbDownValidateOffsetForward = m_climbDownValidateOffsetForward;
				climbDownValidateOffsetForward.x *= forwardDirection.x;
				return Physics2D.OverlapBox(climbPosition + climbDownValidateOffsetForward, m_climbDownValidateSize, 0f, GameLayers.ClimbableMask) == null;
			}
			return true;
		}
		return false;
	}

	private void CheckForClimbDownLedge()
	{
		Vector2 vector = m_direction.GetForwardVector();
		Vector2 vector2 = base.transform.position;
		vector2 += vector * m_climbDownForwardAmount;
		Vector2 vector3 = vector2;
		vector2.y += m_climbDownHeightMax;
		vector3.y += m_climbDownHeightMin;
		RaycastHit2D hit = Physics2D.Linecast(vector2, vector3, GameLayers.ClimbableMask);
		m_detectedClimbDownLedge.m_ledgeType = DetectedLedgeType.None;
		if (IsValidPlatform(hit))
		{
			if (ValidateClimbDown(hit.point, vector, isPlatform: true))
			{
				m_detectedClimbDownLedge.m_ledgeType = DetectedLedgeType.ClimbDownPlatform;
				m_detectedClimbDownLedge.m_detectedLedgePosition = (m_detectedClimbDownLedge.m_initialHitPosition = hit.point);
				m_detectedClimbDownLedge.m_ledgeHeightOffset = 0f;
				m_detectedClimbDownLedge.m_collidable = hit.collider;
				CalculateExitHeightOffsets(ref m_detectedClimbDownLedge, Vector2.zero);
			}
			return;
		}
		vector2.x += vector.x * m_climbDownCornerOffsetClose;
		vector3.x += vector.x * m_climbDownCornerOffsetClose;
		hit = Physics2D.Linecast(vector2, vector3, GameLayers.ClimbableMask);
		if (!IsValidWall(hit, Vector2.down))
		{
			return;
		}
		m_detectedClimbDownLedge.m_initialHitPosition = hit.point;
		Vector2 vector4 = vector2;
		vector4 += vector * m_climbDownCornerOffsetFar;
		Vector2 end = vector3;
		end += vector * m_climbDownCornerOffsetFar;
		RaycastHit2D hit2 = Physics2D.Linecast(vector4, end, GameLayers.ClimbableMask);
		if (!IsValidWall(hit2, Vector2.down) && SearchForLedgeXPosition(vector2, vector4, out var ledgeXPos, out var bestCollider))
		{
			Vector2 point = hit.point;
			point.x = ledgeXPos;
			if (ValidateClimbDown(point, vector, isPlatform: false))
			{
				m_detectedClimbDownLedge.m_ledgeType = DetectedLedgeType.ClimbDown;
				m_detectedClimbDownLedge.m_detectedLedgePosition = point;
				m_detectedClimbDownLedge.m_ledgeHeightOffset = 0f;
				m_detectedClimbDownLedge.m_collidable = bestCollider;
				CalculateExitHeightOffsets(offset: new Vector2(m_exitCharacterWidth * vector.x, 0f), ledge: ref m_detectedClimbDownLedge);
			}
		}
	}

	private bool SearchForLedgeYPosition(Vector2 top, Vector2 bottom, out float ledgeYPos, out Collider2D bestCollider)
	{
		Vector3 forwardVector = m_direction.GetForwardVector();
		ledgeYPos = bottom.y;
		bestCollider = null;
		bool flag = false;
		bool flag2 = false;
		for (int i = 0; i < m_wallEdgeDetectionIterations; i++)
		{
			Vector2 vector = (bottom + top) / 2f;
			RaycastHit2D hit = Physics2D.Raycast(vector, forwardVector, m_raycastDistance, GameLayers.ClimbableMask);
			if (IsValidWall(hit, forwardVector))
			{
				bestCollider = hit.collider;
				bottom = vector;
				ledgeYPos = hit.point.y;
				flag2 = true;
			}
			else
			{
				top = vector;
				flag = true;
			}
			if (Vector2.Distance(bottom, top) < 0.01f)
			{
				break;
			}
		}
		return flag && flag2;
	}

	private bool SearchForLedgeXPosition(Vector2 far, Vector2 close, out float ledgeXPos, out Collider2D bestCollider)
	{
		ledgeXPos = close.x;
		bool flag = false;
		bool flag2 = false;
		bestCollider = null;
		for (int i = 0; i < m_wallEdgeDetectionIterations; i++)
		{
			Vector2 vector = (close + far) / 2f;
			RaycastHit2D hit = Physics2D.Raycast(vector, Vector2.down, m_raycastDistance, GameLayers.ClimbableMask);
			if (IsValidWall(hit, Vector2.down))
			{
				far = vector;
				ledgeXPos = hit.point.x;
				flag2 = true;
				bestCollider = hit.collider;
			}
			else
			{
				close = vector;
				flag = true;
			}
			if (Vector2.Distance(close, far) < 0.01f)
			{
				break;
			}
		}
		return flag && flag2;
	}

	private void OnGUI()
	{
		if (LEDGE_DEBUG)
		{
			DrawLedgeGUI(ref m_detectedForwardLedge);
			DrawLedgeGUI(ref m_detectedClimbUpLedge);
			DrawLedgeGUI(ref m_detectedClimbDownLedge);
		}
	}

	public void DrawDebugShapes()
	{
		Vector2 vector = m_direction.GetForwardVector();
		Vector2 vector2 = GetForwardLedgeRaycastOrigin(vector) + vector * (m_raycastDistance * 0.5f);
		Color magenta = Color.magenta;
		magenta.a = 0.2f;
		Draw.Cuboid(vector2, new Vector3(m_raycastDistance, m_height, 1f), magenta);
	}

	private void DrawLedgeGUI(ref DetectedLedge ledge)
	{
		if (ledge.m_ledgeType != 0)
		{
			Vector2 vector = Camera.main.WorldToScreenPoint(ledge.m_detectedLedgePosition);
			vector.y = (float)Screen.height - vector.y;
			Rect position = default(Rect);
			position.center = vector;
			position.size = Vector2.one * 0.1f;
			GUI.color = Color.green;
			GUI.Box(position, ".");
			GUI.color = Color.white;
			GUILayout.BeginArea(new Rect(vector, new Vector2(96f, 256f)));
			GUILayout.Label(ledge.m_ledgeType.ToString());
			GUILayout.Label(ledge.m_ledgeHeightOffset.ToString());
			GUILayout.Label("ExitUpSpace: " + ledge.m_exitHeightOffsetUp);
			GUILayout.Label("ExitDownSpace: " + ledge.m_exitHeightOffsetDown);
			GUILayout.EndArea();
		}
	}

	public void ExitLedgeGrab()
	{
		m_lastLedgeGrabTime = Time.time;
		ClearLedgeGrabs();
	}
}
