using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;

public class OptionsTabControls : OptionsMenuTab
{
	[Header("Tab Buttons")]
	[SerializeField]
	private MenuTabButton[] m_tabButtons;

	[Header("Sub Tabs")]
	[SerializeField]
	private OptionsTabControlsSub[] m_controlsTabs;

	[Header("Binding Overlay")]
	[SerializeField]
	private RebindingInputOverlayPopup m_rebindingOverlay;

	[SerializeField]
	private RebindingWarningPopup m_warningPopup;

	private int m_currentActiveTab = -1;

	private float m_blockTabInputTime;

	private InputActionRebindingExtensions.RebindingOperation m_rebindOperation;

	protected override void PopulateOptionsTab()
	{
		base.PopulateOptionsTab();
	}

	protected override void Awake()
	{
		base.Awake();
		for (int i = 0; i < m_controlsTabs.Length; i++)
		{
			m_controlsTabs[i].gameObject.SetActive(value: false);
		}
		SetActiveTab(0);
	}

	private void OnEnable()
	{
		GameInputManager.GameInputActions.UI.SecondaryTabLeft.performed += PerformTabLeft;
		GameInputManager.GameInputActions.UI.SecondaryTabRight.performed += PerformTabRight;
		GlobalReferences.Instance.EventChannels.Input.OnRequestRebindGamepadInputAction.Register(OnGamepadInputStartBinding);
		GlobalReferences.Instance.EventChannels.Input.OnRequestRebindKeyboardInputAction.Register(OnKeyboardInputStartBinding);
		GlobalReferences.Instance.EventChannels.Input.OnRequestRebindKeyboardAltInputAction.Register(OnKeyboardAltInputStartBinding);
		GlobalReferences.Instance.EventChannels.Input.OnRequestResetInputAction.Register(OnResetInputBinding);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		m_rebindOperation?.Cancel();
		if (GameInputManager.GameInputActions != null)
		{
			GameInputManager.GameInputActions.UI.SecondaryTabLeft.performed -= PerformTabLeft;
			GameInputManager.GameInputActions.UI.SecondaryTabRight.performed -= PerformTabRight;
		}
		GlobalReferences.Instance.EventChannels.Input.OnRequestRebindGamepadInputAction.Unregister(OnGamepadInputStartBinding);
		GlobalReferences.Instance.EventChannels.Input.OnRequestRebindKeyboardInputAction.Unregister(OnKeyboardInputStartBinding);
		GlobalReferences.Instance.EventChannels.Input.OnRequestRebindKeyboardAltInputAction.Unregister(OnKeyboardAltInputStartBinding);
		GlobalReferences.Instance.EventChannels.Input.OnRequestResetInputAction.Unregister(OnResetInputBinding);
	}

	private void OnDestroy()
	{
		if (m_rebindOperation != null)
		{
			m_rebindOperation.Dispose();
			m_rebindOperation = null;
		}
	}

	private bool TabInputBlocked()
	{
		return Time.unscaledTime < m_blockTabInputTime;
	}

	private void PerformTabLeft(InputAction.CallbackContext context)
	{
		if (context.performed && !TabInputBlocked())
		{
			int num = m_currentActiveTab - 1;
			if (num < 0)
			{
				num = m_controlsTabs.Length - 1;
			}
			SetActiveTab(num);
		}
	}

	private void PerformTabRight(InputAction.CallbackContext context)
	{
		if (context.performed && !TabInputBlocked())
		{
			int num = m_currentActiveTab + 1;
			if (num >= m_controlsTabs.Length)
			{
				num = 0;
			}
			SetActiveTab(num);
		}
	}

	public void SetActiveTab(int index)
	{
		if (m_currentActiveTab != index)
		{
			if (m_currentActiveTab != -1)
			{
				m_tabButtons[m_currentActiveTab].SetActive(active: false);
				m_controlsTabs[m_currentActiveTab].gameObject.SetActive(value: false);
			}
			m_currentActiveTab = index;
			m_tabButtons[m_currentActiveTab].SetActive(active: true);
			m_controlsTabs[m_currentActiveTab].gameObject.SetActive(value: true);
			m_controlsTabs[m_currentActiveTab].SetSelectedGameObject();
		}
	}

	private void OnKeyboardInputStartBinding(BindableInputActionEntry inputActionEntry)
	{
		Debug.Log("Start keyboard binding for: " + inputActionEntry.Settings.m_keyboardAction.m_action);
		StartInteractiveRebind(inputActionEntry, inputActionEntry.Settings.m_keyboardAction);
	}

	private void OnKeyboardAltInputStartBinding(BindableInputActionEntry inputActionEntry)
	{
		Debug.Log("Start keyboard alt binding for: " + inputActionEntry.Settings.m_keyboardAltAction.m_action);
		StartInteractiveRebind(inputActionEntry, inputActionEntry.Settings.m_keyboardAltAction);
	}

