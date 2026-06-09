using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using VLB;

public class PlayerFlashlight : MonoBehaviour
{
	[Serializable]
	private class FlashlightLightSettings
	{
		public Light m_light;

		[Header("Mode Settings")]
		public float m_intensityScalar;

		public Vector3 m_offset;
	}

	[Serializable]
	private struct EnvironmentLightingSettings
	{
		public float m_spotlightRange;

		public float m_spotlightIntensity;

		public float m_spotlightInnerAngle;

		[Tooltip("This is the outer angle when the player is not against a wall")]
		public float m_spotlightOuterAngleMin;

		[Tooltip("This is the outer angle when the player is right up against a wall")]
		public float m_spotlightOuterAngleMax;
	}

	public enum PlayerLightType
	{
		Flashlight,
		UVFlashlight,
		Lantern
	}

	public enum FlashlightPosition
	{
		Normal,
		RotatedAway,
		Aiming,
		RotatedTowards
	}

	[Serializable]
	private class FlashlightModeSettings
	{
		public FlashlightPosition m_mode;

		public Vector3 m_offset;

		public Vector3 m_rotation;

		public float m_lensFlareIntensityScalar;

		public float m_lightBeamIntensityScalar;

		[SerializeField]
		public List<FlashlightLightSettings> m_flashlightLightSettings;
	}

	private struct BaseLightState
	{
		public float m_intensity;

		public Vector3 m_localPosition;
	}

	[SerializeField]
	private FloatVariable m_enemyPresence;

	private Dictionary<Light, BaseLightState> m_baseLightState;

	[SerializeField]
	private VolumetricLightBeamSD m_lightBeam;

	private float m_lightBeamBaseIntensity;

	[SerializeField]
	private LensFlareComponentSRP m_lensFlare;

	private float m_lensFlareBaseIntensity;

	[SerializeField]
	private float m_enemyPresenceThreshold;

	[SerializeField]
	private float m_flickerMoveSpeed;

	[SerializeField]
	private float m_flickerThreshold;

	[SerializeField]
	private float m_flickerMinimumValue = 0.25f;

	private float m_flickerTarget;

	private float m_currentFlickerValue;

	private float m_smoothedEnemyPresence;

	[SerializeField]
	private float m_enemyPresenceSmoothSpeed;

	[SerializeField]
	private ActiveItemListener m_activeItemListener;

	[Header("Battery Charge")]
	[SerializeField]
	private float m_minLightScale = 0.2f;

	[SerializeField]
	private float m_startDipChargePoint = 25f;

	[SerializeField]
	private float m_batteryDipFlickerAmountMax = 0.25f;

	[Header("Spotlight")]
	[SerializeField]
	private Light m_spotlight;

	[SerializeField]
	private float m_spotlightIntensityScalarCloseToWall;

	[SerializeField]
	private AnimationCurve m_spotlightDistanceCurve;

	[Header("Environment Settings")]
	[SerializeField]
	private EnvironmentLightingSettings m_interiorLightingSettings;

	[SerializeField]
	private EnvironmentLightingSettings m_exteriorLightingSettings;

	private EnvironmentLightingSettings m_activeEnvironmentLightingSettings;

	[Header("Player Light Tyype")]
	[SerializeField]
	private PlayerLightType m_lightType;

	[SerializeField]
	private List<FlashlightModeSettings> m_settings = new List<FlashlightModeSettings>();

	private FlashlightPosition m_position;

	private FlashlightPosition m_previousPosition;

	private float m_lerpTimeTotal;

	private float m_lerpTimer;

	private float m_turnOnFlickerScalar = 1f;

	private float m_turnOnFlickerTimer = 1f;

	[SerializeField]
	private AnimationCurve m_turnOnAnimationCurve;

	[SerializeField]
	private float m_turnOnAnimTime;

	[Header("Power Off Event")]
	[SerializeField]
	private float m_powerOffEventBatteryLevel;

	[SerializeField]
	private float m_powerOffEventCheckTickTime;

	[SerializeField]
	private float m_powerOffEventTriggerChance;

	[SerializeField]
	private float m_powerOffEventPowerDuration;

	[SerializeField]
	private AnimationCurve m_powerOffRandomDurationCurve;

	[SerializeField]
	private AudioEvent m_powerOffAudioEvent;

	private float m_powerEventScalar;

	public void TurnOnFlicker()
	{
		m_turnOnFlickerScalar = 0f;
		m_turnOnFlickerTimer = 0f;
	}

	public void SetFlashlightPosition(FlashlightPosition position, float lerpTime)
	{
		if (m_position != position)
		{
			if (m_previousPosition == m_position)
			{
				m_previousPosition = m_position;
			}
			m_position = position;
			m_lerpTimer = 0f;
			m_lerpTimeTotal = lerpTime;
		}
	}

	private void ApplyEnvironmentLightingSettings(EnvironmentLightingSettings settings)
	{
		m_activeEnvironmentLightingSettings = settings;
		m_spotlight.intensity = settings.m_spotlightIntensity;
		m_spotlight.range = settings.m_spotlightRange;
		m_spotlight.innerSpotAngle = settings.m_spotlightInnerAngle;
	}

