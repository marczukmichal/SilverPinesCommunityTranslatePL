using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class TimeScaleManager : MonoBehaviour
{
	[SerializeField]
	private BoolGameEventChannel m_isPlayingVideoCutsceneEventChannel;

	[SerializeField]
	private FloatGameEventChannel m_setDebugTimeScaleEvent;

	[SerializeField]
	private TimeSlowEventChannel m_gameSleepEvent;

	[SerializeField]
	private GameMenuState m_gameMenuState;

	private bool m_isSleping;

	private float m_debugTimeScale = 1f;

	private float m_combatTimeSleepScale;

	private Coroutine m_activeCoroutine;

	[DebugCommand("impact_time_slow", "Enable/disable the impact time slow effect", "s_impact_time_slow <true/false>", typeof(bool), false)]
	private static bool s_impact_time_slow = true;

	private bool m_gamePaused;

	private bool m_cutscenePlaying;

	public float DebugTimeScale
	{
		get
		{
			return m_debugTimeScale;
		}
		set
		{
			m_debugTimeScale = value;
			UpdateTimeScale();
		}
	}

	public bool GamePaused
	{
		get
		{
			return m_gamePaused;
		}
		set
		{
			if (m_gamePaused != value)
			{
				m_gamePaused = value;
				UpdateTimeScale();
			}
		}
	}

	public bool CutscenePlaying
	{
		get
		{
			return m_cutscenePlaying;
		}
		set
		{
			m_cutscenePlaying = value;
			UpdateTimeScale();
		}
	}

	private void OnEnable()
	{
		m_setDebugTimeScaleEvent.Register(SetDebugTimeScale);
		m_gameSleepEvent.Register(StartGameSleepEvent);
		m_isPlayingVideoCutsceneEventChannel.Register(SetCutscenePlaying);
		GameMenuState gameMenuState = m_gameMenuState;
		gameMenuState.OnGameMenuChanged = (UnityAction<GameMenuState.GameMenu>)Delegate.Combine(gameMenuState.OnGameMenuChanged, new UnityAction<GameMenuState.GameMenu>(SetGamePaused));
	}

	private void OnDisable()
	{
		m_setDebugTimeScaleEvent.Unregister(SetDebugTimeScale);
		m_gameSleepEvent.Unregister(StartGameSleepEvent);
		m_isPlayingVideoCutsceneEventChannel.Unregister(SetCutscenePlaying);
		GameMenuState gameMenuState = m_gameMenuState;
		gameMenuState.OnGameMenuChanged = (UnityAction<GameMenuState.GameMenu>)Delegate.Remove(gameMenuState.OnGameMenuChanged, new UnityAction<GameMenuState.GameMenu>(SetGamePaused));
	}

	private void OnDestroy()
	{
		Time.timeScale = 1f;
	}

	private void SetGamePaused(GameMenuState.GameMenu menu)
	{
		bool gamePaused = false;
		if (menu.HasFlag(GameMenuState.GameMenu.GameOver) || menu.HasFlag(GameMenuState.GameMenu.GameNotesReader) || menu.HasFlag(GameMenuState.GameMenu.VideoCutscene) || menu.HasFlag(GameMenuState.GameMenu.Comic) || menu.HasFlag(GameMenuState.GameMenu.PauseHint) || menu.HasFlag(GameMenuState.GameMenu.GameSaving) || menu.HasFlag(GameMenuState.GameMenu.OptionsPage) || menu.HasFlag(GameMenuState.GameMenu.MenuHint))
		{
			gamePaused = true;
		}
		GamePaused = gamePaused;
	}

	private void SetDebugTimeScale(float timeScale)
	{
		DebugTimeScale *= timeScale;
	}

	private void SetCutscenePlaying(bool playing)
	{
		CutscenePlaying = playing;
	}

	private void UpdateTimeScale()
	{
		if (m_gamePaused)
		{
			Time.timeScale = 0f;
		}
		else if (m_isSleping)
		{
			Time.timeScale = 1f - m_combatTimeSleepScale;
		}
		else if (m_cutscenePlaying)
		{
			Time.timeScale = 0f;
		}
		else
		{
			Time.timeScale = m_debugTimeScale;
		}
	}

	private float GetSleepTime(TimeSlowType slowType)
	{
		return slowType switch
		{
			TimeSlowType.Projectile => 0f, 
			TimeSlowType.MeleeLight => 0.02f, 
			TimeSlowType.MeleeHeavy => 0.03f, 
			TimeSlowType.PlayerHit => 0.05f, 
			_ => 0.1f, 
		};
	}

	private float GetSleepRecoverySpeed(TimeSlowType slowType)
	{
		return slowType switch
		{
			TimeSlowType.Projectile => 100f, 
			TimeSlowType.MeleeLight => 6f, 
			TimeSlowType.MeleeHeavy => 6f, 
			TimeSlowType.PlayerHit => 5f, 
			_ => 10f, 
		};
	}

	private void StartGameSleepEvent(TimeSlowType slowType)
	{
		if (slowType != 0 && s_impact_time_slow)
		{
			if (m_isSleping && m_activeCoroutine != null)
			{
				StopCoroutine(m_activeCoroutine);
			}
			m_activeCoroutine = StartCoroutine(GameSleepCoroutine(GetSleepTime(slowType), GetSleepRecoverySpeed(slowType)));
		}
	}

	private IEnumerator GameSleepCoroutine(float time, float recoverySpeed)
	{
		m_isSleping = true;
		m_combatTimeSleepScale = 1f;
		UpdateTimeScale();
		yield return new WaitForSecondsRealtime(time);
		while (m_combatTimeSleepScale > 0f)
		{
			yield return new WaitForEndOfFrame();
			m_combatTimeSleepScale -= Time.unscaledDeltaTime * recoverySpeed;
			m_combatTimeSleepScale = Mathf.Clamp01(m_combatTimeSleepScale);
			UpdateTimeScale();
		}
		m_isSleping = false;
		UpdateTimeScale();
	}
}
