using System;
using Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

public class CameraBounds : MonoBehaviour
{
	[Serializable]
	public class CameraEdgeSettings
	{
		[FormerlySerializedAs("m_edgeTiltStartDistance")]
		public float m_edgeYawStartDistance;

		[Range(0f, 2f)]
		public float m_edgeYawAngleScalar = 1f;

		[Range(0f, 2f)]
		public float m_edgePositionOffsetScalar = 1f;

		public bool CanUseEdgeCamera()
		{
			return m_edgeYawStartDistance > 0f;
		}
	}

	public enum CameraHeightPositioning
	{
		ClampToBounds,
		IgnoreBounds,
		FixedHeight
	}

	public enum ActiveCameraMode
	{
		Normal,
		LeftEdge,
		RightEdge
	}

	[SerializeField]
	private bool m_boundsEnabled = true;

	[Tooltip("Priority for which camera bound should be active if multiple are overlapping. High values means higher priority.")]
	[SerializeField]
	private int m_priority;

	[Tooltip("Enable to seperate out the bounds that the player needs to be in to activate the bound and the view area.")]
	[SerializeField]
	private bool m_usePlayerBounds;

	[FormerlySerializedAs("m_bounds")]
	[SerializeField]
	private Bounds m_viewBounds;

	[SerializeField]
	private Bounds m_playerBounds;

	[SerializeField]
	private FollowPlayerVirtualCamera.CameraSettings m_cameraSettings;

	[SerializeField]
	private float m_depthOfFieldOffset;

	[SerializeField]
	private CameraEdgeSettings m_leftEdgeSettings;

	private CameraBoundsExtender m_leftEdgeDetectedExtender;

	[SerializeField]
	private CameraEdgeSettings m_rightEdgeSettings;

	private CameraBoundsExtender m_rightEdgeDetectedExtender;

	[SerializeField]
	private CinemachineVirtualCamera m_regularVirtualCamera;

	[SerializeField]
	private CameraHeightPositioning m_cameraHeightPositioning;

	[SerializeField]
	private float m_fixedHeightOffset;

	private ActiveCameraMode m_activeCameraMode;

	private CinemachineConfiner2D m_confiner;

	private bool m_boundsCameraActive;

	private Transform m_transform;

	public int Priority => m_priority;

	public bool UsePlayerBounds => m_usePlayerBounds;

	public Bounds ViewBounds
	{
		get
		{
			return m_viewBounds;
		}
		set
		{
			m_viewBounds = value;
		}
	}

	public Bounds PlayerBounds
	{
		get
		{
			return m_playerBounds;
		}
		set
		{
			m_playerBounds = value;
		}
	}

	public FollowPlayerVirtualCamera.CameraSettings CameraSettings => m_cameraSettings;

	public float DepthOfFieldOffset => m_depthOfFieldOffset;

	public CameraEdgeSettings LeftEdgeSettings => m_leftEdgeSettings;

	public bool LeftEdgeExtended
	{
		get
		{
			if (m_leftEdgeDetectedExtender != null)
			{
				return m_leftEdgeDetectedExtender.isActiveAndEnabled;
			}
			return false;
		}
	}

	public CameraEdgeSettings RightEdgeSettings => m_rightEdgeSettings;

	public bool RightEdgeExtended
	{
		get
		{
			if (m_rightEdgeDetectedExtender != null)
			{
				return m_rightEdgeDetectedExtender.isActiveAndEnabled;
			}
			return false;
		}
	}

	public CameraHeightPositioning CameraHeightPositioningMode => m_cameraHeightPositioning;

	public float FixedHeightOffset => m_fixedHeightOffset;

	public ActiveCameraMode ActiveVirtualCamera
	{
		get
		{
			return m_activeCameraMode;
		}
		set
		{
			if (m_activeCameraMode != value)
			{
				m_activeCameraMode = value;
				if (m_boundsCameraActive)
				{
					ActivateCamera();
				}
			}
		}
	}

	public bool BoundsEnabled
	{
		get
		{
			return m_boundsEnabled;
		}
		set
		{
			if (m_boundsEnabled != value)
			{
				m_boundsEnabled = value;
				if (m_boundsEnabled)
				{
					GlobalReferences.Instance.Sets.Camera.ActiveCameraBoundsSet.Add(this);
				}
				else
				{
					GlobalReferences.Instance.Sets.Camera.ActiveCameraBoundsSet.Remove(this);
				}
			}
		}
	}

	public Bounds WorldBounds
	{
		get
		{
			Bounds result = (m_usePlayerBounds ? m_playerBounds : m_viewBounds);
			result.center += base.transform.position;
			return result;
		}
	}

	public Bounds WorldViewBounds
	{
		get
		{
			Bounds viewBounds = m_viewBounds;
			viewBounds.center += base.transform.position;
			return viewBounds;
		}
	}