	private void Start()
	{
		if (GlobalReferences.Instance.Anchors.Generic.ActiveLevelMetadata.Item != null)
		{
			switch (GlobalReferences.Instance.Anchors.Generic.ActiveLevelMetadata.Item.SpacialType)
			{
			case LevelMetadata.LevelSpacialType.Exterior:
				ApplyEnvironmentLightingSettings(m_exteriorLightingSettings);
				break;
			case LevelMetadata.LevelSpacialType.Interior:
				ApplyEnvironmentLightingSettings(m_interiorLightingSettings);
				break;
			}
		}
		m_baseLightState = new Dictionary<Light, BaseLightState>();
		foreach (FlashlightModeSettings setting in m_settings)
		{
			for (int i = 0; i < setting.m_flashlightLightSettings.Count; i++)
			{
				Light light = setting.m_flashlightLightSettings[i].m_light;
				if (!m_baseLightState.ContainsKey(light))
				{
					m_baseLightState.Add(light, new BaseLightState
					{
						m_intensity = light.intensity,
						m_localPosition = light.transform.localPosition
					});
				}
			}
		}
		if (m_lensFlare != null)
		{
			m_lensFlareBaseIntensity = m_lensFlare.intensity;
		}
		m_lightBeamBaseIntensity = m_lightBeam.intensityGlobal;
	}

	private void OnEnable()
	{
		if (m_lightType == PlayerLightType.UVFlashlight)
		{
			Shader.SetGlobalInteger("_UVFlashlightEnabled", 1);
		}
		m_powerEventScalar = 1f;
		StartCoroutine(PowerOffEventLoop());
	}

	private void OnDisable()
	{
		StopAllCoroutines();
		if (m_lightType == PlayerLightType.UVFlashlight)
		{
			Shader.SetGlobalInteger("_UVFlashlightEnabled", 0);
		}
	}

	private float GetBatteryCharge()
	{
		float result = 100f;
		if (m_activeItemListener != null && m_activeItemListener.ActiveItemInstance != null && m_activeItemListener.ActiveItemInstance is PoweredItemInstance poweredItemInstance)
		{
			result = poweredItemInstance.ChargeAmount;
		}
		return result;
	}

	private IEnumerator PowerOffEventLoop()
	{
		while (true)
		{
			yield return new WaitForSeconds(m_powerOffEventCheckTickTime);
			if (!(GetBatteryCharge() < m_powerOffEventBatteryLevel) || !(UnityEngine.Random.Range(0f, 1f) < m_powerOffEventTriggerChance))
			{
				continue;
			}
			int randomAmountCycleAmount = UnityEngine.Random.Range(1, 4);
			for (int i = 0; i < randomAmountCycleAmount; i++)
			{
				if (m_powerOffAudioEvent != null)
				{
					m_powerOffAudioEvent.Play(base.transform.position);
				}
				float duration = m_powerOffRandomDurationCurve.Evaluate(UnityEngine.Random.Range(0f, 1f));
				yield return DOTween.To(() => m_powerEventScalar, delegate(float x)
				{
					m_powerEventScalar = x;
				}, 0f, 0.12f).WaitForCompletion();
				yield return new WaitForSeconds(duration);
				yield return DOTween.To(() => m_powerEventScalar, delegate(float x)
				{
					m_powerEventScalar = x;
				}, 1f, 0.14f).WaitForCompletion();
				yield return new WaitForSeconds(UnityEngine.Random.Range(0.05f, 0.1f));
			}
			m_powerEventScalar = 1f;
		}
	}

	public void Update()
	{
		if (m_lightType == PlayerLightType.UVFlashlight)
		{
			Shader.SetGlobalVector("_UVFlashlightPosition", base.transform.position);
			Shader.SetGlobalVector("_UVFlashlightForwardDir", base.transform.forward);
		}
		if (m_turnOnFlickerTimer < m_turnOnAnimTime)
		{
			m_turnOnFlickerTimer += Time.deltaTime;
			m_turnOnFlickerScalar = m_turnOnAnimationCurve.Evaluate(m_turnOnFlickerTimer / m_turnOnAnimTime);
		}
		else
		{
			m_turnOnFlickerScalar = 1f;
		}
		float batteryCharge = GetBatteryCharge();
		m_smoothedEnemyPresence = Mathf.MoveTowards(m_smoothedEnemyPresence, m_enemyPresence.Value, Time.deltaTime * m_enemyPresenceSmoothSpeed);
		float num = 0f;
		if (m_lightType == PlayerLightType.Lantern)
		{
			num = Mathf.Clamp(1f - batteryCharge, 0.2f, 0.8f);
		}
		else if (m_smoothedEnemyPresence > m_enemyPresenceThreshold)
		{
			num = m_smoothedEnemyPresence.Remap(m_enemyPresenceThreshold, 1f, 0f, 1f);
		}
		float num2 = 1f;
		if (batteryCharge < m_startDipChargePoint)
		{
			num2 = Mathf.Lerp(m_minLightScale, 1f, batteryCharge / m_startDipChargePoint);
			num = Mathf.Max(num, Mathf.Lerp(m_batteryDipFlickerAmountMax, 0f, batteryCharge / m_startDipChargePoint));
		}
		num2 *= m_powerEventScalar;
		if (num <= 0f)
		{
			UpdateLights(num2);
			return;
		}
		m_currentFlickerValue = Mathf.MoveTowards(m_currentFlickerValue, m_flickerTarget, m_flickerMoveSpeed * Time.deltaTime);
		if (Mathf.Abs(m_currentFlickerValue - m_flickerTarget) < 0.01f)
		{
			m_flickerTarget = UnityEngine.Random.Range(1f - num, 1f);
			m_flickerTarget = Mathf.Max(m_flickerMinimumValue, m_flickerTarget);
		}
		UpdateLights(((m_currentFlickerValue > m_flickerThreshold) ? 1f : m_currentFlickerValue) * num2 * m_turnOnFlickerScalar);
	}

