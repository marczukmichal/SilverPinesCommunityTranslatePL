using Cinemachine;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCameraShake", menuName = "Settings/Camera Shake Settings")]
public class CameraShakeSettings : ScriptableObject
{
	public enum CameraShakeYMode
	{
		RandomRange,
		SetValue
	}

	[SerializeField]
	private float m_roughness = 10f;

	[SerializeField]
	private float m_fadeInTime = 0.2f;

	[SerializeField]
	private float m_fadeOutTime = 0.2f;

	[Header("Cinemachine Settings")]
	[SerializeField]
	private CinemachineImpulseDefinition m_impulseSettings;

	[SerializeField]
	private Vector3 m_impulseDirectionOverride = Vector3.zero;

	[SerializeField]
	private float m_magnitude = 3f;

	[Header("Direction")]
	public bool m_matchCharacterDirection;

	public float m_characterDirectionXScalar = 1f;

	public CameraShakeYMode m_yAxisDirectionMode;

	public float m_matchCharacterDirectionYValue;

	[Header("Controller Rumble / Vibration")]
	[SerializeField]
	private bool m_useRumble;

	[SerializeField]
	private ControllerRumbleSettings m_rumble;

	public float Roughness => m_roughness;

	public float FadeInTime => m_fadeInTime;

	public float FadeOutTime => m_fadeOutTime;

	public CinemachineImpulseDefinition ImpulseSettings => m_impulseSettings;

	public Vector3 ImpulseDirectionOverride => m_impulseDirectionOverride;

	public float Magnitude => m_magnitude;

	public bool UseRumble => m_useRumble;

	public ControllerRumbleSettings Rumble => m_rumble;
}
