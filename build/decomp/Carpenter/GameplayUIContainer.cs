using System;
using UnityEngine;
using UnityEngine.Events;

public class GameplayUIContainer : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup m_canvasGroup;

	private static bool m_uiDisabled;

	[DebugCommand("ui_disabled", "Disables in-game UI visibility", "ui_disabled <true/false>", typeof(bool), false)]
	protected static bool UIDisabled
	{
		get
		{
			return m_uiDisabled;
		}
		set
		{
			if (m_uiDisabled != value)
			{
				GameplayUIContainer gameplayUIContainer = UnityEngine.Object.FindAnyObjectByType<GameplayUIContainer>();
				if (value)
				{
					m_uiDisabled = true;
				}
				else
				{
					m_uiDisabled = false;
				}
				if (gameplayUIContainer != null)
				{
					gameplayUIContainer.SetActive(!m_uiDisabled);
				}
			}
		}
	}

	private void OnEnable()
	{
		GameMenuState gameMenuState = GlobalReferences.Instance.GameMenuState;
		gameMenuState.OnGameMenuChanged = (UnityAction<GameMenuState.GameMenu>)Delegate.Combine(gameMenuState.OnGameMenuChanged, new UnityAction<GameMenuState.GameMenu>(OnGameMenuChanged));
		GameCutsceneManager.OnCutsceneActiveChanged = (UnityAction<bool>)Delegate.Combine(GameCutsceneManager.OnCutsceneActiveChanged, new UnityAction<bool>(OnCutsceneChanged));
	}

	private void OnDisable()
	{
		GameMenuState gameMenuState = GlobalReferences.Instance.GameMenuState;
		gameMenuState.OnGameMenuChanged = (UnityAction<GameMenuState.GameMenu>)Delegate.Remove(gameMenuState.OnGameMenuChanged, new UnityAction<GameMenuState.GameMenu>(OnGameMenuChanged));
		GameCutsceneManager.OnCutsceneActiveChanged = (UnityAction<bool>)Delegate.Remove(GameCutsceneManager.OnCutsceneActiveChanged, new UnityAction<bool>(OnCutsceneChanged));
	}

	private void OnCutsceneChanged(bool cutsceneActive)
	{
		Refresh(GlobalReferences.Instance.GameMenuState.ActiveMenuState, cutsceneActive);
	}

	private void OnGameMenuChanged(GameMenuState.GameMenu gameMenu)
	{
		Refresh(gameMenu, GameCutsceneManager.CutsceneActive);
	}

	private void Refresh(GameMenuState.GameMenu gameMenu, bool cutsceneActive)
	{
		bool flag = !gameMenu.HasFlag(GameMenuState.GameMenu.InGameMenu) && !gameMenu.HasFlag(GameMenuState.GameMenu.Minigame) && !gameMenu.HasFlag(GameMenuState.GameMenu.SystemMenu);
		SetActive(flag && !m_uiDisabled && !cutsceneActive);
	}

	private void SetActive(bool active)
	{
		m_canvasGroup.alpha = (active ? 1f : 0f);
	}

	private void Start()
	{
		SetActive(!m_uiDisabled);
	}
}
