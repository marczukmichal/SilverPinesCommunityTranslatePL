using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverMenu : MonoBehaviour
{
	[SerializeField]
	private VoidGameEventChannel m_playerDeadEventChannel;

	[Header("UI")]
	[SerializeField]
	private GameObject m_container;

	[SerializeField]
	private Image m_background;

	[SerializeField]
	private Image m_line;

	[SerializeField]
	private GameObject m_content;

	[SerializeField]
	private GameObject m_selectOnOpen;

	[SerializeField]
	private GameObject m_debugRespawnOption;

	[Header("Animation Timings")]
	[SerializeField]
	private float m_initialShowDelay;

	[Header("Audio")]
	[SerializeField]
	private AudioEvent m_typeAudio;

	private Color m_baseBackgroundColor;

	private void OnEnable()
	{
		GlobalReferences.Instance.EventChannels.Player.PlayerDead.Register(Show);
		GlobalReferences.Instance.EventChannels.SaveLoad.OnGameReloadedEvent.Register(Hide);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.Player.PlayerDead.Unregister(Show);
		GlobalReferences.Instance.EventChannels.SaveLoad.OnGameReloadedEvent.Unregister(Hide);
	}

	private void Start()
	{
		m_baseBackgroundColor = m_background.color;
		Hide();
		if (!Application.isEditor && m_debugRespawnOption != null)
		{
			UnityEngine.Object.Destroy(m_debugRespawnOption);
		}
	}

	private void OnDestroy()
	{
		GameMenuState gameMenuState = GlobalReferences.Instance.GameMenuState;
		gameMenuState.OnGameMenuChanged = (UnityAction<GameMenuState.GameMenu>)Delegate.Remove(gameMenuState.OnGameMenuChanged, new UnityAction<GameMenuState.GameMenu>(GameMenuChanged));
		GlobalReferences.Instance.GameMenuState.ClearInMenu(GameMenuState.GameMenu.GameOver);
	}

	private void Show()
	{
		GameMenuState gameMenuState = GlobalReferences.Instance.GameMenuState;
		gameMenuState.OnGameMenuChanged = (UnityAction<GameMenuState.GameMenu>)Delegate.Combine(gameMenuState.OnGameMenuChanged, new UnityAction<GameMenuState.GameMenu>(GameMenuChanged));
		StartCoroutine(PlayerDeadCoroutine());
	}

	private void Hide()
	{
		GameMenuState gameMenuState = GlobalReferences.Instance.GameMenuState;
		gameMenuState.OnGameMenuChanged = (UnityAction<GameMenuState.GameMenu>)Delegate.Remove(gameMenuState.OnGameMenuChanged, new UnityAction<GameMenuState.GameMenu>(GameMenuChanged));
		GlobalReferences.Instance.GameMenuState.ClearInMenu(GameMenuState.GameMenu.GameOver);
		m_container.SetActive(value: false);
		m_line.gameObject.SetActive(value: false);
	}

	private void GameMenuChanged(GameMenuState.GameMenu gameMenu)
	{
		bool flag = gameMenu.HasFlag(GameMenuState.GameMenu.GameSaving);
		m_content.SetActive(!flag);
		if (!flag)
		{
			UpdateEventSystemSelection();
		}
	}

	private IEnumerator PlayerDeadCoroutine()
	{
		yield return new WaitForSeconds(m_initialShowDelay);
		GlobalReferences.Instance.GameMenuState.SetInMenu(GameMenuState.GameMenu.GameOver);
		UpdateEventSystemSelection();
		m_container.SetActive(value: true);
		m_content.SetActive(value: true);
		Color baseBackgroundColor = m_baseBackgroundColor;
		baseBackgroundColor.a = 0f;
		m_background.color = baseBackgroundColor;
		yield return m_background.DOFade(m_baseBackgroundColor.a, 1f).SetUpdate(isIndependentUpdate: true);
		m_line.gameObject.SetActive(value: true);
		m_line.rectTransform.localScale = new Vector3(0f, 1f, 1f);
		m_line.DOFade(1f, 0f);
	}

	public void LoadSaveButtonPressed()
	{
		GlobalReferences.Instance.EventChannels.SaveLoad.LoadMostRecentSave.Raise();
	}

	public void DebugRespawnButtonPressed()
	{
		GameDebugCommands gameDebugCommands = UnityEngine.Object.FindAnyObjectByType<GameDebugCommands>();
		if (gameDebugCommands != null)
		{
			gameDebugCommands.Respawn();
		}
	}

	public void ReturnToMenuButtonPressed()
	{
		GlobalReferences.Instance.GameState.ClearStateFlag(GameState.GameStateFlag.GameActive);
		SceneManager.LoadScene("Assets/Scenes/Utility/Startup.unity");
	}

	private void UpdateEventSystemSelection()
	{
		EventSystem.current?.SetSelectedGameObject(m_selectOnOpen);
	}
}
