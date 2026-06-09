using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.UI;

public class OptionsTabControlsSub : MonoBehaviour
{
	[Header("UI Prefab")]
	[SerializeField]
	private BindableInputActionEntry m_inputPrefab;

	[Header("Inputs")]
	[SerializeField]
	private BindableInputSettings[] m_settings;

	private BindableInputActionEntry m_firstEntry;

	private List<BindableInputActionEntry> m_entries;

	private void Awake()
	{
		Populate();
	}

	private void OnEnable()
	{
		RefreshEntryVisibility();
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
		RefreshEntryVisibility();
		if (mode == InputState.Mode.Gamepad)
		{
			SetSelectedGameObject();
		}
	}

	private void RefreshEntryVisibility()
	{
		if (m_entries == null)
		{
			return;
		}
		foreach (BindableInputActionEntry entry in m_entries)
		{
			entry.HideUnavailableControls();
		}
	}

	private void Populate()
	{
		m_entries = new List<BindableInputActionEntry>();
		BindableInputSettings[] settings = m_settings;
		foreach (BindableInputSettings settings2 in settings)
		{
			BindableInputActionEntry bindableInputActionEntry = UnityEngine.Object.Instantiate(m_inputPrefab, base.transform);
			bindableInputActionEntry.Configure(settings2);
			if (m_firstEntry == null)
			{
				m_firstEntry = bindableInputActionEntry;
			}
			m_entries.Add(bindableInputActionEntry);
		}
	}

	public void SetSelectedGameObject()
	{
		if (m_firstEntry != null)
		{
			Selectable componentInChildren = m_firstEntry.GetComponentInChildren<Selectable>();
			if (componentInChildren != null)
			{
				EventSystem.current.SetSelectedGameObject(componentInChildren.gameObject);
			}
		}
	}

	public ActionBindingReference GetBindableInputSettingsActionBinding(InputAction action, InputBinding binding, out LocalizedString localisedString)
	{
		BindableInputSettings[] settings = m_settings;
		foreach (BindableInputSettings bindableInputSettings in settings)
		{
			if (bindableInputSettings.m_gamepadAction.m_action != null && bindableInputSettings.m_gamepadAction.m_action.action.name.Equals(action.name) && bindableInputSettings.m_gamepadAction.m_bindingId.Equals(binding.id.ToString()))
			{
				localisedString = bindableInputSettings.m_label;
				return bindableInputSettings.m_gamepadAction;
			}
			if (bindableInputSettings.m_keyboardAction.m_action != null && bindableInputSettings.m_keyboardAction.m_action.action.name.Equals(action.name) && bindableInputSettings.m_keyboardAction.m_bindingId.Equals(binding.id.ToString()))
			{
				localisedString = bindableInputSettings.m_label;
				return bindableInputSettings.m_keyboardAction;
			}
			if (bindableInputSettings.m_keyboardAltAction.m_action != null && bindableInputSettings.m_keyboardAltAction.m_action.action.name.Equals(action.name) && bindableInputSettings.m_keyboardAltAction.m_bindingId.Equals(binding.id.ToString()))
			{
				localisedString = bindableInputSettings.m_label;
				return bindableInputSettings.m_keyboardAltAction;
			}
		}
		localisedString = null;
		return null;
	}

	public BindableInputSettings GetBindableInputSettings(InputAction action, InputBinding binding)
	{
		BindableInputSettings[] settings = m_settings;
		foreach (BindableInputSettings bindableInputSettings in settings)
		{
			if (bindableInputSettings.m_gamepadAction.m_action != null && bindableInputSettings.m_gamepadAction.m_action.action.name.Equals(action.name) && bindableInputSettings.m_gamepadAction.m_bindingId.Equals(binding.id.ToString()))
			{
				return bindableInputSettings;
			}
			if (bindableInputSettings.m_keyboardAction.m_action != null && bindableInputSettings.m_keyboardAction.m_action.action.name.Equals(action.name) && bindableInputSettings.m_keyboardAction.m_bindingId.Equals(binding.id.ToString()))
			{
				return bindableInputSettings;
			}
			if (bindableInputSettings.m_keyboardAltAction.m_action != null && bindableInputSettings.m_keyboardAltAction.m_action.action.name.Equals(action.name) && bindableInputSettings.m_keyboardAltAction.m_bindingId.Equals(binding.id.ToString()))
			{
				return bindableInputSettings;
			}
		}
		return null;
	}
}