	private void OnGamepadInputStartBinding(BindableInputActionEntry inputActionEntry)
	{
		Debug.Log("Start gamepad binding for: " + inputActionEntry.Settings.m_gamepadAction.m_action);
		StartInteractiveRebind(inputActionEntry, inputActionEntry.Settings.m_gamepadAction);
	}

	private void OnResetInputBinding(BindableInputActionEntry inputActionEntry)
	{
		if (inputActionEntry.Settings.m_gamepadAction.m_action != null)
		{
			ResolveActionAndBinding(inputActionEntry.Settings.m_gamepadAction, out var action, out var bindingIndex);
			if (action != null)
			{
				action.RemoveBindingOverride(bindingIndex);
				if (!inputActionEntry.Settings.m_allowDuplicateBinding)
				{
					CheckForDuplicates(action, bindingIndex);
				}
			}
		}
		if (inputActionEntry.Settings.m_keyboardAction.m_action != null)
		{
			ResolveActionAndBinding(inputActionEntry.Settings.m_keyboardAction, out var action2, out var bindingIndex2);
			if (action2 != null)
			{
				action2.RemoveBindingOverride(bindingIndex2);
				if (!inputActionEntry.Settings.m_allowDuplicateBinding)
				{
					CheckForDuplicates(action2, bindingIndex2);
				}
			}
		}
		if (inputActionEntry.Settings.m_keyboardAltAction.m_action != null)
		{
			ResolveActionAndBinding(inputActionEntry.Settings.m_keyboardAltAction, out var action3, out var bindingIndex3);
			if (action3 != null)
			{
				action3.RemoveBindingOverride(bindingIndex3);
				if (!inputActionEntry.Settings.m_allowDuplicateBinding)
				{
					CheckForDuplicates(action3, bindingIndex3);
				}
			}
		}
		inputActionEntry.Refresh();
	}

	private bool ResolveActionAndBinding(ActionBindingReference bindingReference, out InputAction action, out int bindingIndex)
	{
		bindingIndex = -1;
		action = bindingReference.m_action?.action;
		if (action == null)
		{
			return false;
		}
		action = GameInputManager.GameInputActions.FindAction(action.name);
		if (action == null)
		{
			return false;
		}
		if (string.IsNullOrEmpty(bindingReference.m_bindingId))
		{
			return false;
		}
		Guid bindingId = new Guid(bindingReference.m_bindingId);
		bindingIndex = action.bindings.IndexOf((InputBinding x) => x.id == bindingId);
		if (bindingIndex == -1)
		{
			Debug.LogError($"Cannot find binding with ID '{bindingId}' on '{action}'", this);
			return false;
		}
		return true;
	}

	private void StartInteractiveRebind(BindableInputActionEntry entry, ActionBindingReference bindingReference)
	{
		if (!ResolveActionAndBinding(bindingReference, out var action, out var bindingIndex))
		{
			return;
		}
		if (action.bindings[bindingIndex].isComposite)
		{
			int num = bindingIndex + 1;
			if (num < action.bindings.Count && action.bindings[num].isPartOfComposite)
			{
				PerformInteractiveRebind(entry, action, num, allCompositeParts: true);
			}
		}
		else
		{
			PerformInteractiveRebind(entry, action, bindingIndex);
		}
	}

	private ActionBindingReference GetBindableInputSettingsForActionBinding(InputAction action, InputBinding binding, out LocalizedString localisedString)
	{
		OptionsTabControlsSub[] controlsTabs = m_controlsTabs;
		for (int i = 0; i < controlsTabs.Length; i++)
		{
			ActionBindingReference bindableInputSettingsActionBinding = controlsTabs[i].GetBindableInputSettingsActionBinding(action, binding, out localisedString);
			if (bindableInputSettingsActionBinding != null)
			{
				return bindableInputSettingsActionBinding;
			}
		}
		localisedString = null;
		return null;
	}

	private BindableInputSettings GetBindableInputSettings(InputAction action, InputBinding binding)
	{
		OptionsTabControlsSub[] controlsTabs = m_controlsTabs;
		for (int i = 0; i < controlsTabs.Length; i++)
		{
			BindableInputSettings bindableInputSettings = controlsTabs[i].GetBindableInputSettings(action, binding);
			if (bindableInputSettings != null)
			{
				return bindableInputSettings;
			}
		}
		return null;
	}

	private bool CheckForDuplicatesInActionMap(InputAction action, InputBinding newBinding, InputActionMap actionMap)
	{
		bool result = false;
		foreach (InputBinding binding in actionMap.bindings)
		{
			if (binding.id == newBinding.id || !(binding.effectivePath == newBinding.effectivePath))
			{
				continue;
			}
			InputAction inputAction = GameInputManager.GameInputActions.FindAction(binding.action);
			LocalizedString localisedString;
			ActionBindingReference bindableInputSettingsForActionBinding = GetBindableInputSettingsForActionBinding(inputAction, binding, out localisedString);
			BindableInputSettings bindableInputSettings = GetBindableInputSettings(inputAction, binding);
			if (bindableInputSettingsForActionBinding != null && !bindableInputSettingsForActionBinding.m_isLockedControls && !bindableInputSettings.m_allowDuplicateBinding)
			{
				for (int i = 0; i < inputAction.bindings.Count; i++)
				{
					if (inputAction.bindings[i].id == binding.id)
					{
						m_warningPopup.WarnForClearedBinding(localisedString);
						inputAction.ApplyBindingOverride(i, "");
					}
				}
				Debug.Log("Duplicate binding found against " + binding.action + " name " + localisedString.GetLocalizedString());
				result = true;
			}
			else
			{
				Debug.Log("Found duplicate binding but not unbinding as it is not user-editable input: " + binding.action);
			}
		}
		return result;
	}

