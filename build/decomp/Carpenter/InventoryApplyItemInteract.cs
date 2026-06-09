using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class InventoryApplyItemInteract : MonoBehaviour
{
	[Header("Events")]
	[SerializeField]
	private UnityEvent m_onUsedSuccess;

	[SerializeField]
	private UnityEvent m_onUsedFail;

	[SerializeField]
	private AudioEvent m_applyItemCorrectItemAudioEvent;

	[SerializeField]
	private AudioEvent m_applyItemWrongItemAudioEvent;

	[Header("UI")]
	[SerializeField]
	private RectTransform m_clickableArea;

	[SerializeField]
	private bool m_alwaysShowInventory;

	[SerializeField]
	private float m_closeDelay = 1f;

	private bool m_showingInventory;

	private UIMinigameHighlight m_minigameHighlight;

	public RectTransform ClickableArea => m_clickableArea;

	public bool AlwaysShowInventory => m_alwaysShowInventory;

	public float CloseDelay => m_closeDelay;

	public void Initialise()
	{
		m_minigameHighlight = GetComponentInChildren<UIMinigameHighlight>();
		if (m_clickableArea != null)
		{
			Button button = m_clickableArea.gameObject.AddComponent<Button>();
			button.navigation = new Navigation
			{
				mode = Navigation.Mode.None
			};
			button.transition = Selectable.Transition.None;
			button.onClick.AddListener(OnClickableAreaSelected);
			GamepadCursorMagnetismTarget gamepadCursorMagnetismTarget = m_clickableArea.gameObject.AddComponent<GamepadCursorMagnetismTarget>();
			if (gamepadCursorMagnetismTarget != null)
			{
				gamepadCursorMagnetismTarget.SetMagnetismDisabled(disabled: true);
			}
		}
	}

	private void OnEnable()
	{
		GlobalReferences.Instance.EventChannels.Generic.TryApplyItemSuccessFail.Register(OnTryApplyItemEvent);
		GameMenuState gameMenuState = GlobalReferences.Instance.GameMenuState;
		gameMenuState.OnGameMenuChanged = (UnityAction<GameMenuState.GameMenu>)Delegate.Combine(gameMenuState.OnGameMenuChanged, new UnityAction<GameMenuState.GameMenu>(OnGameMenuChanged));
		StartCoroutine(ShowLockedIntro());
	}

	private void OnDisable()
	{
		if (m_minigameHighlight != null)
		{
			m_minigameHighlight.SetSelected(selected: false);
		}
		GlobalReferences.Instance.EventChannels.Generic.TryApplyItemSuccessFail.Unregister(OnTryApplyItemEvent);
		if (m_showingInventory)
		{
			GlobalReferences.Instance.EventChannels.Generic.SetGamepadCursorAllowed.Raise(value: true);
		}
		GameMenuState gameMenuState = GlobalReferences.Instance.GameMenuState;
		gameMenuState.OnGameMenuChanged = (UnityAction<GameMenuState.GameMenu>)Delegate.Remove(gameMenuState.OnGameMenuChanged, new UnityAction<GameMenuState.GameMenu>(OnGameMenuChanged));
	}

	private IEnumerator ShowLockedIntro()
	{
		yield return new WaitForSecondsRealtime(0.1f);
		OnTryApplyItemEvent(success: false);
	}

	private void OnTryApplyItemEvent(bool success)
	{
		if (m_minigameHighlight != null)
		{
			m_minigameHighlight.SetSelected(selected: false);
		}
		AudioEvent.Play(success ? m_applyItemCorrectItemAudioEvent : m_applyItemWrongItemAudioEvent, base.transform.position);
		if (success)
		{
			m_onUsedSuccess.Invoke();
		}
		else
		{
			m_onUsedFail.Invoke();
		}
	}

	private void OnClickableAreaSelected()
	{
		m_showingInventory = true;
		GlobalReferences.Instance.EventChannels.InGameMenu.ShowInventoryMenuTab.Raise();
		GlobalReferences.Instance.EventChannels.Generic.SetGamepadCursorAllowed.Raise(value: false);
		GlobalReferences.Instance.EventChannels.Minigames.MinigameHighlightInteract.Raise(m_clickableArea.gameObject);
		if (m_minigameHighlight != null)
		{
			m_minigameHighlight.SetSelected(selected: true);
		}
	}

	private void OnGameMenuChanged(GameMenuState.GameMenu gameMenu)
	{
		if (!gameMenu.HasFlag(GameMenuState.GameMenu.InGameMenu) && m_showingInventory)
		{
			m_showingInventory = false;
			GlobalReferences.Instance.EventChannels.Generic.SetGamepadCursorAllowed.Raise(value: true);
			GlobalReferences.Instance.EventChannels.Minigames.MinigameHighlightInteract.Raise(null);
			if (m_minigameHighlight != null)
			{
				m_minigameHighlight.SetSelected(selected: false);
			}
		}
	}
}