	public float GetFixedHeightY()
	{
		return WorldViewBounds.min.y + m_fixedHeightOffset;
	}

	private void ActivateCamera()
	{
		if ((bool)m_regularVirtualCamera)
		{
			FollowPlayerVirtualCamera component = m_regularVirtualCamera.gameObject.GetComponent<FollowPlayerVirtualCamera>();
			if (component != null)
			{
				component.ResetOffset();
			}
		}
		if (m_regularVirtualCamera != null)
		{
			m_regularVirtualCamera.enabled = m_activeCameraMode == ActiveCameraMode.Normal;
		}
	}

	private void DisableCamera()
	{
		if (m_regularVirtualCamera != null)
		{
			m_regularVirtualCamera.enabled = false;
		}
	}

	public float GetLeftTiltEdgeLocalPosition()
	{
		return (m_usePlayerBounds ? PlayerBounds : ViewBounds).min.x + m_leftEdgeSettings.m_edgeYawStartDistance;
	}

	public float GetRightTiltEdgeLocalPosition()
	{
		return (m_usePlayerBounds ? PlayerBounds : ViewBounds).max.x - m_rightEdgeSettings.m_edgeYawStartDistance;
	}

	private void Awake()
	{
		m_transform = GetComponent<Transform>();
		if (m_regularVirtualCamera != null)
		{
			m_confiner = m_regularVirtualCamera.GetComponent<CinemachineConfiner2D>();
		}
	}

	private void Start()
	{
		Vector3 extents = m_viewBounds.extents;
		extents.z = 10f;
		m_viewBounds.extents = extents;
		Vector3 extents2 = m_playerBounds.extents;
		extents2.z = 10f;
		m_playerBounds.extents = extents2;
		if (m_regularVirtualCamera != null)
		{
			m_regularVirtualCamera.enabled = false;
		}
	}

	public void SetupConfiner(CompositeCollider2D collider, bool invalidateCache)
	{
		if (m_confiner != null)
		{
			m_confiner.m_BoundingShape2D = collider;
			if (invalidateCache)
			{
				m_confiner.InvalidateCache();
			}
		}
	}

	private void OnEnable()
	{
		GlobalReferences.Instance.Sets.Camera.ActiveCameraBoundsExtenderSet.Register(AddCameraBoundsExtender, RemoveCameraBoundsExtender);
		DisableCamera();
		if (m_boundsEnabled)
		{
			GlobalReferences.Instance.Sets.Camera.ActiveCameraBoundsSet.Add(this);
			GlobalReferences.Instance.Anchors.Camera.ActiveCameraBoundsAnchor.Register(OnCameraBoundsChanged);
			OnCameraBoundsChanged(GlobalReferences.Instance.Anchors.Camera.ActiveCameraBoundsAnchor.Item);
		}
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.Sets.Camera.ActiveCameraBoundsExtenderSet.Unregister(AddCameraBoundsExtender, RemoveCameraBoundsExtender);
		DisableCamera();
		GlobalReferences.Instance.Sets.Camera.ActiveCameraBoundsSet.Remove(this);
		GlobalReferences.Instance.Anchors.Camera.ActiveCameraBoundsAnchor.Unregister(OnCameraBoundsChanged);
	}

	private void AddCameraBoundsExtender(CameraBoundsExtender extender)
	{
		if (extender.OverlapsCameraBounds(this))
		{
			Bounds worldViewBounds = WorldViewBounds;
			float num = Mathf.Abs(worldViewBounds.min.x - extender.transform.position.x);
			float num2 = Mathf.Abs(worldViewBounds.max.x - extender.transform.position.x);
			if (num2 < num && num2 < worldViewBounds.extents.x * 0.5f)
			{
				m_rightEdgeDetectedExtender = extender;
			}
			else if (num2 > num && num < worldViewBounds.extents.x * 0.5f)
			{
				m_leftEdgeDetectedExtender = extender;
			}
		}
	}

	private void RemoveCameraBoundsExtender(CameraBoundsExtender extender)
	{
	}

	private void OnCameraBoundsChanged(CameraBounds bounds)
	{
		m_boundsCameraActive = bounds == this;
		if (m_boundsCameraActive)
		{
			ActivateCamera();
		}
		else
		{
			DisableCamera();
		}
	}

	public bool IsInBounds(Vector2 position, bool checkPlayerBounds)
	{
		Vector3 point = m_transform.InverseTransformPoint(position);
		point.z = 0f;
		return ((!checkPlayerBounds || !m_usePlayerBounds) ? m_viewBounds : m_playerBounds).Contains(point);
	}

