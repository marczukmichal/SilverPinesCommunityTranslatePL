using System;
using System.Collections.Generic;
using Cinemachine;
using Shapes;
using UnityEngine;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class FollowPlayerVirtualCamera : MonoBehaviour
{
	[Serializable]
	public class CameraSettings
	{
		[SerializeField]
		private float m_fieldOfView = 10f;

		[SerializeField]
		private float m_distance = 30f;

		[SerializeField]
		private Vector3 m_trackingOffset = new Vector3(0f, 0f, 0f);

		public float FieldOfView => m_fieldOfView;

		public float Distance => m_distance;

		public Vector3 TrackingOffset => m_trackingOffset;

		public CameraSettings()
		{
		}

		public CameraSettings(float fov, float distance, Vector3 trackingOffset)
		{
			m_fieldOfView = fov;
			m_distance = distance;
			m_trackingOffset = trackingOffset;
		}
	}

	[SerializeField]
	private CinemachineVirtualCamera m_virtualCamera;

	[SerializeField]
	private bool m_startEnabled;

	[Header("Health Wobble")]
	[SerializeField]
	private FloatVariable m_playerHealthPercentage;

	[SerializeField]
	private AnimationCurve m_healthNoiseAmplitudeCurve;

	[SerializeField]
	private AnimationCurve m_healthNoiseFrequencyCurve;

	[Header("Movement")]
	[SerializeField]
	private AnimationCurve m_followSmoothTimeDistanceCurve;

	[SerializeField]
	private float m_rotationEnableTime;

	[SerializeField]
	private float m_edgeOffsetEnableTranslationTime;

	[SerializeField]
	private float m_rotationDisableTime;

	[SerializeField]
	private float m_edgeOffsetDisableTranslationTime;

	[SerializeField]
	private float m_edgeOffsetScalar = 1f;

	[Header("Lookahead")]
	[SerializeField]
	private float m_lookaheadScale;

	[SerializeField]
	private float m_lookaheadDampingTime;

	[Header("Bounds Tweaks")]
	[SerializeField]
	private float m_boundsExpandX = 1.5f;

	[Header("Aspect Ratio FOV Tweak")]
	public float m_startAspect = 2.333f;

	public float m_endAspect = 3.556f;

	[Header("Curve")]
	public AnimationCurve m_fovCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	private Vector3 m_followVelocity;

	private Vector3 m_previousTargetPosition;

	private List<CameraBounds> m_activeCameraBounds;

	private Bounds m_activeBounds;

	private static float s_force_camera_fov;

	[DebugCommand("camera_edge_rotate", "Enable/disable camera rotating to show walls near edge of camera bounds", "camera_edge_rotate <true/false>", typeof(bool), false)]
	public static bool CAMERA_EDGE_ROTATE;

	private CinemachineBasicMultiChannelPerlin m_noise;

	private bool m_initialised;

	private float m_baseAmplitude;

	private float m_baseFrequency;

	private Vector3 m_cameraFocusCurrentOffset;

	private float m_distanceVelocity;

	private float m_FOVVelocity;

	private Transform m_cameraFollowTransform;

	private float m_currentRotationYAngle;

	private float m_rotationChangeVelocity;

	private float m_currentXOffset;

	private float m_currentXOffsetVelocity;

	private Vector3 m_lookaheadOffset;

	private Vector3 m_lookaheadOffsetVelocity;

	private float m_cameraDistance;

	private bool m_hasHadCameraRebounds;

	private bool m_pauseCameraFollow;

	[SerializeField]
	private CameraSettings m_defaultCameraSettings;

	[SerializeField]
	private float m_settingsTransitionSpeed;

	private Vector3 m_boundedPositionDebug;

	public Bounds ActiveBounds => m_activeBounds;

	[DebugCommand("force_camera_fov", "Force a specific camera FOV to follow cameras. Set to 0 to disable", "force_camera_fov <value>", typeof(float), false)]
	private static void DebugForceCameraFOV(float value)
	{
		s_force_camera_fov = value;
	}

	[DebugCommand("pause_camera", "Toggle pause camera", "pause_camera <value>", typeof(bool), false)]
	private static void DebugPauseCamera(bool pausecamera)
	{
		FollowPlayerVirtualCamera followPlayerVirtualCamera = UnityEngine.Object.FindFirstObjectByType<FollowPlayerVirtualCamera>();
		if (followPlayerVirtualCamera != null)
		{
			followPlayerVirtualCamera.m_pauseCameraFollow = pausecamera;
		}
	}

	public CameraSettings GetCameraSettings()
	{
		CameraBounds item = GlobalReferences.Instance.Anchors.Camera.ActiveCameraBoundsAnchor.Item;
		if (item != null)
		{
			return item.CameraSettings;
		}
		return m_defaultCameraSettings;
	}

	private void Reset()
	{
		m_virtualCamera = GetComponent<CinemachineVirtualCamera>();
	}

	private void Awake()
	{
		m_virtualCamera.enabled = m_startEnabled;
		m_noise = m_virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
		m_baseAmplitude = m_noise.m_AmplitudeGain;
		m_baseFrequency = m_noise.m_FrequencyGain;
		m_activeCameraBounds = new List<CameraBounds>();
	}

	private void OnEnable()
	{
		GameObject gameObject = new GameObject("Camera Follow Helper");
		m_cameraFollowTransform = gameObject.transform;
		OnActiveCameraBoundChanged(GlobalReferences.Instance.Anchors.Camera.ActiveCameraBoundsAnchor.Item);
		GlobalReferences.Instance.Anchors.Camera.ActiveCameraBoundsAnchor.Register(OnActiveCameraBoundChanged);
		GlobalReferences.Instance.Sets.Camera.ActiveCameraBoundsExtenderSet.Register(AddCameraBoundsExtender, RemoveCameraBoundsExtender);
		GlobalReferences.Instance.EventChannels.Camera.PauseCameraFollow.Register(SetPauseCameraFollow);
		GlobalReferences.Instance.EventChannels.Camera.ForceCameraReset.Register(ForceCameraReset);
		GlobalReferences.Instance.EventChannels.Camera.ResetCameraLookahead.Register(ResetLookahead);
	}

	private void OnDisable()
	{
		if (m_cameraFollowTransform != null)
		{
			UnityEngine.Object.Destroy(m_cameraFollowTransform.gameObject);
			m_cameraFollowTransform = null;
		}
		GlobalReferences.Instance.Anchors.Camera.ActiveCameraBoundsAnchor.Unregister(OnActiveCameraBoundChanged);
		GlobalReferences.Instance.Sets.Camera.ActiveCameraBoundsExtenderSet.Unregister(AddCameraBoundsExtender, RemoveCameraBoundsExtender);
		GlobalReferences.Instance.EventChannels.Camera.PauseCameraFollow.Unregister(SetPauseCameraFollow);
		GlobalReferences.Instance.EventChannels.Camera.ForceCameraReset.Unregister(ForceCameraReset);
		GlobalReferences.Instance.EventChannels.Camera.ResetCameraLookahead.Unregister(ResetLookahead);
	}

	private void SetPauseCameraFollow(bool pauseFollow)
	{
		m_pauseCameraFollow = pauseFollow;
	}

	private void FixedUpdate()
	{
		if (Camera.main == null)
		{
			return;
		}
		CameraSettings cameraSettings = GetCameraSettings();
		CameraTargetData cameraTargetData = GlobalReferences.Instance.CameraTargetData;
		if (!cameraTargetData.m_active || !m_virtualCamera.enabled)
		{
			return;
		}
		Vector3 vector = ((!m_pauseCameraFollow) ? cameraTargetData.m_position : m_previousTargetPosition);
		vector.z = 0f;
		if (!m_initialised)
		{
			m_previousTargetPosition = vector;
			m_lookaheadOffset = Vector3.zero;
		}
		Vector3 target = (vector - m_previousTargetPosition) * Time.deltaTime * m_lookaheadScale;
		target.y = 0f;
		m_previousTargetPosition = vector;
		Vector3 baseOffset = cameraTargetData.m_baseOffset;
		if (!m_pauseCameraFollow)
		{
			baseOffset += cameraTargetData.m_userOffset + cameraSettings.TrackingOffset;
			m_lookaheadOffset = Vector3.SmoothDamp(m_lookaheadOffset, target, ref m_lookaheadOffsetVelocity, m_lookaheadDampingTime);
		}
		else
		{
			m_lookaheadOffset = Vector3.zero;
		}
		vector += baseOffset;
		vector += m_lookaheadOffset;
		Vector3 vector2 = ApplyCameraBounds(vector);
		if (!m_initialised)
		{
			m_cameraFollowTransform.position = vector2;
		}
		Quaternion quaternion = Quaternion.identity;
		Vector3 position = m_cameraFollowTransform.position;
		position.z -= m_cameraDistance;
		float num = 0f;
		float num2 = 1f;
		float num3 = 1f;
		bool flag = true;
		if (CAMERA_EDGE_ROTATE && m_activeCameraBounds.Count > 0 && m_activeCameraBounds[0].ActiveVirtualCamera != 0)
		{
			if (m_activeCameraBounds[0].ActiveVirtualCamera == CameraBounds.ActiveCameraMode.RightEdge)
			{
				num3 = m_activeCameraBounds[0].RightEdgeSettings.m_edgeYawAngleScalar;
				num2 = m_activeCameraBounds[0].RightEdgeSettings.m_edgePositionOffsetScalar;
			}
			else if (m_activeCameraBounds[0].ActiveVirtualCamera == CameraBounds.ActiveCameraMode.LeftEdge)
			{
				num3 = m_activeCameraBounds[0].LeftEdgeSettings.m_edgeYawAngleScalar;
				num2 = m_activeCameraBounds[0].LeftEdgeSettings.m_edgePositionOffsetScalar;
			}
			Vector3 vector3 = vector;
			vector3.y = position.y;
			quaternion = Quaternion.LookRotation((vector3 - position).normalized, Vector3.up);
			num = (vector2.x - vector.x) * m_edgeOffsetScalar * num2 * num3 * m_cameraDistance;
			flag = false;
		}
		position.x += m_currentXOffset;
		float num4 = quaternion.eulerAngles.y;
		if (num4 > 180f)
		{
			num4 -= 360f;
		}
		num4 *= num3;
		float num5 = ((s_force_camera_fov > 0f) ? s_force_camera_fov : (cameraSettings.FieldOfView * cameraTargetData.m_zoomScale));
		float value = (float)Screen.width / (float)Screen.height;
		float value2 = Mathf.InverseLerp(m_startAspect, m_endAspect, value);
		value2 = Mathf.Clamp01(value2);
		float num6 = m_fovCurve.Evaluate(value2);
		num5 *= num6;
		if (!m_initialised)
		{
			m_virtualCamera.PreviousStateIsValid = false;
			m_initialised = true;
			m_cameraFocusCurrentOffset = baseOffset;
			m_cameraFollowTransform.position = vector2;
			m_cameraDistance = cameraSettings.Distance;
			m_currentXOffset = num;
			m_currentRotationYAngle = num4;
			m_virtualCamera.m_Lens.FieldOfView = num5;
			m_lookaheadOffset = Vector3.zero;
		}
		else
		{
			m_cameraFocusCurrentOffset = Vector3.MoveTowards(m_cameraFocusCurrentOffset, baseOffset, Time.deltaTime * 10f);
			float time = Vector3.Distance(m_cameraFollowTransform.position, vector2);
			float smoothTime = m_followSmoothTimeDistanceCurve.Evaluate(time);
			m_cameraFollowTransform.position = Vector3.SmoothDamp(m_cameraFollowTransform.position, vector2, ref m_followVelocity, smoothTime);
			m_cameraDistance = Mathf.SmoothDamp(m_cameraDistance, cameraSettings.Distance, ref m_distanceVelocity, m_settingsTransitionSpeed);
			m_currentXOffset = Mathf.SmoothDamp(m_currentXOffset, num, ref m_currentXOffsetVelocity, flag ? m_edgeOffsetDisableTranslationTime : m_edgeOffsetEnableTranslationTime);
			m_currentRotationYAngle = Mathf.SmoothDampAngle(m_currentRotationYAngle, num4, ref m_rotationChangeVelocity, flag ? m_rotationDisableTime : m_rotationEnableTime);
			m_virtualCamera.m_Lens.FieldOfView = Mathf.SmoothDamp(m_virtualCamera.m_Lens.FieldOfView, num5, ref m_FOVVelocity, m_settingsTransitionSpeed);
		}
		m_boundedPositionDebug = vector2;
		base.transform.position = position;
		base.transform.rotation = Quaternion.Euler(0f, m_currentRotationYAngle, 0f);
		float num7 = m_healthNoiseAmplitudeCurve.Evaluate(Mathf.Clamp01(m_playerHealthPercentage.Value));
		float num8 = m_healthNoiseFrequencyCurve.Evaluate(Mathf.Clamp01(m_playerHealthPercentage.Value));
		m_noise.m_AmplitudeGain = m_baseAmplitude * num7;
		m_noise.m_FrequencyGain = m_baseFrequency * num8;
	}

	public void ResetOffset()
	{
		CameraTargetData cameraTargetData = GlobalReferences.Instance.CameraTargetData;
		m_cameraFocusCurrentOffset = cameraTargetData.m_baseOffset;
	}

	public void DrawDebug()
	{
		if (m_cameraFollowTransform != null)
		{
			Draw.Sphere(m_cameraFollowTransform.position, 0.25f, Color.green);
		}
		Draw.Sphere(m_boundedPositionDebug, 0.125f, Color.magenta);
		Color red = Color.red;
		red.a = 0.2f;
		GameDebugDrawer.Draw2DBounds(m_activeBounds, red);
	}

	public void DebugText()
	{
		Vector3 cameraFocusCurrentOffset = m_cameraFocusCurrentOffset;
		GUILayout.Label("m_cameraFocusCurrentOffset: " + cameraFocusCurrentOffset.ToString());
		GUILayout.Label("m_cameraFollowTransform: " + m_cameraFollowTransform.position.ToString());
		GUILayout.Label("m_cameraDistance: " + m_cameraDistance);
		GUILayout.Label("m_currentXOffset: " + m_currentXOffset);
		GUILayout.Label("m_currentRotationYAngle: " + m_currentRotationYAngle);
		GUILayout.Label("m_virtualCamera.m_Lens.FieldOfView: " + m_virtualCamera.m_Lens.FieldOfView);
	}

	private void ResetLookahead()
	{
		m_lookaheadOffset = Vector3.zero;
		m_lookaheadOffsetVelocity = Vector3.zero;
	}

	private void ClearCurrentBounds()
	{
		m_activeCameraBounds.Clear();
	}

	private void OnActiveCameraBoundChanged(CameraBounds cameraBound)
	{
		SetActiveCameraBound(cameraBound, invalidateCache: false);
	}

	private void SetActiveCameraBound(CameraBounds cameraBounds, bool invalidateCache)
	{
		ClearCurrentBounds();
		if (cameraBounds != null)
		{
			if (!m_activeCameraBounds.Contains(cameraBounds))
			{
				m_activeCameraBounds.Add(cameraBounds);
			}
			foreach (CameraBoundsExtender item in GlobalReferences.Instance.Sets.Camera.ActiveCameraBoundsExtenderSet.Items)
			{
				if (!item.OverlapsCameraBounds(cameraBounds))
				{
					continue;
				}
				foreach (CameraBounds item2 in GlobalReferences.Instance.Sets.Camera.ActiveCameraBoundsSet.Items)
				{
					if (!(item2 == cameraBounds) && item.OverlapsCameraBounds(item2))
					{
						m_activeCameraBounds.Add(item2);
					}
				}
			}
		}
		if (cameraBounds != null && !m_hasHadCameraRebounds)
		{
			m_hasHadCameraRebounds = true;
			m_initialised = false;
		}
	}

	private void ForceCameraReset()
	{
		m_initialised = false;
	}

	private void AddCameraBoundsExtender(CameraBoundsExtender arg0)
	{
		SetActiveCameraBound(GlobalReferences.Instance.Anchors.Camera.ActiveCameraBoundsAnchor.Item, invalidateCache: true);
	}

	private void RemoveCameraBoundsExtender(CameraBoundsExtender arg0)
	{
		SetActiveCameraBound(GlobalReferences.Instance.Anchors.Camera.ActiveCameraBoundsAnchor.Item, invalidateCache: true);
	}

	private Vector3 ApplyCameraBounds(Vector3 position)
	{
		float aspect = Camera.main.aspect;
		CameraState state = m_virtualCamera.State;
		float num = CalculateHalfFrustumHeight(in state, in m_cameraDistance);
		float num2 = num * aspect;
		if (m_activeCameraBounds.Count > 0)
		{
			m_activeBounds = m_activeCameraBounds[0].WorldViewBounds;
			for (int i = 1; i < m_activeCameraBounds.Count; i++)
			{
				m_activeBounds.Encapsulate(m_activeCameraBounds[i].WorldViewBounds);
			}
			Vector3 extents = m_activeBounds.extents;
			if (!CAMERA_EDGE_ROTATE)
			{
				extents.x += m_boundsExpandX;
			}
			extents.x -= num2;
			extents.y -= num;
			extents.x = Mathf.Max(0f, extents.x);
			extents.y = Mathf.Max(0f, extents.y);
			m_activeBounds.extents = extents;
			Vector3 min = m_activeBounds.min;
			Vector3 max = m_activeBounds.max;
			if (CAMERA_EDGE_ROTATE)
			{
				if (m_activeCameraBounds[0].ActiveVirtualCamera == CameraBounds.ActiveCameraMode.LeftEdge)
				{
					float x = m_activeCameraBounds[0].transform.TransformPoint(new Vector3(m_activeCameraBounds[0].GetLeftTiltEdgeLocalPosition(), 0f, 0f)).x;
					min.x = Mathf.Max(x, min.x);
					max.x = Mathf.Max(min.x, max.x);
				}
				else if (m_activeCameraBounds[0].ActiveVirtualCamera == CameraBounds.ActiveCameraMode.RightEdge)
				{
					float x2 = m_activeCameraBounds[0].transform.TransformPoint(new Vector3(m_activeCameraBounds[0].GetRightTiltEdgeLocalPosition(), 0f, 0f)).x;
					max.x = Mathf.Min(x2, max.x);
					min.x = Mathf.Min(min.x, max.x);
				}
			}
			m_activeBounds.SetMinMax(min, max);
			Vector3 result = m_activeBounds.ClosestPoint(position);
			if (m_activeCameraBounds[0].CameraHeightPositioningMode == CameraBounds.CameraHeightPositioning.FixedHeight)
			{
				result.y = m_activeCameraBounds[0].GetFixedHeightY();
			}
			else if (m_activeCameraBounds[0].CameraHeightPositioningMode == CameraBounds.CameraHeightPositioning.IgnoreBounds)
			{
				result.y = position.y;
			}
			return result;
		}
		return position;
	}

	private float CalculateHalfFrustumHeight(in CameraState state, in float cameraPosLocalZ)
	{
		float f = ((!state.Lens.Orthographic) ? (cameraPosLocalZ * Mathf.Tan(state.Lens.FieldOfView * 0.5f * (MathF.PI / 180f))) : state.Lens.OrthographicSize);
		return Mathf.Abs(f);
	}
}
