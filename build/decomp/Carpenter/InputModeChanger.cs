using System;
using UnityEngine;
using UnityEngine.Events;

public class InputModeChanger : MonoBehaviour
{
	[SerializeField]
	private GameObject m_gamepadInputPrompt;

	[SerializeField]
	private GameObject m_keyboardInputPrompt;

	private void OnEnable()
	{
		UpdateShownInputPrompt();
		InputState inputState = GlobalReferences.Instance.InputState;
		inputState.OnInputModeChanged = (UnityAction<InputState.Mode>)Delegate.Combine(inputState.OnInputModeChanged, new UnityAction<InputState.Mode>(OnInputModeChanged));
	}

	private void OnDisable()
	{
		InputState inputState = GlobalReferences.Instance.InputState;
		inputState.OnInputModeChanged = (UnityAction<InputState.Mode>)Delegate.Remove(inputState.OnInputModeChanged, new UnityAction<InputState.Mode>(OnInputModeChanged));
	}

	private void OnInputModeChanged(InputState.Mode mode)
	{
		UpdateShownInputPrompt();
	}

	private void UpdateShownInputPrompt()
	{
		bool flag = GlobalReferences.Instance.InputState.InputMode == InputState.Mode.Gamepad;
		m_gamepadInputPrompt.SetActive(flag);
		m_keyboardInputPrompt.SetActive(!flag);
	}
}
