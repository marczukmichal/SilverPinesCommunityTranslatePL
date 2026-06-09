using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;

public class InlineButtonPromptsText : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI m_text;

	public void SetText(string text, InputActionReference[] inputActionReferences)
	{
		string text2 = text;
		InputState inputState = GlobalReferences.Instance.InputState;
		InputDevice inputDevice = null;
		if (inputState.InputMode == InputState.Mode.Gamepad || inputState.InputMode == InputState.Mode.None)
		{
			inputDevice = Gamepad.current;
			if (inputDevice == null)
			{
				inputDevice = InputSystem.AddDevice<Gamepad>();
			}
		}
		else
		{
			inputDevice = Keyboard.current;
			if (inputDevice == null)
			{
				inputDevice = InputSystem.AddDevice<Keyboard>();
			}
		}
		ButtonPromptIconSettings buttonPromptSettingsForDevice = InputPrompt.GetButtonPromptSettingsForDevice(inputDevice);
		if (buttonPromptSettingsForDevice == null || !buttonPromptSettingsForDevice.HasSpriteAsset)
		{
			m_text.spriteAsset = null;
		}
		else
		{
			m_text.spriteAsset = buttonPromptSettingsForDevice.SpriteAsset;
		}
		int num = 0;
		if (inputActionReferences != null)
		{
			foreach (InputActionReference inputActionReference in inputActionReferences)
			{
				InputAction inputAction = GameInputManager.GameInputActions.FindAction(inputActionReference.action.name);
				int num2 = InputPrompt.GetBindingIndexForDevice(inputAction, inputDevice);
				if (inputState.InputMode == InputState.Mode.KeyboardMouse)
				{
					int bindingIndexForDevice = InputPrompt.GetBindingIndexForDevice(inputAction, Mouse.current);
					if (bindingIndexForDevice != -1)
					{
						num2 = bindingIndexForDevice;
					}
				}
				if (num2 != -1)
				{
					string controlPath = "";
					string deviceLayoutName;
					string bindingDisplayString = inputAction.GetBindingDisplayString(num2, out deviceLayoutName, out controlPath);
					if (!string.IsNullOrEmpty(controlPath))
					{
						bool flag = false;
						if (buttonPromptSettingsForDevice.SpriteAsset != null)
						{
							Sprite buttonPrompt = buttonPromptSettingsForDevice.GetButtonPrompt(inputAction, controlPath, bindingDisplayString);
							if (buttonPrompt != null && m_text.spriteAsset.GetSpriteIndexFromName(buttonPrompt.name) != 1)
							{
								flag = true;
								text2 = text2.Replace("{input_" + num + "}", "<sprite name=\"" + buttonPrompt.name + "\">");
							}
						}
						if (!flag)
						{
							text2 = text2.Replace("{input_" + num + "}", "<b><color=white>[" + bindingDisplayString + "]</color></b>");
						}
					}
				}
				else
				{
					string localizedString = new LocalizedString("UICommon", "Input_Unbound").GetLocalizedString();
					text2 = text2.Replace("{input_" + num + "}", "<b><color=red>[" + localizedString + "]</color></b>");
				}
				num++;
			}
		}
		m_text.text = text2;
	}
}
