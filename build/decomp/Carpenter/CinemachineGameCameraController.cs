using Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

public class CinemachineGameCameraController : MonoBehaviour
{
	[SerializeField]
	private CinemachineBrain m_cinemachineBrain;

	[SerializeField]
	private CinemachineVirtualCamera m_regularCamera;

	[SerializeField]
	private ActiveCameraBoundsSet m_activeCameraBoundsSet;

	[SerializeField]
	private CameraShakeEventChannel m_cameraShakeChannel;

	[SerializeField]
	private CameraTargetData m_targetData;

	[SerializeField]
	private Transform m_audioListenerTransform;

	[SerializeField]
	private float m_audioListenerOffset;

	[Header("Post Processing / Depth of Field")]
	[FormerlySerializedAs("m_depthOfFieldFocusDistanceOffset")]
	[SerializeField]
	private float m_depthOfFieldBaseFocusDistanceOffset;

	[SerializeField]
	private float m_depthOfFieldFocusDistanceAdjustTime;

	public UnityAction<float> OnDOFFocusDistance;

	private float m_currentDOFFocusOffset;

	private float m_depthOfFieldOffsetVelocity;

	private Volume m_postProcessingVolume;

	private Camera m_camera;

	private Transform m_cameraTransform;

	private bool m_pauseCameraFollow;

	private bool m_lockCameraBounds;

	private DepthOfField m_depthOfField;

	private void SetPauseCameraFollow(bool pauseFollow)
	{
		m_pauseCameraFollow = pauseFollow;
	}

	private void LockCameraBounds(bool lockedBounds)
	{
		m_lockCameraBounds = lockedBounds;
	}

	private void OnEnable()
	{
		m_camera = GetComponentInChildren<Camera>();
		m_cameraTransform = GetComponent<Transform>();
		m_cameraShakeChannel.Register(TriggerCameraShake);
		m_currentDOFFocusOffset = m_depthOfFieldBaseFocusDistanceOffset;
		GlobalReferences.Instance.EventChannels.Camera.NewCamera.Raise(m_camera);
		GlobalReferences.Instance.EventChannels.Camera.SetMainCameraEnabled.Register(OnSetMainCameraEnabledEvent);
		GlobalReferences.Instance.EventChannels.Camera.PauseCameraFollow.Register(SetPauseCameraFollow);
		GlobalReferences.Instance.EventChannels.Camera.LockActiveCameraBounds.Register(LockCameraBounds);
		GlobalReferences.Instance.EventChannels.Camera.CameraFollowDataInitialised.Register(OnCameraFollowInitialised);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.Camera.SetMainCameraEnabled.Unregister(OnSetMainCameraEnabledEvent);
		GlobalReferences.Instance.Anchors.Camera.ActiveCameraBoundsAnchor.Set(null);
		GlobalReferences.Instance.EventChannels.Camera.PauseCameraFollow.Unregister(SetPauseCameraFollow);
		GlobalReferences.Instance.EventChannels.Camera.LockActiveCameraBounds.Unregister(LockCameraBounds);
		GlobalReferences.Instance.EventChannels.Camera.CameraFollowDataInitialised.Unregister(OnCameraFollowInitialised);
		m_cameraShakeChannel.Unregister(TriggerCameraShake);
	}

	private void OnCameraFollowInitialised()
	{
		UpdatePostProcessingVolume();
		UpdateActiveBoundsSettings();
	}

	private void OnSetMainCameraEnabledEvent(bool enabled)
	{
		m_camera.enabled = enabled;
	}

	private void Start()
	{
		m_cinemachineBrain.m_UpdateMethod = CinemachineBrain.UpdateMethod.FixedUpdate;
		m_cinemachineBrain.m_BlendUpdateMethod = CinemachineBrain.BrainUpdateMethod.FixedUpdate;
	}

	private void UpdatePostProcessingVolume()
	{
		Volume volume = null;
		if (TimeOfDaySceneLighting.ActiveSceneLighting != null)
		{
			volume = TimeOfDaySceneLighting.ActiveSceneLighting.PostProcessingVolume;
		}
		if (m_postProcessingVolume != volume)
		{
			m_postProcessingVolume = volume;
			if (volume != null)
			{
				volume.profile.TryGet<DepthOfField>(out m_depthOfField);
			}
			else
			{
				m_depthOfField = null;
			}
		}
	}

