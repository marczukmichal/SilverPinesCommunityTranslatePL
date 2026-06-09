using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class HintInfoUI : MonoBehaviour
{
	[Header("Event Channels")]
	[SerializeField]
	private HintInfoGameEventChannel m_hintInfoEventChannel;

	[SerializeField]
	private HintInfoGameEventChannel m_cancelHintInfoEventChannel;

	[Header("UI")]
	[SerializeField]
	private CanvasGroup m_canvasGroup;

	[SerializeField]
	private InlineButtonPromptsText m_text;

	[SerializeField]
	private InputPrompt m_inputButtonPrompt;

	[Header("Animation")]
	[SerializeField]
	private float m_initalShowDelay = 1f;

	[SerializeField]
	private float m_showAnimTime = 0.5f;

	[SerializeField]
	private float m_holdAnimTime = 10f;

	[SerializeField]
	private float m_hideDelay = 2f;

	[Header("Game Menu State")]
	[SerializeField]
	private GameMenuState m_gameMenuState;

	private float m_timer;

	private List<HintInfo> m_queuedHints;

	private HintInfo m_activeHint;

	private HintInfo m_activeAreaHint;

	private bool m_showingHint;

	private bool m_hintHiddenByMenu;

	private bool m_inputPressed;

	private void OnEnable()
	{
		m_queuedHints = new List<HintInfo>();
		m_canvasGroup.alpha = 0f;
		m_canvasGroup.gameObject.SetActive(value: false);
		m_hintInfoEventChannel.Register(OnShowHint);
		m_cancelHintInfoEventChannel.Register(OnHideHint);
		GlobalReferences.Instance.Anchors.Hints.ActiveAreaHintInfoAnchor.Register(OnAreaHintChanged);
		GameMenuState gameMenuState = m_gameMenuState;
		gameMenuState.OnGameMenuChanged = (UnityAction<GameMenuState.GameMenu>)Delegate.Combine(gameMenuState.OnGameMenuChanged, new UnityAction<GameMenuState.GameMenu>(MenuStateChanged));
	}

	private void OnDisable()
	{
		m_hintInfoEventChannel.Unregister(OnShowHint);
		m_cancelHintInfoEventChannel.Unregister(OnHideHint);
		GlobalReferences.Instance.Anchors.Hints.ActiveAreaHintInfoAnchor.Unregister(OnAreaHintChanged);
		GameMenuState gameMenuState = m_gameMenuState;
		gameMenuState.OnGameMenuChanged = (UnityAction<GameMenuState.GameMenu>)Delegate.Remove(gameMenuState.OnGameMenuChanged, new UnityAction<GameMenuState.GameMenu>(MenuStateChanged));
	}

	private void OnAreaHintChanged(HintInfo areaHint)
	{
		if (areaHint == null && m_activeHint == m_activeAreaHint)
		{
			OnHideHint(m_activeAreaHint);
		}
		m_activeAreaHint = areaHint;
		RefreshHintState();
	}

	private void OnShowHint(HintInfo hint)
	{
		if (GlobalReferences.Instance.UserPreferences.ShowHints && !(m_activeHint == hint))
		{
			if (!m_queuedHints.Contains(hint))
			{
				m_queuedHints.Add(hint);
			}
			RefreshHintState();
		}
	}

	private void OnHideHint(HintInfo hint)
	{
		if (m_activeHint == hint)
		{
			m_activeHint = null;
		}
		if (m_queuedHints.Contains(hint))
		{
			m_queuedHints.Remove(hint);
		}
		RefreshHintState();
	}

	private HintInfo GetNextHint()
	{
		HintInfo result = null;
		if (m_activeAreaHint != null)
		{
			result = m_activeAreaHint;
		}
		else if (m_queuedHints.Count > 0)
		{
			result = m_queuedHints[0];
			m_queuedHints.RemoveAt(0);
		}
		return result;
	}

	private void RefreshHintState()
	{
		if (!m_showingHint)
		{
			HintInfo nextHint = GetNextHint();
			if (nextHint != null)
			{
				StopAllCoroutines();
				StartCoroutine(ShowHint(nextHint));
			}
		}
	}

	private void InputDone(InputAction.CallbackContext callback)
	{
		m_inputPressed = true;
	}

	private IEnumerator ShowHint(HintInfo hint)
	{
		m_activeHint = hint;
		m_text.SetText(hint.HintText, hint.InlineTextInputActions);
		InputAction fixedAction = null;
		if (hint.InputAction != null)
		{
			fixedAction = GameInputManager.GameInputActions.FindAction(hint.InputAction.name);
		}
		m_inputButtonPrompt.gameObject.SetActive(fixedAction != null);
		if (fixedAction != null)
		{
			m_inputButtonPrompt.SetInputAction(fixedAction);
		}
		m_inputPressed = false;
		m_showingHint = true;
		m_timer = 0f;
		if (fixedAction != null && !hint.DontClearOnInput)
		{
			fixedAction.performed += InputDone;
		}
		yield return new WaitForSeconds(m_initalShowDelay);
		if (!m_inputPressed)
		{
			m_canvasGroup.gameObject.SetActive(value: true);
			yield return m_canvasGroup.DOFade(1f, m_showAnimTime).WaitForCompletion();
			while (m_activeHint == hint && (m_timer <= m_holdAnimTime || m_activeHint.DisableTimeout) && (!m_inputPressed || m_timer <= hint.MinimumShowTime))
			{
				yield return new WaitForEndOfFrame();
				if (!GlobalReferences.Instance.GameMenuState.IsInMenu(GameMenuState.GameMenu.InGameMenu))
				{
					m_timer += Time.deltaTime;
				}
			}
			if (fixedAction != null && !hint.DontClearOnInput)
			{
				fixedAction.performed -= InputDone;
			}
			yield return m_canvasGroup.DOFade(0f, m_showAnimTime).SetDelay(m_hideDelay).WaitForCompletion();
			m_canvasGroup.gameObject.SetActive(value: false);
		}
		m_showingHint = false;
		m_activeHint = null;
		HintInfo nextHint = GetNextHint();
		if (nextHint != null)
		{
			StartCoroutine(ShowHint(nextHint));
		}
	}

	private void MenuStateChanged(GameMenuState.GameMenu menu)
	{
		m_hintHiddenByMenu = menu != GameMenuState.GameMenu.None;
		if (m_showingHint)
		{
			m_canvasGroup.gameObject.SetActive(!m_hintHiddenByMenu);
		}
	}
}
