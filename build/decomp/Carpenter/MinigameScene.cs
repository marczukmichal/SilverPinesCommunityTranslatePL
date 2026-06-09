using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MinigameScene : MonoBehaviour, IInputBackHandler
{
	[SerializeField]
	private MinigameMetadata m_minigameMetadata;

	[Header("UI")]
	[SerializeField]
	private GameObject m_foregroundOverlay;

	[SerializeField]
	private CanvasGroup m_canvasGroup;

	[SerializeField]
	private CanvasGroup m_uiCanvasGroup;

	[Header("Close Delay")]
	[SerializeField]
	private float m_minigameCloseDelayOnSuccess = -1f;

	[Header("Photograph")]
	[SerializeField]
	private ItemDefinition m_cameraItemDefinition;

	[SerializeField]
	private MinigamePhotoController m_photoController;

	[SerializeField]
	private GameObject m_cameraPrompt;

	public UnityAction m_onMinigameComplete;

	public UnityAction m_onMinigameClose;

	private int m_customOutputValue;

	private bool m_minigameCompleted;

	private Coroutine m_closingCoroutine;

	private bool m_isClosing;

	private IMinigameBackInputHandler[] m_minigameBackInputHandler;

	private bool m_takenPhotograph;

	public MinigameMetadata MinigameMetadata => m_minigameMetadata;

	public int CustomData => m_customOutputValue;

	private void Start()
	{
		m_minigameBackInputHandler = GetComponentsInChildren<IMinigameBackInputHandler>(includeInactive: true);
	}

	private void OnEnable()
	{
		GameInputManager.PushBackInputHandler(this);
		if (m_minigameMetadata.DoFullscreenFade)
		{
			GlobalReferences.Instance.EventChannels.Generic.ScreenFadeOfType.Raise(FadeType.None);
		}
		else
		{
			m_canvasGroup.alpha = 0f;
			m_canvasGroup.DOFade(1f, 0.5f).SetUpdate(isIndependentUpdate: true);
		}
		if (m_minigameMetadata.CanPhotograph)
		{
			GameInputManager.GameInputActions.UI.TakeMinigamePhoto.performed += TakePhotoInput;
		}
		GlobalReferences.Instance.GameMenuState.SetInMenu(GameMenuState.GameMenu.Minigame);
		GameMenuState gameMenuState = GlobalReferences.Instance.GameMenuState;
		gameMenuState.OnGameMenuChanged = (UnityAction<GameMenuState.GameMenu>)Delegate.Combine(gameMenuState.OnGameMenuChanged, new UnityAction<GameMenuState.GameMenu>(OnGameMenuChanged));
		GlobalReferences.Instance.EventChannels.Player.PlayerTakeDamage.Register(TryCloseMinigame);
		GlobalReferences.Instance.EventChannels.Player.PlayerDead.Register(TryCloseMinigame);
		m_cameraPrompt.SetActive(CanPhotograph());
	}

	private void OnDisable()
	{
		GameInputManager.RemoveBackInputHandler(this);
		if (GameInputManager.GameInputActions != null)
		{
			GameInputManager.GameInputActions.UI.TakeMinigamePhoto.performed -= TakePhotoInput;
		}
		GameMenuState gameMenuState = GlobalReferences.Instance.GameMenuState;
		gameMenuState.OnGameMenuChanged = (UnityAction<GameMenuState.GameMenu>)Delegate.Remove(gameMenuState.OnGameMenuChanged, new UnityAction<GameMenuState.GameMenu>(OnGameMenuChanged));
		GlobalReferences.Instance.GameMenuState.ClearInMenu(GameMenuState.GameMenu.Minigame);
		GlobalReferences.Instance.EventChannels.Player.PlayerTakeDamage.Unregister(TryCloseMinigame);
		GlobalReferences.Instance.EventChannels.Player.PlayerDead.Unregister(TryCloseMinigame);
	}

	public bool CanPhotograph()
	{
		bool flag = MinigameMetadata.CanPhotograph && m_cameraItemDefinition != null;
		if (flag && GlobalReferences.Instance.MainInventory.GetItemOfType(m_cameraItemDefinition) == null)
		{
			flag = false;
		}
		return flag;
	}

	private void TakePhotoInput(InputAction.CallbackContext obj)
	{
		if (!m_takenPhotograph)
		{
			m_takenPhotograph = true;
			m_photoController.TakePhotograph();
			m_cameraPrompt.SetActive(value: false);
		}
	}

	private void OnGameMenuChanged(GameMenuState.GameMenu gameMenuState)
	{
		GraphicRaycaster componentInChildren = GetComponentInChildren<GraphicRaycaster>();
		if (componentInChildren != null)
		{
			componentInChildren.enabled = !gameMenuState.HasFlag(GameMenuState.GameMenu.InGameMenu);
		}
	}

	public void TryCloseMinigame()
	{
		CloseMinigame();
	}

	private void CloseMinigame()
	{
		if (!m_isClosing)
		{
			m_isClosing = true;
			m_closingCoroutine = StartCoroutine(OnCloseDelay());
		}
	}

	public void SetMinigameCompleted()
	{
		if (!m_minigameCompleted)
		{
			m_minigameCompleted = true;
			CloseMinigame();
		}
	}

	private IEnumerator OnCloseDelay()
	{
		if (m_minigameCompleted)
		{
			if (m_minigameCloseDelayOnSuccess > 0f)
			{
				yield return new WaitForSecondsRealtime(m_minigameCloseDelayOnSuccess);
			}
			else
			{
				yield return new WaitForSecondsRealtime(1f);
			}
		}
		if (m_minigameMetadata.DoFullscreenFade)
		{
			GlobalReferences.Instance.EventChannels.Generic.ScreenFadeOfType.Raise(FadeType.Minigame);
			yield return new WaitForSecondsRealtime(0.75f);
		}
		else
		{
			if (m_uiCanvasGroup != null)
			{
				DOTween.Kill(m_uiCanvasGroup);
				m_uiCanvasGroup.DOFade(0f, 0.5f).SetUpdate(isIndependentUpdate: true);
			}
			DOTween.Kill(m_canvasGroup);
			yield return m_canvasGroup.DOFade(0f, 0.5f).SetUpdate(isIndependentUpdate: true).WaitForCompletion();
		}
		if (m_minigameCompleted)
		{
			m_onMinigameComplete?.Invoke();
		}
		else
		{
			m_onMinigameClose?.Invoke();
		}
	}

	public void SetCustomOutputInteger(int value)
	{
		m_customOutputValue = value;
	}

	public bool OnInputBack()
	{
		bool flag = true;
		IMinigameBackInputHandler[] minigameBackInputHandler = m_minigameBackInputHandler;
		for (int i = 0; i < minigameBackInputHandler.Length; i++)
		{
			if (minigameBackInputHandler[i].TryBackInput())
			{
				flag = false;
			}
		}
		if (!GlobalReferences.Instance.GameMenuState.IsInMenuExclusively(GameMenuState.GameMenu.Minigame))
		{
			flag = false;
		}
		if (flag)
		{
			TryCloseMinigame();
		}
		return false;
	}
}
