using System.Collections;
using DG.Tweening;
using RaindropFX;
using UnityEngine;
using UnityEngine.Rendering;

public class WeatherPostProcessing : MonoBehaviour
{
	[SerializeField]
	private Volume m_volume;

	[SerializeField]
	private Weather m_weather;

	[Header("Rain Amounts")]
	[SerializeField]
	private float m_baseDynamicRainDropSpawnChance = 0.5f;

	[SerializeField]
	private float m_baseStaticRanDropSpawnChance = 0.5f;

	[Header("Interior Fogging")]
	[SerializeField]
	private float m_fogIntensity = 0.1f;

	[SerializeField]
	private float m_fogFadeDelay = 1f;

	[SerializeField]
	private float m_fogFadeTime = 5f;

	[Header("Wind")]
	[SerializeField]
	private Vector2 m_weatherWindScalar = Vector2.one;

	[SerializeField]
	private float m_updateCameraVelocityRefreshTime = 1f;

	[SerializeField]
	private float m_cameraMovementWindScalar = 10f;

	[SerializeField]
	private float m_maxWindFromCameraMovement = 3f;

	private LevelMetadata m_activeLevelMetadata;

	private bool m_isOutDoors;

	private RaindropFX_URP m_rainDropFX;

	private Vector2 m_cameraVelocity;

	private Vector2 m_previousCameraPosition;

	private float m_cameraRefreshTimer;

	private PostProcessingLevel m_qualityLevel;

	private void Awake()
	{
		m_volume.profile.TryGet<RaindropFX_URP>(out m_rainDropFX);
	}

	private void OnEnable()
	{
		m_isOutDoors = false;
		GlobalReferences.Instance.Anchors.Generic.ActiveLevelMetadata.Register(OnLevelMetadataChanged);
		OnLevelMetadataChanged(GlobalReferences.Instance.Anchors.Generic.ActiveLevelMetadata.Item);
		GlobalReferences.Instance.EventChannels.UserPreferences.GraphicsQualityChanged.Register(GraphicsQualityChanged);
		GraphicsQualityChanged();
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.Anchors.Generic.ActiveLevelMetadata.Unregister(OnLevelMetadataChanged);
		GlobalReferences.Instance.EventChannels.UserPreferences.GraphicsQualityChanged.Unregister(GraphicsQualityChanged);
	}

	private void GraphicsQualityChanged()
	{
		m_qualityLevel = GlobalReferences.Instance.UserPreferences.GraphicsQualitySettings.CameraRain;
		if (m_qualityLevel == PostProcessingLevel.Off)
		{
			m_volume.enabled = false;
		}
		else
		{
			m_volume.enabled = true;
		}
	}

	private void OnLevelMetadataChanged(LevelMetadata newLevel)
	{
		if (newLevel == m_activeLevelMetadata || (newLevel != null && newLevel.SpacialType == LevelMetadata.LevelSpacialType.KeepPrevious))
		{
			return;
		}
		bool isOutDoors = m_isOutDoors;
		if (newLevel != null && newLevel.SpacialType == LevelMetadata.LevelSpacialType.Exterior)
		{
			m_isOutDoors = true;
		}
		else
		{
			m_isOutDoors = false;
		}
		m_activeLevelMetadata = newLevel;
		if (isOutDoors != m_isOutDoors)
		{
			StopAllCoroutines();
			if (m_qualityLevel != 0)
			{
				if (m_isOutDoors)
				{
					EnterExterior();
				}
				else if (m_weather.CurrentRainAmount > 0.25f)
				{
					StartCoroutine(EnterInterior());
				}
			}
		}
		m_previousCameraPosition = Vector2.zero;
		m_cameraRefreshTimer = 2f;
	}

	private IEnumerator EnterInterior()
	{
		m_rainDropFX.useFog.value = true;
		m_rainDropFX.fogIntensity.value = 0f;
		yield return DOTween.To(() => m_rainDropFX.fogIntensity.value, delegate(float x)
		{
			m_rainDropFX.fogIntensity.value = x;
		}, m_fogIntensity, 0.1f).WaitForCompletion();
		yield return new WaitForSeconds(m_fogFadeDelay);
		yield return DOTween.To(() => m_rainDropFX.fogIntensity.value, delegate(float x)
		{
			m_rainDropFX.fogIntensity.value = x;
		}, 0f, m_fogFadeTime).WaitForCompletion();
		m_rainDropFX.useFog.value = false;
	}

	private IEnumerator EnterExterior()
	{
		m_rainDropFX.useFog.value = false;
		yield return null;
	}

	private void Update()
	{
		if (m_qualityLevel != 0)
		{
			m_cameraRefreshTimer -= Time.deltaTime;
			if (m_cameraRefreshTimer <= 0f)
			{
				UpdateCameraPosition();
			}
			float num = 0f;
			if (m_isOutDoors)
			{
				num = m_weather.CurrentRainAmount;
			}
			m_rainDropFX.fastMode.value = m_qualityLevel == PostProcessingLevel.High;
			m_rainDropFX.refreshRate.value = ((m_qualityLevel != PostProcessingLevel.High) ? 1 : 0);
			if (m_qualityLevel == PostProcessingLevel.Low)
			{
				num *= 0.5f;
			}
			m_rainDropFX.chanceToSpawnStaticRaindrop.value = m_baseStaticRanDropSpawnChance * num;
			m_rainDropFX.chanceToSpawnDynamicRaindrop.value = m_baseDynamicRainDropSpawnChance * num;
			Vector2 zero = Vector2.zero;
			zero.x = m_weather.CurrentWindAmount * m_weatherWindScalar.x;
			zero.y = m_weather.CurrentWindAmount * m_weatherWindScalar.y * Mathf.Sin(Time.time);
			float value = 0f - m_cameraVelocity.x;
			value = Mathf.Clamp(value, 0f - m_maxWindFromCameraMovement, m_maxWindFromCameraMovement);
			zero.x += value * m_cameraMovementWindScalar;
			m_rainDropFX.wind.value = zero;
		}
	}

	private void UpdateCameraPosition()
	{
		Camera main = Camera.main;
		if (main != null)
		{
			Vector2 vector = main.transform.position;
			if (m_previousCameraPosition != Vector2.zero)
			{
				m_cameraVelocity = (vector - m_previousCameraPosition) / m_updateCameraVelocityRefreshTime;
			}
			m_previousCameraPosition = vector;
		}
		m_cameraRefreshTimer = m_updateCameraVelocityRefreshTime;
	}
}