	private void UpdateLights(float lightScale)
	{
		FlashlightModeSettings flashlightModeSettings = null;
		FlashlightModeSettings flashlightModeSettings2 = null;
		foreach (FlashlightModeSettings setting in m_settings)
		{
			if (setting.m_mode == m_position)
			{
				flashlightModeSettings = setting;
			}
			if (setting.m_mode == m_previousPosition)
			{
				flashlightModeSettings2 = setting;
			}
		}
		float t = 1f;
		if (flashlightModeSettings != flashlightModeSettings2)
		{
			t = ((!(m_lerpTimeTotal <= float.Epsilon)) ? (m_lerpTimer / m_lerpTimeTotal) : 1f);
			base.transform.localPosition = Vector3.Lerp(flashlightModeSettings2.m_offset, flashlightModeSettings.m_offset, t);
			base.transform.localRotation = Quaternion.Lerp(Quaternion.Euler(flashlightModeSettings2.m_rotation), Quaternion.Euler(flashlightModeSettings.m_rotation), t);
			m_lerpTimer += Time.deltaTime;
			if (m_lerpTimer >= m_lerpTimeTotal)
			{
				m_previousPosition = m_position;
			}
		}
		else
		{
			base.transform.localPosition = flashlightModeSettings.m_offset;
			base.transform.localRotation = Quaternion.Euler(flashlightModeSettings.m_rotation);
		}
		float t2 = 0f;
		if (m_spotlight != null)
		{
			float time = 10f;
			RaycastHit2D raycastHit2D = Physics2D.Raycast(base.transform.position, base.transform.forward, 10f, GameLayers.EnvironmentMask);
			if (raycastHit2D.collider != null)
			{
				time = raycastHit2D.distance;
			}
			t2 = m_spotlightDistanceCurve.Evaluate(time);
		}
		foreach (FlashlightLightSettings flashlightLightSetting in flashlightModeSettings.m_flashlightLightSettings)
		{
			float num = lightScale;
			if (flashlightLightSetting.m_light == m_spotlight)
			{
				num *= Mathf.Lerp(1f, m_spotlightIntensityScalarCloseToWall, t2);
			}
			bool flag = false;
			foreach (FlashlightLightSettings flashlightLightSetting2 in flashlightModeSettings2.m_flashlightLightSettings)
			{
				if (flashlightLightSetting2.m_light == flashlightLightSetting.m_light)
				{
					flag = true;
					flashlightLightSetting.m_light.intensity = m_baseLightState[flashlightLightSetting.m_light].m_intensity * num * Mathf.Lerp(flashlightLightSetting2.m_intensityScalar, flashlightLightSetting.m_intensityScalar, t);
				}
			}
			if (!flag)
			{
				flashlightLightSetting.m_light.intensity = m_baseLightState[flashlightLightSetting.m_light].m_intensity * num * flashlightLightSetting.m_intensityScalar;
			}
			flashlightLightSetting.m_light.transform.localPosition = m_baseLightState[flashlightLightSetting.m_light].m_localPosition + flashlightLightSetting.m_offset;
		}
		if (m_lensFlare != null)
		{
			float num2 = Mathf.Lerp(flashlightModeSettings2.m_lensFlareIntensityScalar, flashlightModeSettings.m_lensFlareIntensityScalar, t);
			m_lensFlare.intensity = m_lensFlareBaseIntensity * lightScale * num2;
		}
		if (m_lightBeam != null)
		{
			float num3 = Mathf.Lerp(flashlightModeSettings2.m_lightBeamIntensityScalar, flashlightModeSettings.m_lightBeamIntensityScalar, t);
			m_lightBeam.intensityGlobal = m_lightBeamBaseIntensity * lightScale * num3;
		}
		if (m_spotlight != null)
		{
			m_spotlight.spotAngle = Mathf.Lerp(m_activeEnvironmentLightingSettings.m_spotlightOuterAngleMin, m_activeEnvironmentLightingSettings.m_spotlightOuterAngleMax, t2);
		}
	}
}
