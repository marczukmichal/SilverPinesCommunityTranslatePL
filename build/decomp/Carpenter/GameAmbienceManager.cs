using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class GameAmbienceManager : MonoBehaviour
{
	[SerializeField]
	private EventReference m_nearbyEnemyPresenceEvent;

	[SerializeField]
	private EventReference m_weatherAmbienceEvent;

	[SerializeField]
	private float m_weatherInsideOutTransitionSpeed;

	[SerializeField]
	private Weather m_weather;

	private EventInstance m_nearbyEnemyEventInstance;

	private float m_targetEnemyPresence;

	private float m_enemyPresenceAmount;

	private EventInstance m_weatherAmbienceInstance;

	private PARAMETER_ID m_weatherIntensityParemeterID;

	private PARAMETER_ID m_rainIntensityParemeterID;

	private PARAMETER_ID m_windIntensityParemeterID;

	private PARAMETER_ID m_weatherInsideOrOutsideParameterID;

	private float m_targetAmbienceInternalValue;

	private float m_currentAmbienceInternalValue;

	private LevelMetadata m_activeLevelMetadata;

	private LevelAmbienceSettings m_activeAmbienceSettings;

	private EventInstance m_activeAmbienceEventInstance;

	private LevelReverbSettings m_activeReverbSettings;

	private EventInstance m_activeReverbEventInstance;

	private void OnEnable()
	{
		m_nearbyEnemyEventInstance = RuntimeManager.CreateInstance(m_nearbyEnemyPresenceEvent);
		m_nearbyEnemyEventInstance.setVolume(0f);
		m_nearbyEnemyEventInstance.start();
		GlobalReferences.Instance.EventChannels.Generic.NearbyEnemyPresenceChanged.Register(OnEnemyPresenceChanged);
		m_weatherAmbienceInstance = RuntimeManager.CreateInstance(m_weatherAmbienceEvent);
		m_weatherAmbienceInstance.setVolume(0f);
		m_weatherAmbienceInstance.start();
		m_weatherAmbienceInstance.getDescription(out var description);
		RuntimeManager.StudioSystem.getParameterDescriptionByName("WeatherIntensity", out var parameter);
		m_weatherIntensityParemeterID = parameter.id;
		RuntimeManager.StudioSystem.getParameterDescriptionByName("RainIntensity", out var parameter2);
		m_rainIntensityParemeterID = parameter2.id;
		RuntimeManager.StudioSystem.getParameterDescriptionByName("WindIntensity", out var parameter3);
		m_windIntensityParemeterID = parameter3.id;
		description.getParameterDescriptionByName("WeatherInsideorOutside", out var parameter4);
		m_weatherInsideOrOutsideParameterID = parameter4.id;
		m_weatherAmbienceInstance.setParameterByID(m_weatherInsideOrOutsideParameterID, 0f);
		GlobalReferences.Instance.Anchors.Generic.ActiveLevelMetadata.Register(OnLevelMetadataChanged);
		OnLevelMetadataChanged(GlobalReferences.Instance.Anchors.Generic.ActiveLevelMetadata.Item);
		m_currentAmbienceInternalValue = m_targetAmbienceInternalValue;
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.Generic.NearbyEnemyPresenceChanged.Unregister(OnEnemyPresenceChanged);
		GlobalReferences.Instance.Anchors.Generic.ActiveLevelMetadata.Unregister(OnLevelMetadataChanged);
		if (m_activeReverbEventInstance.isValid())
		{
			m_activeReverbEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			m_activeReverbEventInstance.release();
		}
		if (m_nearbyEnemyEventInstance.isValid())
		{
			m_nearbyEnemyEventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
			m_nearbyEnemyEventInstance.release();
		}
		if (m_weatherAmbienceInstance.isValid())
		{
			m_weatherAmbienceInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
			m_weatherAmbienceInstance.release();
		}
		if (m_activeAmbienceEventInstance.isValid())
		{
			m_activeAmbienceEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			m_activeAmbienceEventInstance.release();
		}
	}

	private void OnLevelMetadataChanged(LevelMetadata newLevel)
	{
		if (newLevel != null)
		{
			if (newLevel.SpacialType == LevelMetadata.LevelSpacialType.KeepPrevious)
			{
				return;
			}
			m_weatherAmbienceInstance.setVolume(1f);
			m_targetAmbienceInternalValue = ((newLevel.SpacialType == LevelMetadata.LevelSpacialType.Interior) ? 1f : 0f);
		}
		else
		{
			m_targetAmbienceInternalValue = 1f;
		}
		if (m_activeLevelMetadata == null)
		{
			m_currentAmbienceInternalValue = m_targetAmbienceInternalValue;
		}
		LevelAmbienceSettings levelAmbienceSettings = null;
		LevelReverbSettings levelReverbSettings = null;
		if (newLevel != null)
		{
			levelAmbienceSettings = newLevel.AmbienceSettings;
			levelReverbSettings = newLevel.ReverbSettings;
		}
		if (levelAmbienceSettings != m_activeAmbienceSettings)
		{
			if (m_activeAmbienceEventInstance.isValid())
			{
				m_activeAmbienceEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
				m_activeAmbienceEventInstance.release();
			}
			m_activeAmbienceSettings = levelAmbienceSettings;
			if (m_activeAmbienceSettings != null && !m_activeAmbienceSettings.AmbienceFMODEvent.IsNull)
			{
				m_activeAmbienceEventInstance = RuntimeManager.CreateInstance(m_activeAmbienceSettings.AmbienceFMODEvent);
				m_activeAmbienceEventInstance.start();
			}
		}
		if (levelReverbSettings != m_activeReverbSettings)
		{
			if (m_activeReverbEventInstance.isValid())
			{
				m_activeReverbEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
				m_activeReverbEventInstance.release();
			}
			m_activeReverbSettings = levelReverbSettings;
			if (m_activeReverbSettings != null && !m_activeReverbSettings.ReverbFMODEvent.IsNull)
			{
				m_activeReverbEventInstance = RuntimeManager.CreateInstance(m_activeReverbSettings.ReverbFMODEvent);
				m_activeReverbEventInstance.start();
			}
		}
		m_activeLevelMetadata = newLevel;
	}

	private void Update()
	{
		m_enemyPresenceAmount = Mathf.MoveTowards(m_enemyPresenceAmount, m_targetEnemyPresence, Time.deltaTime);
		SetNearbyEnemyPresenceVolume(m_enemyPresenceAmount);
		m_currentAmbienceInternalValue = Mathf.MoveTowards(m_currentAmbienceInternalValue, m_targetAmbienceInternalValue, Time.deltaTime * m_weatherInsideOutTransitionSpeed);
		RuntimeManager.StudioSystem.setParameterByID(m_weatherIntensityParemeterID, m_weather.CurrentRainAmount);
		RuntimeManager.StudioSystem.setParameterByID(m_rainIntensityParemeterID, m_weather.CurrentRainAmount);
		RuntimeManager.StudioSystem.setParameterByID(m_windIntensityParemeterID, m_weather.CurrentWindAmount);
		m_weatherAmbienceInstance.setParameterByID(m_weatherInsideOrOutsideParameterID, m_currentAmbienceInternalValue);
	}

	private void SetNearbyEnemyPresenceVolume(float volume)
	{
		if (m_nearbyEnemyEventInstance.isValid())
		{
			m_nearbyEnemyEventInstance.setVolume(volume);
		}
	}

	private void OnEnemyPresenceChanged(float presence)
	{
		m_targetEnemyPresence = Mathf.Clamp01(presence);
	}
}