	public bool IsInBounds(Bounds followCharacterBounds, bool checkPlayerBounds)
	{
		Vector3 center = m_transform.InverseTransformPoint(followCharacterBounds.center);
		center.z = 0f;
		followCharacterBounds.center = center;
		return ((!checkPlayerBounds || !m_usePlayerBounds) ? m_viewBounds : m_playerBounds).Intersects(followCharacterBounds);
	}

	public float GetIntersectAmount(Bounds followCharacterBounds, bool checkPlayerBounds)
	{
		Vector3 center = m_transform.InverseTransformPoint(followCharacterBounds.center);
		center.z = 0f;
		followCharacterBounds.center = center;
		Bounds bounds = ((!checkPlayerBounds || !m_usePlayerBounds) ? m_viewBounds : m_playerBounds);
		float result = 0f;
		if (followCharacterBounds.min.x > bounds.min.x && followCharacterBounds.max.x < bounds.max.x)
		{
			result = 1f;
		}
		else if (followCharacterBounds.min.x < bounds.min.x && followCharacterBounds.max.x > bounds.min.x)
		{
			result = Mathf.InverseLerp(followCharacterBounds.min.x, followCharacterBounds.max.x, bounds.min.x);
		}
		else if (followCharacterBounds.max.x > bounds.max.x && followCharacterBounds.min.x < bounds.max.x)
		{
			result = Mathf.InverseLerp(followCharacterBounds.min.x, followCharacterBounds.max.x, bounds.max.x);
		}
		return result;
	}

	public bool IsInBounds(Vector2[] positions, bool checkPlayerBounds)
	{
		foreach (Vector2 position in positions)
		{
			if (IsInBounds(position, checkPlayerBounds))
			{
				return true;
			}
		}
		return false;
	}

	public float DistanceSqrFromBounds(Vector2[] positions, bool checkPlayerBounds)
	{
		float num = float.MaxValue;
		foreach (Vector2 position in positions)
		{
			num = Mathf.Min(DistanceSqrFromBounds(position, checkPlayerBounds), num);
		}
		return num;
	}

	public float DistanceSqrFromBounds(Vector2 position, bool checkPlayerBounds)
	{
		Vector2 vector = base.transform.InverseTransformPoint(position);
		if (checkPlayerBounds && m_usePlayerBounds)
		{
			return m_playerBounds.SqrDistance(vector);
		}
		return m_viewBounds.SqrDistance(vector);
	}

	public Vector2 ClosestPoint(Vector2 position)
	{
		Vector3 point = m_transform.InverseTransformPoint(position);
		Vector3 position2 = m_viewBounds.ClosestPoint(point);
		return m_transform.TransformPoint(position2);
	}

	private void OnDrawGizmos()
	{
		Color yellow = Color.yellow;
		yellow.a = 0.25f;
		Gizmos.color = yellow;
		Gizmos.DrawWireCube(base.transform.position + m_viewBounds.center, m_viewBounds.size);
		if (m_usePlayerBounds)
		{
			yellow = Color.cyan;
			yellow.a = 0.25f;
			Gizmos.color = yellow;
			Gizmos.DrawWireCube(base.transform.position + m_playerBounds.center, m_playerBounds.size);
		}
		Gizmos.color = Color.white;
	}

	private void OnDrawGizmosSelected()
	{
		Color yellow = Color.yellow;
		yellow.a = 0.25f;
		Gizmos.color = yellow;
		Gizmos.DrawCube(base.transform.position + m_viewBounds.center, m_viewBounds.size);
		if (m_leftEdgeSettings.m_edgeYawStartDistance > 0f)
		{
			float leftTiltEdgeLocalPosition = GetLeftTiltEdgeLocalPosition();
			Vector3 min = m_viewBounds.min;
			min.x = leftTiltEdgeLocalPosition;
			min.y = m_viewBounds.center.y;
			Vector3 from = min;
			from.y -= m_viewBounds.extents.y;
			Vector3 to = min;
			to.y += m_viewBounds.extents.y;
			from += base.transform.position;
			to += base.transform.position;
			Gizmos.color = Color.white;
			Gizmos.DrawLine(from, to);
		}
		if (m_rightEdgeSettings.m_edgeYawStartDistance > 0f)
		{
			float rightTiltEdgeLocalPosition = GetRightTiltEdgeLocalPosition();
			Vector3 min2 = m_viewBounds.min;
			min2.x = rightTiltEdgeLocalPosition;
			min2.y = m_viewBounds.center.y;
			Vector3 from2 = min2;
			from2.y -= m_viewBounds.extents.y;
			Vector3 to2 = min2;
			to2.y += m_viewBounds.extents.y;
			from2 += base.transform.position;
			to2 += base.transform.position;
			Gizmos.color = Color.white;
			Gizmos.DrawLine(from2, to2);
		}
		Gizmos.color = Color.white;
	}

	public void DebugGUI()
	{
		GUILayout.Label("Mode: " + m_activeCameraMode);
	}
}