	private bool CheckForDuplicates(InputAction action, int bindingIndex)
	{
		InputBinding newBinding = action.bindings[bindingIndex];
		if (string.IsNullOrEmpty(newBinding.effectivePath))
		{
			return false;
		}
		InputActionMap inputActionMap = GameInputManager.GameInputActions.Player.Get();
		InputActionMap inputActionMap2 = GameInputManager.GameInputActions.MenuToggles.Get();
		bool flag = CheckForDuplicatesInActionMap(action, newBinding, action.actionMap);
		if (action.actionMap == inputActionMap)
		{
			if (CheckForDuplicatesInActionMap(action, newBinding, inputActionMap2))
			{
				flag = true;
			}
		}
		else if (action.actionMap == inputActionMap2 && CheckForDuplicatesInActionMap(action, newBinding, inputActionMap))
		{
			flag = true;
		}
		if (flag)
		{
			BindableInputActionEntry[] componentsInChildren = GetComponentsInChildren<BindableInputActionEntry>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].Refresh();
			}
		}
		return flag;
	}

	private void CleanUpAfterRebind(InputAction action)
	{
		m_rebindOperation?.Dispose();
		m_rebindOperation = null;
		action.Enable();
		m_blockTabInputTime = Time.unscaledTime + 1f;
	}

	private void PerformInteractiveRebind(BindableInputActionEntry entry, InputAction action, int bindingIndex, bool allCompositeParts = false)
	{
		m_rebindOperation?.Cancel();
		action.Disable();
		float num = 3f;
		InputBinding inputBinding = action.bindings[bindingIndex];
		m_rebindOperation = action.PerformInteractiveRebinding(bindingIndex).WithTargetBinding(bindingIndex).WithMatchingEventsBeingSuppressed()
			.WithControlsExcluding("/leftStick/up")
			.WithControlsExcluding("/leftStick/down")
			.WithControlsExcluding("/leftStick/right")
			.WithControlsExcluding("/leftStick/left")
			.WithControlsExcluding("/leftStick")
			.WithControlsExcluding("/rightStick/up")
			.WithControlsExcluding("/rightStick/down")
			.WithControlsExcluding("/rightStick/right")
			.WithControlsExcluding("/rightStick/left")
			.WithControlsExcluding("/rightStick")
			.WithControlsExcluding("<Mouse>/delta")
			.WithCancelingThrough("<Keyboard>/escape")
			.WithTimeout(num)
			.OnPotentialMatch(PotentialMatchCallback)
			.OnCancel(delegate
			{
				m_rebindingOverlay.Hide();
				CleanUpAfterRebind(action);
			})
			.OnComplete(delegate(InputActionRebindingExtensions.RebindingOperation operation)
			{
				_ = operation.selectedControl.path;
				_ = action.bindings[bindingIndex].effectivePath;
				m_rebindingOverlay.Hide();
				CleanUpAfterRebind(action);
				if (!entry.Settings.m_allowDuplicateBinding)
				{
					CheckForDuplicates(action, bindingIndex);
				}
				GlobalReferences.Instance.UserPreferences.SetDirtyFlag();
				entry.Refresh();
				if (allCompositeParts)
				{
					int num2 = bindingIndex + 1;
					if (num2 < action.bindings.Count && action.bindings[num2].isPartOfComposite)
					{
						PerformInteractiveRebind(entry, action, num2, allCompositeParts: true);
					}
				}
			});
		if (!string.IsNullOrEmpty(inputBinding.effectivePath))
		{
			m_rebindOperation.WithControlsExcluding(inputBinding.effectivePath);
		}
		m_rebindingOverlay.ShowForRebind(entry, action, bindingIndex, num);
		m_rebindOperation.Start();
	}

	private bool IsAllowedControl(InputControl control)
	{
		if (control.name.Contains("anyKey"))
		{
			return false;
		}
		return true;
	}

	private void PotentialMatchCallback(InputActionRebindingExtensions.RebindingOperation operation)
	{
		List<InputControl> list = new List<InputControl>();
		for (int i = 0; i < operation.candidates.Count; i++)
		{
			InputControl inputControl = operation.candidates[i];
			if (!IsAllowedControl(inputControl))
			{
				list.Add(inputControl);
			}
		}
		foreach (InputControl item in list)
		{
			operation.RemoveCandidate(item);
		}
		if (operation.candidates.Count == 0)
		{
			operation.Cancel();
		}
	}
}