	private void Update()
	{
		UpdatePostProcessingVolume();
		UpdateActiveBoundsSettings();
		if (m_audioListenerTransform != null && m_targetData.m_active)
		{
			Vector3 position = m_audioListenerTransform.position;
			position.z = m_targetData.m_position.z;
			position.z += m_audioListenerOffset;
			m_audioListenerTransform.position = position;
		}
		if (m_targetData.m_active)
		{
			float z = m_targetData.m_position.z;
			float z2 = m_cameraTransform.position.z;
			float num = z - z2 + m_currentDOFFocusOffset;
			float num2 = m_depthOfFieldBaseFocusDistanceOffset;
			CinemachineCameraBlend item = GlobalReferences.Instance.Anchors.Camera.CinemachineCameraBlendAnchor.Item;
			if (item != null && item.ChangeDepthOfField && item.IsCameraActive)
			{
				num2 += item.DepthOfFieldOffset;
			}
			if (GlobalReferences.Instance.Anchors.Camera.ActiveCameraBoundsAnchor.Item != null)
			{
				num2 += GlobalReferences.Instance.Anchors.Camera.ActiveCameraBoundsAnchor.Item.DepthOfFieldOffset;
			}
			m_currentDOFFocusOffset = Mathf.SmoothDamp(m_currentDOFFocusOffset, num2, ref m_depthOfFieldOffsetVelocity, m_depthOfFieldFocusDistanceAdjustTime);
			if (m_depthOfField != null)
			{
				m_depthOfField.focusDistance.value = num;
			}
			OnDOFFocusDistance?.Invoke(num);
		}
	}

	private void UpdateActiveBoundsSettings()
	{
		if (!m_targetData.m_active || m_pauseCameraFollow || m_lockCameraBounds)
		{
			return;
		}
		Vector3 position = m_targetData.m_position;
		Bounds characterBounds = m_targetData.m_characterBounds;
		CameraBounds cameraBounds = null;
		int num = int.MinValue;
		float num2 = 0f;
		foreach (CameraBounds item in m_activeCameraBoundsSet.Items)
		{
			CameraBounds.ActiveCameraMode activeVirtualCamera = CameraBounds.ActiveCameraMode.Normal;
			if (item.IsInBounds(characterBounds, checkPlayerBounds: true))
			{
				bool flag = false;
				if (item.Priority > num || cameraBounds == null)
				{
					flag = true;
				}
				else if (item.Priority == num && item.GetIntersectAmount(characterBounds, checkPlayerBounds: true) >= num2)
				{
					flag = true;
				}
				if (flag)
				{
					cameraBounds = item;
					num = item.Priority;
					num2 = item.GetIntersectAmount(characterBounds, checkPlayerBounds: true);
				}
				if (item.LeftEdgeSettings.CanUseEdgeCamera() && !item.LeftEdgeExtended)
				{
					float num3 = item.GetLeftTiltEdgeLocalPosition() + item.transform.position.x;
					if (position.x <= num3)
					{
						activeVirtualCamera = CameraBounds.ActiveCameraMode.LeftEdge;
					}
				}
				if (item.RightEdgeSettings.CanUseEdgeCamera() && !item.RightEdgeExtended)
				{
					float num4 = item.GetRightTiltEdgeLocalPosition() + item.transform.position.x;
					if (position.x >= num4)
					{
						activeVirtualCamera = CameraBounds.ActiveCameraMode.RightEdge;
					}
				}
			}
			item.ActiveVirtualCamera = activeVirtualCamera;
		}
		if (cameraBounds == null)
		{
			float num5 = float.MaxValue;
			foreach (CameraBounds item2 in m_activeCameraBoundsSet.Items)
			{
				float num6 = item2.DistanceSqrFromBounds(position, checkPlayerBounds: true);
				if (num6 < num5)
				{
					num5 = num6;
					cameraBounds = item2;
				}
			}
		}
		if (cameraBounds != GlobalReferences.Instance.Anchors.Camera.ActiveCameraBoundsAnchor.Item)
		{
			GlobalReferences.Instance.Anchors.Camera.ActiveCameraBoundsAnchor.Set(cameraBounds);
		}
	}

	private void TriggerCameraShake(CameraShakeEventData shakeEvent)
	{
		Vector3 velocity = shakeEvent.m_direction;
		if (shakeEvent.m_cameraShakeSettings.ImpulseDirectionOverride != Vector3.zero)
		{
			velocity = shakeEvent.m_cameraShakeSettings.ImpulseDirectionOverride;
		}
		velocity.Normalize();
		velocity *= shakeEvent.m_cameraShakeSettings.Magnitude;
		shakeEvent.m_cameraShakeSettings.ImpulseSettings.CreateEvent(shakeEvent.m_position, velocity);
	}

	private void OnGUI()
	{
		if (!GameDebugCommands.CAMERA_DEBUG)
		{
			return;
		}
		GUILayout.BeginArea(new Rect((float)Screen.width - 550f, (float)Screen.height * 0.1f, 420f, 512f));
		GUILayout.Label("Camera Debug");
		CameraBounds item = GlobalReferences.Instance.Anchors.Camera.ActiveCameraBoundsAnchor.Item;
		if (item != null)
		{
			GUILayout.Label("Active Camerabound: " + item.name);
			item.DebugGUI();
		}
		else
		{
			GUILayout.Label("No active camera bound!");
		}
		ICinemachineCamera activeVirtualCamera = m_cinemachineBrain.ActiveVirtualCamera;
		if (activeVirtualCamera != null)
		{
			FollowPlayerVirtualCamera component = activeVirtualCamera.VirtualCameraGameObject.GetComponent<FollowPlayerVirtualCamera>();
			if (component != null)
			{
				component.DebugText();
			}
		}
		GUILayout.EndArea();
	}
}
