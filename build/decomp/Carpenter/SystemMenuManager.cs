using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SystemMenuManager : MonoBehaviour, IInputBackHandler
{
	[SerializeField]
	private MenuPage m_activePage;

	[SerializeField]
	private MenuPage m_frontPage;

	[SerializeField]
	private MenuPageEventChannel m_requestPageEventChannel;

	[SerializeField]
	private NewGameMenuPage m_newGameMenuPage;

	[SerializeField]
	private GameObject m_toggleCanvas;

	[SerializeField]
	private GameObject m_newGameButton;

	[SerializeField]
	private GameObject m_resumeButton;

	[SerializeField]
	private GameObject m_hintsButton;

	[SerializeField]
	private GameObject m_switchProfileButton;

	[SerializeField]
	private GameObject m_reloadPreviousGame;

	[SerializeField]
	private GameObject m_chapterSelectButton;

	[SerializeField]
	private GameObject m_quitGameButton;

	[SerializeField]
	private AudioEvent m_backAudioEvent;

	[Header("Input Prompts")]
	[SerializeField]
	private MenuInputPrompts m_menuInputPrompts;

	[Header("Flash & Fade")]
	[SerializeField]
	private FullscreenMenuRedFlash m_flash;

	[SerializeField]
	private MenuFade m_fade;

	[SerializeField]
	private CanvasGroup m_canvasGroup;

	[Header("Build Info")]
	[SerializeField]
	private TextMeshProUGUI m_buildInfo;

	private Stack<MenuPage> m_menuStack;

	private bool m_isLoading;

	private bool m_menuActive;

	private bool m_transitionInProgress;

	private bool m_levelTransitionActive;

	private float m_lastInputTime;

	private void Awake()
	{
		m_toggleCanvas.SetActive(value: false);
		m_menuStack = new Stack<MenuPage>();
		m_menuStack.Push(m_activePage);
		if (m_buildInfo != null)
		{
			m_buildInfo.text = BuildInfo.VerboseString;
		}
	}

	private void OnEnable()
	{
		if (GlobalReferences.Instance.GameMenuState.IsInMenu(GameMenuState.GameMenu.SystemMenu))
		{
			ShowSystemMenu();
		}
		GameMenuState gameMenuState = GlobalReferences.Instance.GameMenuState;
		gameMenuState.OnGameMenuChanged = (UnityAction<GameMenuState.GameMenu>)Delegate.Combine(gameMenuState.OnGameMenuChanged, new UnityAction<GameMenuState.GameMenu>(OnGameMenuChanged));
		GlobalReferences.Instance.EventChannels.LevelTransition.LevelTransitionStarted.Register(OnLevelTransitionStarted);
		GlobalReferences.Instance.EventChannels.LevelTransition.LevelTransitionCompleted.Register(OnLevelTransitionCompleted);
		GlobalReferences.Instance.EventChannels.MainMenu.ChapterSelect.Register(OnChapterSelect);
		if (GlobalReferences.Instance.GameMenuState.IsInMenu(GameMenuState.GameMenu.SystemMenu) && m_flash != null)
		{
			m_flash.Flash();
		}
		GlobalReferences.Instance.EventChannels.Audio.PauseGameAudioBusses.Raise(value: false);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.GameMenuState.ClearInMenu(GameMenuState.GameMenu.SystemMenu);
		GameMenuState gameMenuState = GlobalReferences.Instance.GameMenuState;
		gameMenuState.OnGameMenuChanged = (UnityAction<GameMenuState.GameMenu>)Delegate.Remove(gameMenuState.OnGameMenuChanged, new UnityAction<GameMenuState.GameMenu>(OnGameMenuChanged));
		GlobalReferences.Instance.EventChannels.LevelTransition.LevelTransitionStarted.Unregister(OnLevelTransitionStarted);
		GlobalReferences.Instance.EventChannels.LevelTransition.LevelTransitionCompleted.Unregister(OnLevelTransitionCompleted);
		GlobalReferences.Instance.EventChannels.MainMenu.ChapterSelect.Unregister(OnChapterSelect);
	}

	private void OnDestroy()
	{
		if (GlobalReferences.Instance.GameMenuState != null)
		{
			GameMenuState gameMenuState = GlobalReferences.Instance.GameMenuState;
			gameMenuState.OnGameMenuChanged = (UnityAction<GameMenuState.GameMenu>)Delegate.Remove(gameMenuState.OnGameMenuChanged, new UnityAction<GameMenuState.GameMenu>(OnGameMenuChanged));
		}
		m_requestPageEventChannel.Unregister(OnRequestPage);
	}

	private void BackAction()
	{
		if (m_activePage.CanExitPage() && !m_activePage.OnBackInput() && !(Time.unscaledTime - m_lastInputTime < 0.1f) && !m_isLoading && !m_transitionInProgress)
		{
			m_lastInputTime = Time.unscaledTime;
			if (m_toggleCanvas.activeSelf && m_menuStack.Count > 1)
			{
				BackPage();
			}
			else
			{
				TryToggleMenu();
			}
		}
	}

	private void SystemMenuInput(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			if (!m_menuActive)
			{
				TryToggleMenu();
			}
			else
			{
				BackAction();
			}
		}
	}

	private void TryToggleMenu()
	{
		if (m_isLoading || m_levelTransitionActive || !GlobalReferences.Instance.GameState.IsInState(GameState.GameStateFlag.GameActive))
		{
			return;
		}
		if (m_toggleCanvas.activeSelf)
		{
			CloseSystemMenu();
		}
		else if (!GlobalReferences.Instance.GameMenuState.IsInAnyMenu())
		{
			LevelMetadataReference levelMetadataReference = UnityEngine.Object.FindAnyObjectByType<LevelMetadataReference>();
			if (!(levelMetadataReference != null) || !levelMetadataReference.Metadata.IsCutscene)
			{
				ShowSystemMenu();
			}
		}
	}

	private void OnGameMenuChanged(GameMenuState.GameMenu menu)
	{
		if (menu.HasFlag(GameMenuState.GameMenu.SystemMenu))
		{
			ShowSystemMenu();
		}
		else
		{
			CloseSystemMenu();
		}
	}

	private void ShowSystemMenu()
	{
		if (!m_menuActive)
		{
			bool flag = GlobalReferences.Instance.GameState.IsInState(GameState.GameStateFlag.GameActive);
			bool currentProfileHasSaveData = SaveDataManager.Instance.CurrentProfileHasSaveData;
			if (m_activePage != m_frontPage)
			{
				m_activePage.Hide();
				m_activePage = m_frontPage;
				m_frontPage.Show(instant: true);
				m_menuStack.Clear();
				m_menuStack.Push(m_frontPage);
			}
			Debug.Log($"Showing system menu. '{DateTime.UtcNow}']");
			bool flag2 = !flag && !currentProfileHasSaveData;
			m_newGameButton.SetActive(flag2);
			m_chapterSelectButton.SetActive(value: false);
			m_resumeButton.SetActive(!flag2);
			m_quitGameButton.SetActive(!flag);
			m_switchProfileButton.SetActive(!flag);
			m_reloadPreviousGame.SetActive(!flag2);
			m_activePage.SelectDefaultSelectable();
			m_menuActive = true;
			m_toggleCanvas.SetActive(value: true);
			m_requestPageEventChannel.Register(OnRequestPage);
			GlobalReferences.Instance.GameMenuState.SetInMenu(GameMenuState.GameMenu.SystemMenu);
			GameInputManager.PushBackInputHandler(this);
			m_menuInputPrompts.SetInputs(m_activePage.AvailableInputs);
		}
	}

	private void CloseSystemMenu()
	{
		if (m_menuActive)
		{
			m_menuActive = false;
			m_toggleCanvas.SetActive(value: false);
			m_requestPageEventChannel.Unregister(OnRequestPage);
			GlobalReferences.Instance.GameMenuState.ClearInMenu(GameMenuState.GameMenu.SystemMenu);
			GameInputManager.RemoveBackInputHandler(this);
		}
	}

	private void OnRequestPage(MenuPage newPage)
	{
		if (m_flash != null)
		{
			m_flash.Flash();
		}
		StartCoroutine(PageTransition(newPage));
		m_menuStack.Push(newPage);
	}

	public void BackPage()
	{
		if (m_menuStack.Count > 1)
		{
			if (m_backAudioEvent != null)
			{
				m_backAudioEvent.Play2D();
			}
			m_menuStack.Pop();
			MenuPage newPage = m_menuStack.Peek();
			StartCoroutine(PageTransition(newPage));
		}
	}

	private IEnumerator PageTransition(MenuPage newPage)
	{
		m_transitionInProgress = true;
		m_activePage.Hide();
		yield return new WaitForSecondsRealtime(m_activePage.AnimTime);
		m_activePage = newPage;
		newPage.Show();
		m_menuInputPrompts.SetInputs(m_activePage.AvailableInputs);
		m_transitionInProgress = false;
	}

	private void Start()
	{
		m_activePage.Show();
	}

	public void ButtonQuit()
	{
		Application.Quit();
	}

	public void SaveUserPreferences()
	{
		GlobalReferences.Instance.EventChannels.UserPreferences.SaveUserPreferences.Raise();
	}

	public void OnTryLoadSave(int index)
	{
		if (!m_isLoading)
		{
			StartCoroutine(LoadGameCoroutine(index));
		}
	}

	private IEnumerator LoadGameCoroutine(int index)
	{
		m_isLoading = true;
		GlobalReferences.Instance.EventChannels.LevelTransition.ShowLoadingScreen.Raise(value: true);
		GlobalReferences.Instance.GameMenuState.ClearInMenu(GameMenuState.GameMenu.SystemMenu);
		yield return LevelManager.LoadUtilityScenesAsync();
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		GlobalReferences.Instance.EventChannels.SaveLoad.GameLoad.Raise(index);
		GlobalReferences.Instance.EventChannels.LevelTransition.ShowLoadingScreen.Raise(value: false);
		GlobalReferences.Instance.GameState.SetStateFlag(GameState.GameStateFlag.GameActive);
		m_isLoading = false;
	}

	public void NewGame()
	{
		if (!m_isLoading)
		{
			StartCoroutine(NewGameCoroutine());
		}
	}

	private IEnumerator NewGameCoroutine()
	{
		m_isLoading = true;
		if (m_flash != null)
		{
			yield return m_flash.FlashCoroutine();
		}
		if (m_fade != null)
		{
			yield return m_fade.FadeCoroutine();
		}
		GlobalReferences.Instance.DataStore.SetStartingDifficulty(m_newGameMenuPage.SelectedDifficulty);
		GlobalReferences.Instance.EventChannels.LevelTransition.ShowLoadingScreen.Raise(value: true);
		m_newGameMenuPage.Hide(instant: true);
		yield return new WaitForSecondsRealtime(0.1f);
		yield return LevelManager.LoadUtilityScenesAsync();
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		GlobalReferences.Instance.EventChannels.LevelTransition.TriggerStartGameInScene.Raise();
		GlobalReferences.Instance.GameMenuState.ClearInMenu(GameMenuState.GameMenu.SystemMenu);
		GlobalReferences.Instance.GameState.SetStateFlag(GameState.GameStateFlag.GameActive);
		GlobalReferences.Instance.EventChannels.LevelTransition.ShowLoadingScreen.Raise(value: false);
		GlobalReferences.Instance.EventChannels.Audio.PauseMusic.Raise(value: true);
		m_isLoading = false;
		UnloadSystemMenu();
	}

	private IEnumerator LoadAndContinuePreviousGameCoroutine()
	{
		m_isLoading = true;
		if (m_flash != null)
		{
			yield return m_flash.FlashCoroutine();
		}
		yield return m_canvasGroup.DOFade(0f, 0.5f).SetUpdate(isIndependentUpdate: true).WaitForCompletion();
		GlobalReferences.Instance.EventChannels.LevelTransition.ShowLoadingScreen.Raise(value: true);
		yield return LevelManager.LoadUtilityScenesAsync();
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		GlobalReferences.Instance.GameState.SetStateFlag(GameState.GameStateFlag.GameActive);
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		GlobalReferences.Instance.EventChannels.LevelTransition.ShowLoadingScreen.Raise(value: false);
		GlobalReferences.Instance.EventChannels.LevelTransition.TriggerStartGameInScene.Raise();
		GlobalReferences.Instance.GameMenuState.ClearInMenu(GameMenuState.GameMenu.SystemMenu);
		m_isLoading = false;
		SaveDataManager.SetLastSaveTimeToNow();
		UnloadSystemMenu();
	}

	public void ResumeGame()
	{
		if (GlobalReferences.Instance.GameState.IsInState(GameState.GameStateFlag.GameActive))
		{
			CloseSystemMenu();
		}
		else
		{
			StartCoroutine(LoadAndContinuePreviousGameCoroutine());
		}
	}

	public bool OnInputBack()
	{
		BackAction();
		return false;
	}

	private void OnLevelTransitionStarted()
	{
		m_levelTransitionActive = true;
		if (m_toggleCanvas.activeSelf)
		{
			CloseSystemMenu();
		}
	}

	private void OnLevelTransitionCompleted()
	{
		m_levelTransitionActive = false;
	}

	private void UnloadSystemMenu()
	{
		SceneManager.UnloadSceneAsync(base.gameObject.scene);
	}

	private void OnChapterSelect(NewGameStartConfiguration newGameStart)
	{
		UnloadSystemMenu();
	}
}
