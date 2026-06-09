using System;
using System.Collections;
using TMPro;
using Team17;
using Team17.InputButtons;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class InputPrompt : MonoBehaviour
{
	private enum DynamicPromptMode
	{
		Normal,
		ForceKeyboardMouse,
		ForceGamepad
	}

	[SerializeField]
	private TextMeshProUGUI m_text;

	[SerializeField]
	private LayoutElement m_layoutElement;

	[SerializeField]
	private float m_baseWidth = 48f;

	[SerializeField]
	private float m_extraWidthPerChar = 6f;

	[FormerlySerializedAs("m_gamepadButton")]
	[SerializeField]
	private Image m_inputIcon;

	[SerializeField]
	private GameObject m_keyboardButton;

	[SerializeField]
	private TextMeshProUGUI m_label;

	[Header("Input")]
	[SerializeField]
	private InputState m_inputState;

	[Header("Modes")]
	[SerializeField]
	private DynamicPromptMode m_dynamicPromptMode;

	[Header("Input")]
	[FormerlySerializedAs("m_inputAction")]
	[SerializeField]
	private InputActionReference m_inputActionReference;

	[SerializeField]
	private int m_bindingIndexOverride = -1;

	[SerializeField]
	private bool m_submitIsClickMouse = true;

	private InputAction m_inputAction;

	private InputAction InputAction
	{
		get
		{
			if (m_submitIsClickMouse && m_inputAction == GameInputManager.GameInputActions.UI.Submit && GlobalReferences.Instance.InputState.InputMode == InputState.Mode.KeyboardMouse)
			{
				return GameInputManager.GameInputActions.UI.Click;
			}
			return m_inputAction;
		}
	}

	public void SetBindingIndexOverride(int bindingIndex)
	{
		m_bindingIndexOverride = bindingIndex;
		RefreshPrompt();
	}

	public void ClearBindingIndexOverride()
	{
		m_bindingIndexOverride = -1;
	}

	private void Reset()
	{
		m_layoutElement = GetComponent<LayoutElement>();
		m_text = GetComponentInChildren<TextMeshProUGUI>();
	}

	private void OnEnable()
	{
		if (m_inputActionReference != null && m_inputAction == null)
		{
			m_inputAction = GameInputManager.GameInputActions.FindAction(m_inputActionReference.action.name);
		}
		InputState inputState = m_inputState;
		inputState.OnInputModeChanged = (UnityAction<InputState.Mode>)Delegate.Combine(inputState.OnInputModeChanged, new UnityAction<InputState.Mode>(InputModeChanged));
		UpdatePrompt();
	}

	private void OnDisable()
	{
		InputState inputState = m_inputState;
		inputState.OnInputModeChanged = (UnityAction<InputState.Mode>)Delegate.Remove(inputState.OnInputModeChanged, new UnityAction<InputState.Mode>(InputModeChanged));
	}

	private void InputModeChanged(InputState.Mode mode)
	{
		StartCoroutine(DelayedUpdate());
	}

	private IEnumerator DelayedUpdate()
	{
		yield return new WaitForEndOfFrame();
		UpdatePrompt();
	}

	public static ButtonPromptIconSettings GetButtonPromptSettingsForDevice(InputDevice device)
	{
		if (Services.TryGet<IOverrideButtonPromptSettingsService>(out var service))
		{
			return service.GetButtonPromptIconSettings();
		}
		if (device == null || device is Keyboard || device is Mouse)
		{
			return GlobalReferences.Instance.KeyboardMouseButtonPrompts;
		}
		switch (GlobalReferences.Instance.UserPreferences.InputPromptGamepadDetectionMode)
		{
		case InputPromptGamepadDetectionMode.AutoDetect:
			if (device is XInputController)
			{
				return GlobalReferences.Instance.XboxButtonPrompts;
			}
			if (device is DualShockGamepad)
			{
				return GlobalReferences.Instance.PlaystationButtonPrompts;
			}
			return GlobalReferences.Instance.XboxButtonPrompts;
		case InputPromptGamepadDetectionMode.ForceXbox:
			return GlobalReferences.Instance.XboxButtonPrompts;
		case InputPromptGamepadDetectionMode.ForcePlaystation:
			return GlobalReferences.Instance.PlaystationButtonPrompts;
		case InputPromptGamepadDetectionMode.ForceSwitch:
			return GlobalReferences.Instance.SwitchButtonPrompts;
		default:
			return GlobalReferences.Instance.XboxButtonPrompts;
		}
	}

	public static Sprite GetIconSprite(InputDevice device, InputAction action, string controlPath, string buttonDisplayString)
	{
		_ = GlobalReferences.Instance.UserPreferences.InputPromptGamepadDetectionMode;
		Sprite buttonPrompt = GetButtonPromptSettingsForDevice(device).GetButtonPrompt(action, controlPath, buttonDisplayString);
		if (buttonPrompt == null)
		{
			Debug.LogError("Unknown button for " + buttonDisplayString + " uses control path: " + controlPath);
		}
		return buttonPrompt;
	}

	private void UpdatePrompt()
	{
		if (InputAction == null)
		{
			return;
		}
		Sprite sprite = null;
		string text = "";
		string controlPath = "";
		bool flag = false;
		InputAction inputAction = InputAction;
		string deviceLayoutName;
		switch (m_dynamicPromptMode)
		{
		case DynamicPromptMode.Normal:
		{
			if (m_inputState.InputMode == InputState.Mode.Gamepad)
			{
				InputDevice current2 = Gamepad.current;
				int bindingIndexForDeviceWithOverride4 = GetBindingIndexForDeviceWithOverride(inputAction, current2);
				if (bindingIndexForDeviceWithOverride4 != -1)
				{
					text = inputAction.GetBindingDisplayString(bindingIndexForDeviceWithOverride4, out deviceLayoutName, out controlPath);
					if (!string.IsNullOrEmpty(controlPath))
					{
						sprite = GetIconSprite(current2, inputAction, controlPath, text);
					}
					else
					{
						flag = true;
					}
				}
				else
				{
					flag = true;
				}
				break;
			}
			int bindingIndexForDeviceWithOverride5 = GetBindingIndexForDeviceWithOverride(inputAction, Mouse.current);
			int bindingIndexForDeviceWithOverride6 = GetBindingIndexForDeviceWithOverride(inputAction, Keyboard.current);
			int num2 = ((bindingIndexForDeviceWithOverride5 == -1) ? bindingIndexForDeviceWithOverride6 : bindingIndexForDeviceWithOverride5);
			if (num2 != -1)
			{
				text = inputAction.GetBindingDisplayString(num2, out deviceLayoutName, out controlPath);
			}
			if (!string.IsNullOrEmpty(controlPath))
			{
				sprite = GlobalReferences.Instance.KeyboardMouseButtonPrompts.GetButtonPrompt(inputAction, controlPath, text);
			}
			break;
		}
		case DynamicPromptMode.ForceKeyboardMouse:
		{
			int bindingIndexForDeviceWithOverride2 = GetBindingIndexForDeviceWithOverride(inputAction, Mouse.current);
			int bindingIndexForDeviceWithOverride3 = GetBindingIndexForDeviceWithOverride(inputAction, Keyboard.current);
			int num = ((bindingIndexForDeviceWithOverride2 == -1) ? bindingIndexForDeviceWithOverride3 : bindingIndexForDeviceWithOverride2);
			if (num != -1)
			{
				text = inputAction.GetBindingDisplayString(num, out deviceLayoutName, out controlPath);
			}
			else
			{
				flag = true;
				text = "";
			}
			if (!string.IsNullOrEmpty(controlPath))
			{
				sprite = GlobalReferences.Instance.KeyboardMouseButtonPrompts.GetButtonPrompt(inputAction, controlPath, text);
			}
			break;
		}
		case DynamicPromptMode.ForceGamepad:
		{
			InputDevice current = Gamepad.current;
			if (current != null)
			{
				int bindingIndexForDeviceWithOverride = GetBindingIndexForDeviceWithOverride(inputAction, current);
				if (bindingIndexForDeviceWithOverride != -1)
				{
					text = inputAction.GetBindingDisplayString(bindingIndexForDeviceWithOverride, out deviceLayoutName, out controlPath);
					if (!string.IsNullOrEmpty(controlPath))
					{
						sprite = GetIconSprite(current, inputAction, controlPath, text);
					}
					else
					{
						flag = true;
					}
				}
				else
				{
					flag = true;
				}
			}
			else
			{
				flag = true;
			}
			break;
		}
		}
		if (flag)
		{
			m_inputIcon.gameObject.SetActive(value: false);
			m_keyboardButton.gameObject.SetActive(value: false);
			return;
		}
		m_inputIcon.gameObject.SetActive(sprite != null);
		m_keyboardButton.gameObject.SetActive(sprite == null);
		if ((bool)sprite)
		{
			m_inputIcon.sprite = sprite;
			if (m_layoutElement != null)
			{
				m_layoutElement.preferredWidth = m_baseWidth;
			}
		}
		else
		{
			m_text.text = text;
			if (m_layoutElement != null)
			{
				m_layoutElement.preferredWidth = m_baseWidth + (float)Mathf.Max(m_text.text.Length - 1, 0) * m_extraWidthPerChar;
			}
		}
	}

	private int GetBindingIndexForDeviceWithOverride(InputAction action, InputDevice device)
	{
		if (m_bindingIndexOverride != -1)
		{
			return m_bindingIndexOverride;
		}
		return GetBindingIndexForDevice(action, device);
	}

	public static int GetBindingIndexForDevice(InputAction action, InputDevice device)
	{
		int num = 0;
		foreach (InputBinding binding in action.bindings)
		{
			if (InputControlPath.TryFindControl(device, binding.effectivePath) != null)
			{
				return num;
			}
			num++;
		}
		return -1;
	}

	public void SetInputAction(InputActionReference inputActionReference)
	{
		SetInputAction(GameInputManager.GameInputActions.FindAction(inputActionReference.action.name));
	}

	public void SetInputAction(InputAction action, int bindingIndex)
	{
		m_bindingIndexOverride = bindingIndex;
		SetInputAction(action);
	}

	public void SetInputAction(InputAction inputAction)
	{
		if (m_inputAction != inputAction)
		{
			m_inputAction = inputAction;
			UpdatePrompt();
		}
	}

	public void SetLabelText(string labelText)
	{
		if (m_label != null)
		{
			m_label.text = labelText;
		}
	}

	public void SetLabelText(LocalizedString localizedString)
	{
		if (m_label != null)
		{
			LocalizeStringEvent component = m_label.GetComponent<LocalizeStringEvent>();
			if (component != null)
			{
				component.StringReference = localizedString;
			}
			else
			{
				Debug.LogError("Input prompt SetLabel is missing LocalizeStringEvent to set string reference directly", this);
			}
		}
	}

	public void RefreshPrompt()
	{
		if (m_inputAction != null)
		{
			UpdatePrompt();
		}
	}
}
