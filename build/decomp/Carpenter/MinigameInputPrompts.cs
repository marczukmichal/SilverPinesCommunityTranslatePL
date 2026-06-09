using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MinigameInputPrompts : MonoBehaviour
{
	[SerializeField]
	private GameObject m_closePrompt;

	[SerializeField]
	private GameObject m_interactPrompt;

	[SerializeField]
	private CanvasGroup m_canvasGroup;

	[SerializeField]
	private GameObject m_minigameContentParent;

	[Header("Input Action Override")]
	[SerializeField]
	private InputPrompt m_actionPrompt;

	[SerializeField]
	private InputActionReference m_keyboardAction;

	[SerializeField]
	private InputActionReference m_gamepadAction;

	[Header("Active / Custom Actions")]
	[SerializeField]
	private InputPrompt m_customActionPrompt1;

	[SerializeField]
	private InputPrompt m_customActionPrompt2;

	private BaseMinigameHoldInteract m_activeHoldInteract;

	private void Awake()
	{
		bool active = false;
		if (m_minigameContentParent != null)
		{
			if (m_minigameContentParent.GetComponentsInChildren<Selectable>(includeInactive: true).Length != 0)
			{
				active = true;
			}
			BaseMinigameHoldInteract[] componentsInChildren = m_minigameContentParent.GetComponentsInChildren<BaseMinigameHoldInteract>(includeInactive: true);
			if (componentsInChildren.Length != 0)
			{
				active = true;
				BaseMinigameHoldInteract[] array = componentsInChildren;
				foreach (BaseMinigameHoldInteract obj in array)
				{
					obj.OnStartHoldInteractEvent = (UnityAction<BaseMinigameHoldInteract>)Delegate.Combine(obj.OnStartHoldInteractEvent, new UnityAction<BaseMinigameHoldInteract>(OnHoldInteractChanged));
				}
			}
		}
		else
		{
			active = true;
		}
		m_interactPrompt.SetActive(active);
	}

	private void OnHoldInteractChanged(BaseMinigameHoldInteract activeHoldInteract)
	{
		if (m_activeHoldInteract != null)
		{
			BaseMinigameHoldInteract activeHoldInteract2 = m_activeHoldInteract;
			activeHoldInteract2.OnInputOptionsChangedEvent = (UnityAction)Delegate.Remove(activeHoldInteract2.OnInputOptionsChangedEvent, new UnityAction(Refresh));
		}
		m_activeHoldInteract = activeHoldInteract;
		if (m_activeHoldInteract != null)
		{
			BaseMinigameHoldInteract activeHoldInteract3 = m_activeHoldInteract;
			activeHoldInteract3.OnInputOptionsChangedEvent = (UnityAction)Delegate.Combine(activeHoldInteract3.OnInputOptionsChangedEvent, new UnityAction(Refresh));
		}
		Refresh();
	}

	private void Refresh()
	{
		OnInputModeChanged(GlobalReferences.Instance.InputState.InputMode);
	}

	private void OnEnable()
	{
		GameMenuState gameMenuState = GlobalReferences.Instance.GameMenuState;
		gameMenuState.OnGameMenuChanged = (UnityAction<GameMenuState.GameMenu>)Delegate.Combine(gameMenuState.OnGameMenuChanged, new UnityAction<GameMenuState.GameMenu>(OnMenuChanged));
		InputState inputState = GlobalReferences.Instance.InputState;
		inputState.OnInputModeChanged = (UnityAction<InputState.Mode>)Delegate.Combine(inputState.OnInputModeChanged, new UnityAction<InputState.Mode>(OnInputModeChanged));
		OnInputModeChanged(GlobalReferences.Instance.InputState.InputMode);
	}

	private void OnDisable()
	{
		GameMenuState gameMenuState = GlobalReferences.Instance.GameMenuState;
		gameMenuState.OnGameMenuChanged = (UnityAction<GameMenuState.GameMenu>)Delegate.Remove(gameMenuState.OnGameMenuChanged, new UnityAction<GameMenuState.GameMenu>(OnMenuChanged));
		InputState inputState = GlobalReferences.Instance.InputState;
		inputState.OnInputModeChanged = (UnityAction<InputState.Mode>)Delegate.Remove(inputState.OnInputModeChanged, new UnityAction<InputState.Mode>(OnInputModeChanged));
	}

	private void OnInputModeChanged(InputState.Mode mode)
	{
		switch (mode)
		{
		case InputState.Mode.KeyboardMouse:
			m_actionPrompt.SetInputAction(m_keyboardAction);
			break;
		case InputState.Mode.Gamepad:
			m_actionPrompt.SetInputAction(m_gamepadAction);
			break;
		}
		bool active = false;
		bool active2 = false;
		if (m_activeHoldInteract != null)
		{
			BaseMinigameHoldInteract.MinigameHoldCustomInputPrompt action = default(BaseMinigameHoldInteract.MinigameHoldCustomInputPrompt);
			BaseMinigameHoldInteract.MinigameHoldCustomInputPrompt action2 = default(BaseMinigameHoldInteract.MinigameHoldCustomInputPrompt);
			m_activeHoldInteract.PopulateCustomInputs(ref action, ref action2);
			if (action.m_inputAction != null)
			{
				m_customActionPrompt1.SetInputAction(action.m_inputAction);
				m_customActionPrompt1.SetLabelText(action.m_inputString);
				active = true;
			}
			if (action2.m_inputAction != null)
			{
				m_customActionPrompt2.SetInputAction(action2.m_inputAction);
				m_customActionPrompt2.SetLabelText(action2.m_inputString);
				active2 = true;
			}
		}
		if (m_customActionPrompt1 != null)
		{
			m_customActionPrompt1.gameObject.SetActive(active);
		}
		if (m_customActionPrompt2 != null)
		{
			m_customActionPrompt2.gameObject.SetActive(active2);
		}
	}

	private void OnMenuChanged(GameMenuState.GameMenu menuState)
	{
		bool flag = menuState.HasFlag(GameMenuState.GameMenu.InGameMenu) || menuState.HasFlag(GameMenuState.GameMenu.SystemMenu) || menuState.HasFlag(GameMenuState.GameMenu.GameNotesReader);
		m_canvasGroup.alpha = (flag ? 0f : 1f);
	}
}
