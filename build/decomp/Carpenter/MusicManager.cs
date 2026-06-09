using System;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.Events;

public class MusicManager : MonoBehaviour
{
	[SerializeField]
	private float m_combatFadeInRate = 0.5f;

	[SerializeField]
	private float m_combatFadeOutRate = 0.25f;

	[SerializeField]
	private AudioEvent m_suddenEnemyAudioEvent;

	[SerializeField]
	private MusicSettings m_menuMusic;

	private LevelMetadata m_activeLevelMetadata;

	private MusicSettings m_activeMusicSettings;

	private static bool m_musicDisabled;

	private bool m_pausedMusic;

	private bool m_isGameActive;

	private MusicSettings m_musicOverride;

	private float m_currentCombatIntensityBalance;

	private PARAMETER_ID m_combatIntensityParameterID;

	private EventInstance m_currentActiveEventInstance;

	private EventInstance m_currentCombatEventInstance;

	private EventInstance m_menuMusicEventInstance;

	[DebugCommand("music_debug", "Enable music debug", "music_debug <true/false>", typeof(bool), false)]
	private static bool m_musicDebug;

	private bool GameActive
	{
		get
		{
			return m_isGameActive;
		}
		set
		{
			if (m_isGameActive == value)
			{
				return;
			}
			if (value)
			{
				if (m_menuMusicEventInstance.isValid())
				{
					m_menuMusicEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
					m_menuMusicEventInstance.release();
				}
				OnLevelChanged(GlobalReferences.Instance.Anchors.Generic.ActiveLevelMetadata.Item);
			}
			else
			{
				PlayMusic(null);
			}
			m_isGameActive = value;
		}
	}

	[DebugCommand("music_disabled", "Disables music playback", "music_disabled <true/false>", typeof(bool), false)]
	protected static bool MusicDisabled
	{
		get
		{
			return m_musicDisabled;
		}
		set
		{
			MusicManager musicManager = UnityEngine.Object.FindAnyObjectByType<MusicManager>();
			if (value)
			{
				if (musicManager != null)
				{
					musicManager.PlayMusic(null);
				}
				m_musicDisabled = true;
			}
			else
			{
				m_musicDisabled = false;
				if (musicManager != null)
				{
					musicManager.PlayLevelMusic();
				}
			}
		}
	}

	private void OnEnable()
	{
		GlobalReferences.Instance.Anchors.Generic.ActiveLevelMetadata.Register(OnLevelChanged);
		GlobalReferences.Instance.EventChannels.Audio.PlayMusicOverride.Register(OnPlayOverrideMusic);
		GlobalReferences.Instance.EventChannels.Audio.PauseMusic.Register(OnPauseMusic);
		GlobalReferences.Instance.EventChannels.Gameplay.SuddenEnemyAppeared.Register(OnEnemySuddenlyAppeared);
		GlobalReferences.Instance.EventChannels.Audio.TriggerMainMenuMusic.Register(TriggerMainMenuMusic);
		GlobalReferences.Instance.EventChannels.Audio.TriggerMainMenuMusicTransition.Register(TriggerMainMenuMusicParameter);
		GameState gameState = GlobalReferences.Instance.GameState;
		gameState.OnGameStateFlagChanged = (UnityAction<GameState.GameStateFlag>)Delegate.Combine(gameState.OnGameStateFlagChanged, new UnityAction<GameState.GameStateFlag>(OnGameStateChanged));
		if (GlobalReferences.Instance.GameState.IsInState(GameState.GameStateFlag.GameActive))
		{
			GameActive = true;
		}
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.Anchors.Generic.ActiveLevelMetadata.Unregister(OnLevelChanged);
		GlobalReferences.Instance.EventChannels.Audio.PlayMusicOverride.Unregister(OnPlayOverrideMusic);
		GlobalReferences.Instance.EventChannels.Audio.PauseMusic.Unregister(OnPauseMusic);
		GlobalReferences.Instance.EventChannels.Gameplay.SuddenEnemyAppeared.Unregister(OnEnemySuddenlyAppeared);
		GlobalReferences.Instance.EventChannels.Audio.TriggerMainMenuMusic.Unregister(TriggerMainMenuMusic);
		GlobalReferences.Instance.EventChannels.Audio.TriggerMainMenuMusicTransition.Unregister(TriggerMainMenuMusicParameter);
		if (m_currentActiveEventInstance.isValid())
		{
			m_currentActiveEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			m_currentActiveEventInstance.release();
		}
		if (m_currentCombatEventInstance.isValid())
		{
			m_currentCombatEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			m_currentCombatEventInstance.release();
		}
		if (m_menuMusicEventInstance.isValid())
		{
			m_menuMusicEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			m_menuMusicEventInstance.release();
		}
		GameState gameState = GlobalReferences.Instance.GameState;
		gameState.OnGameStateFlagChanged = (UnityAction<GameState.GameStateFlag>)Delegate.Remove(gameState.OnGameStateFlagChanged, new UnityAction<GameState.GameStateFlag>(OnGameStateChanged));
	}

