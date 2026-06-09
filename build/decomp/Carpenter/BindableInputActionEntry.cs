using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class BindableInputActionEntry : MonoBehaviour
{
	[SerializeField]
	private LocalizeStringEvent m_labelLocalization;

	[Header("Keyboard & Mouse")]
	[SerializeField]
	private GameObject m_keyboardGameObject;

	[SerializeField]
	private InputPrompt m_keyboardInputPrompt;

	[SerializeField]
	private CanvasGroup m_keyboardCanvasGroup;

	[SerializeField]
	private Button m_keyboardButton;

	[Header("Keyboard & Mouse Alternate")]
	[SerializeField]
	private GameObject m_keyboardAltGameObject;

	[SerializeField]
	private InputPrompt m_keyboardAltInputPrompt;

	[SerializeField]
	private CanvasGroup m_keyboardAltCanvasGroup;

	[SerializeField]
	private Button m_keyboardAltButton;

	[Header("Gamepad")]
	[SerializeField]
	private GameObject m_gamepadGameObject;

	[SerializeField]
	private InputPrompt m_gamepadInputPrompt;

	[SerializeField]
	private CanvasGroup m_gamepadCanvasGroup;

	[SerializeField]
	private Button m_gamepadButton;

	[Header("Reset")]
	[SerializeField]
	private Button m_resetButton;

	[Header("UI")]
	[SerializeField]
	private GameObject m_selectedBackground;

	[SerializeField]
	private TextMeshProUGUI m_textLabel;

	[SerializeField]
	private Color m_labelNormalColor;

	[SerializeField]
	private Color m_labelLockedColor;

	[SerializeField]
	private Color m_buttonNormalColor;

	[SerializeField]
	private Color m_buttonErrorColor;

	private BindableInputSettings m_settings;

	private bool m_canReset;

	public BindableInputSettings Settings => m_settings;

	public void Configure(BindableInputSettings settings)
	{
		m_settings = settings;
		if (!settings.m_label.IsEmpty)
		{
			m_textLabel.text = settings.m_label.GetLocalizedString();
		}
		m_labelLocalization.StringReference = settings.m_label;
		m_labelLocalization.RefreshString();
		if (settings.m_keyboardAction.m_action != null)
		{
			m_keyboardGameObject.SetActive(value: true);
			SetupInputPrompt(m_keyboardInputPrompt, settings.m_keyboardAction);
			if (!settings.m_keyboardAction.m_isLockedControls)
			{
				m_keyboardButton.onClick.AddListener(OnKeyboardBindingClicked);
			}
			m_keyboardCanvasGroup.alpha = (settings.m_keyboardAction.m_isLockedControls ? 0.4f : 1f);
		}
		else
		{
			m_keyboardGameObject.SetActive(value: false);
		}
		if (settings.m_keyboardAltAction.m_action != null)
		{
			m_keyboardAltGameObject.SetActive(value: true);
			SetupInputPrompt(m_keyboardAltInputPrompt, settings.m_keyboardAltAction);
			if (!settings.m_keyboardAltAction.m_isLockedControls)
			{
				m_keyboardAltButton.onClick.AddListener(OnKeyboardAltBindingClicked);
			}
			m_keyboardAltCanvasGroup.alpha = (settings.m_keyboardAltAction.m_isLockedControls ? 0.4f : 1f);
		}
		else
		{
			m_keyboardAltGameObject.SetActive(value: false);
		}
		if (settings.m_gamepadAction.m_action != null)
		{
			m_gamepadGameObject.SetActive(value: true);
			SetupInputPrompt(m_gamepadInputPrompt, settings.m_gamepadAction);
			if (!settings.m_gamepadAction.m_isLockedControls)
			{
				m_gamepadButton.onClick.AddListener(OnGamepadBindingClicked);
			}
			m_gamepadCanvasGroup.alpha = (settings.m_gamepadAction.m_isLockedControls ? 0.4f : 1f);
		}
		else
		{
			m_gamepadGameObject.SetActive(value: false);
		}
		RefreshLockedLabelStatus();
		RefreshResetStatus();
		m_resetButton.onClick.AddListener(OnResetButtonClicked);
		HideUnavailableControls();
		RefreshErrorState();
	}

	private void OnEnable()
	{
		InputState inputState = GlobalReferences.Instance.InputState;
		inputState.OnInputModeChanged = (UnityAction<InputState.Mode>)Delegate.Combine(inputState.OnInputModeChanged, new UnityAction<InputState.Mode>(OnInputModeChanged));
		HideUnavailableControls();
		RefreshLockedLabelStatus();
		RefreshResetStatus();
		RefreshErrorState();
	}

	private void OnDisable()
	{
		InputState inputState = GlobalReferences.Instance.InputState;
		inputState.OnInputModeChanged = (UnityAction<InputState.Mode>)Delegate.Remove(inputState.OnInputModeChanged, new UnityAction<InputState.Mode>(OnInputModeChanged));
	}

	private void OnInputModeChanged(InputState.Mode arg0)
	{
		HideUnavailableControls();
		RefreshResetStatus();
		RefreshLockedLabelStatus();
	}

	private void SetupInputPrompt(InputPrompt inputPrompt, ActionBindingReference reference)
	{
		inputPrompt.SetInputAction(reference.m_action);
		InputAction action = reference.m_action.action;
		if (action != null)
		{
			int num = action.bindings.IndexOf((InputBinding x) => x.id.ToString() == reference.m_bindingId);
			if (num != -1)
			{
				inputPrompt.SetBindingIndexOverride(num);
			}
		}
	}

	private bool CanReset()
	{
		if (m_settings == null)
		{
			return false;
		}
		if (m_settings.m_gamepadAction.m_action != null && GlobalReferences.Instance.InputState.InputMode == InputState.Mode.Gamepad)
		{
			if (m_settings.m_gamepadAction.m_isLockedControls)
			{
				return false;
			}
			InputAction inputAction = GameInputManager.GameInputActions.FindAction(m_settings.m_gamepadAction.m_action.action.name);
			if (inputAction != null)
			{
				foreach (InputBinding binding in inputAction.bindings)
				{
					if (binding.id.ToString().Equals(m_settings.m_gamepadAction.m_bindingId) && binding.hasOverrides)
					{
						return true;
					}
				}
			}
		}
		if (m_settings.m_keyboardAction.m_action != null && GlobalReferences.Instance.InputState.InputMode == InputState.Mode.KeyboardMouse)
		{
			if (m_settings.m_keyboardAction.m_isLockedControls)
			{
				return false;
			}
			InputAction inputAction2 = GameInputManager.GameInputActions.FindAction(m_settings.m_keyboardAction.m_action.action.name);
			if (inputAction2 != null)
			{
				foreach (InputBinding binding2 in inputAction2.bindings)
				{
					if (binding2.id.ToString().Equals(m_settings.m_keyboardAction.m_bindingId) && binding2.hasOverrides)
					{
						return true;
					}
				}
			}
		}
		if (m_settings.m_keyboardAltAction.m_action != null && GlobalReferences.Instance.InputState.InputMode == InputState.Mode.KeyboardMouse)
		{
			if (m_settings.m_keyboardAltAction.m_isLockedControls)
			{
				return false;
			}
			InputAction inputAction3 = GameInputManager.GameInputActions.FindAction(m_settings.m_keyboardAltAction.m_action.action.name);
			if (inputAction3 != null)
			{
				foreach (InputBinding binding3 in inputAction3.bindings)
				{
					if (binding3.id.ToString().Equals(m_settings.m_keyboardAltAction.m_bindingId) && binding3.hasOverrides)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private bool IsLockedControlForCurrentInputMode()
	{
		if (m_settings != null)
		{
			if (m_settings.m_gamepadAction.m_action != null && GlobalReferences.Instance.InputState.InputMode == InputState.Mode.Gamepad)
			{
				return m_settings.m_gamepadAction.m_isLockedControls;
			}
			if (m_settings.m_keyboardAction.m_action != null && GlobalReferences.Instance.InputState.InputMode == InputState.Mode.KeyboardMouse)
			{
				return m_settings.m_keyboardAction.m_isLockedControls;
			}
		}
		return false;
	}

	private void RefreshLockedLabelStatus()
	{
		m_textLabel.color = (IsLockedControlForCurrentInputMode() ? m_labelLockedColor : m_labelNormalColor);
	}

	private void RefreshResetStatus()
	{
		m_canReset = CanReset();
		m_resetButton.gameObject.SetActive(m_canReset);
	}

	public void Refresh()
	{
		m_keyboardInputPrompt.RefreshPrompt();
		m_keyboardAltInputPrompt.RefreshPrompt();
		m_gamepadInputPrompt.RefreshPrompt();
		RefreshResetStatus();
		HideUnavailableControls();
		RefreshErrorState();
	}

	private void RefreshErrorState()
	{
		if (m_settings != null)
		{
			RefreshErrorState(m_gamepadButton, m_settings.m_gamepadAction);
			RefreshErrorState(m_keyboardButton, m_settings.m_keyboardAction);
			RefreshErrorState(m_keyboardAltButton, m_settings.m_keyboardAltAction);
		}
	}

	private void RefreshErrorState(Button button, ActionBindingReference bindingReference)
	{
		bool flag = false;
		if (bindingReference.m_action != null)
		{
			InputAction inputAction = GameInputManager.GameInputActions.FindAction(bindingReference.m_action.name);
			if (inputAction != null)
			{
				foreach (InputBinding binding in inputAction.bindings)
				{
					if (binding.id.ToString().Equals(bindingReference.m_bindingId) && string.IsNullOrEmpty(binding.effectivePath))
					{
						flag = true;
						break;
					}
				}
			}
		}
		ColorBlock colors = button.colors;
		colors.normalColor = (flag ? m_buttonErrorColor : m_buttonNormalColor);
		button.colors = colors;
	}

	public void HideUnavailableControls()
	{
		if (m_settings != null)
		{
			InputState.Mode inputMode = GlobalReferences.Instance.InputState.InputMode;
			bool flag = Gamepad.current != null && m_settings.m_gamepadAction.m_action != null && inputMode == InputState.Mode.Gamepad;
			bool flag2 = Keyboard.current != null && m_settings.m_keyboardAction.m_action != null && inputMode == InputState.Mode.KeyboardMouse;
			bool active = Keyboard.current != null && m_settings.m_keyboardAltAction.m_action != null && inputMode == InputState.Mode.KeyboardMouse;
			m_gamepadGameObject.SetActive(flag);
			m_keyboardGameObject.SetActive(flag2);
			m_keyboardAltGameObject.SetActive(active);
			base.gameObject.SetActive(flag || flag2);
		}
	}

	private void OnGamepadBindingClicked()
	{
		GlobalReferences.Instance.EventChannels.Input.OnRequestRebindGamepadInputAction.Raise(this);
	}

	private void OnKeyboardBindingClicked()
	{
		GlobalReferences.Instance.EventChannels.Input.OnRequestRebindKeyboardInputAction.Raise(this);
	}

	private void OnKeyboardAltBindingClicked()
	{
		GlobalReferences.Instance.EventChannels.Input.OnRequestRebindKeyboardAltInputAction.Raise(this);
	}

	private void OnResetButtonClicked()
	{
		if (m_canReset)
		{
			GlobalReferences.Instance.EventChannels.Input.OnRequestResetInputAction.Raise(this);
			if (GlobalReferences.Instance.InputState.InputMode == InputState.Mode.Gamepad)
			{
				EventSystem.current.SetSelectedGameObject(m_gamepadGameObject);
			}
			else
			{
				EventSystem.current.SetSelectedGameObject(m_keyboardGameObject);
			}
		}
	}

	private void Update()
	{
		GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
		bool flag = false;
		flag = ((currentSelectedGameObject == m_gamepadButton.gameObject || currentSelectedGameObject == m_keyboardButton.gameObject || currentSelectedGameObject == m_keyboardAltButton.gameObject || currentSelectedGameObject == m_resetButton.gameObject) ? true : false);
		m_selectedBackground.SetActive(flag);
	}
}