	private void TriggerMainMenuMusic()
	{
		PlayMusic(null);
		if (!m_menuMusicEventInstance.isValid())
		{
			m_menuMusicEventInstance = RuntimeManager.CreateInstance(m_menuMusic.StartFMODEvent);
			m_menuMusicEventInstance.start();
		}
	}

	private void PlayLevelMusic()
	{
		if (GlobalReferences.Instance.GameState.IsInState(GameState.GameStateFlag.GameActive) && m_activeLevelMetadata != null)
		{
			PlayMusic(m_activeLevelMetadata.MusicSettings);
		}
	}

	private void OnLevelChanged(LevelMetadata level)
	{
		m_activeLevelMetadata = level;
		if (m_musicOverride == null)
		{
			PlayLevelMusic();
		}
	}

	private void OnPauseMusic(bool paused)
	{
		if (m_pausedMusic != paused)
		{
			m_pausedMusic = paused;
			if (m_currentActiveEventInstance.isValid())
			{
				m_currentActiveEventInstance.setPaused(paused);
			}
			if (m_currentCombatEventInstance.isValid())
			{
				m_currentCombatEventInstance.setPaused(paused);
			}
		}
	}

	private void OnPlayOverrideMusic(MusicSettings musicClip)
	{
		m_musicOverride = musicClip;
		if (musicClip != null)
		{
			PlayMusic(musicClip);
		}
		else
		{
			PlayLevelMusic();
		}
	}

	private void PlayMusic(MusicSettings levelMusicSettings)
	{
		if (m_musicDisabled || !(m_activeMusicSettings != levelMusicSettings))
		{
			return;
		}
		if (m_activeMusicSettings != null)
		{
			if (m_currentActiveEventInstance.isValid())
			{
				m_currentActiveEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
				m_currentActiveEventInstance.release();
			}
			if (m_currentCombatEventInstance.isValid())
			{
				m_currentCombatEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
				m_currentCombatEventInstance.release();
			}
		}
		m_activeMusicSettings = levelMusicSettings;
		if (m_activeMusicSettings != null && !m_activeMusicSettings.StartFMODEvent.IsNull)
		{
			m_currentActiveEventInstance = RuntimeManager.CreateInstance(m_activeMusicSettings.StartFMODEvent);
			m_currentActiveEventInstance.start();
			if (m_pausedMusic)
			{
				m_currentActiveEventInstance.setPaused(paused: true);
			}
		}
	}

	private void OnGameStateChanged(GameState.GameStateFlag flag)
	{
		GameActive = flag.HasFlag(GameState.GameStateFlag.GameActive);
	}

	private void Update()
	{
		if (m_activeMusicSettings == null)
		{
			return;
		}
		float num = 0f;
		if (!m_activeMusicSettings.ForceNoCombatMusic)
		{
			num = GlobalReferences.Instance.Variables.Generic.NearbyEnemyPresence.Value;
		}
		float num2 = ((num > m_currentCombatIntensityBalance) ? m_combatFadeInRate : m_combatFadeOutRate);
		m_currentCombatIntensityBalance = Mathf.MoveTowards(m_currentCombatIntensityBalance, num, Time.deltaTime * num2);
		if (m_currentCombatIntensityBalance > 0f && !m_currentCombatEventInstance.isValid() && !m_activeMusicSettings.CombatFMODEvent.IsNull)
		{
			m_currentCombatEventInstance = RuntimeManager.CreateInstance(m_activeMusicSettings.CombatFMODEvent);
			m_currentCombatEventInstance.start();
			m_currentCombatEventInstance.getDescription(out var description);
			description.getParameterDescriptionByName("CombatIntensity", out var parameter);
			m_combatIntensityParameterID = parameter.id;
			if (m_pausedMusic)
			{
				m_currentCombatEventInstance.setPaused(paused: true);
			}
		}
		else if (m_currentCombatIntensityBalance <= 0f && m_currentCombatEventInstance.isValid())
		{
			m_currentCombatEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			m_currentCombatEventInstance.release();
		}
		if (m_currentCombatEventInstance.isValid())
		{
			m_currentCombatEventInstance.setParameterByID(m_combatIntensityParameterID, m_currentCombatIntensityBalance);
		}
		if (m_currentActiveEventInstance.isValid())
		{
			float target = ((m_currentCombatIntensityBalance > 0f) ? 0f : 1f);
			m_currentActiveEventInstance.getVolume(out var volume);
			float volume2 = Mathf.MoveTowards(volume, target, Time.unscaledDeltaTime * 0.5f);
			m_currentActiveEventInstance.setVolume(volume2);
		}
	}

	private void OnEnemySuddenlyAppeared()
	{
		if (m_suddenEnemyAudioEvent != null)
		{
			m_suddenEnemyAudioEvent.Play2D();
		}
	}

	private void TriggerMainMenuMusicParameter()
	{
		if (m_menuMusicEventInstance.isValid())
		{
			m_menuMusicEventInstance.setParameterByName("MenuTransition", 1f);
		}
	}
}
