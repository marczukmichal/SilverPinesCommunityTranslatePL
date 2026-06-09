using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;

public class GameInputManager : MonoBehaviour
{
	public enum ActiveActionMapMode
	{
		None,
		PlayerControl,
		GameUI,
		DebugConsole,
		InputField
	}

	[SerializeField]
	private GameInputManagerAnchor m_anchor;

	[SerializeField]
	private InputState m_inputState;

	[SerializeField]
	private InputActionAsset m_actionAsset;

	[SerializeField]
	private GameMenuState m_gameMenuState;

	private InputControlScheme m_keyboardControlScheme;

	private InputControlScheme m_gamepadControlScheme;

	private InputUser m_mainPlayerInputUser;

	private InputUser m_compaionPlayerInputUser;

	private ActiveActionMapMode m_actionMapMode;

	private static GameInputActions m_gameInputActions;

	private static CompanionInputActions m_companionInputActions;

	private int m_inDebugMenuCounter;

	private int m_activeInputFieldMenuCounter;

	private InputDevice m_mainPlayerDevice;

	private InputDevice m_companionPlayerDevice;

	public UnityAction<bool> OnCompanionPlayerControlledStateChanged;

	[DebugCommand("only_gamepad", "Only use gamepad controller devices, ignore keyboard and mouse", "only_gamepad <true/false>", typeof(bool), false)]
	private static bool m_only_gamepad_override;

	private bool m_isOnlyGamepadMode;

	private static List<IInputBackHandler> m_inputBackHandlers;

	private ControllerRumbleManager m_rumbleManager;

	private CursorManager m_cursorManager;

	public InputUser MainPlayerInputUser => m_mainPlayerInputUser;

	public InputUser CompanionPlayerInputUser => m_compaionPlayerInputUser;

	public ActiveActionMapMode ActionMapMode => m_actionMapMode;

	public static GameInputActions GameInputActions => m_gameInputActions;

	public static CompanionInputActions CompanionInputActions => m_companionInputActions;

	public bool IsCompanionPlayerControlled => m_companionPlayerDevice != null;

	private static bool ONLY_GAMEPAD => m_only_gamepad_override;

	public static void PushBackInputHandler(IInputBackHandler handler)
	{
		m_inputBackHandlers.Insert(0, handler);
	}

	public static void RemoveBackInputHandler(IInputBackHandler handler)
	{
		m_inputBackHandlers.Remove(handler);
	}

	private void Awake()
	{
		m_isOnlyGamepadMode = ONLY_GAMEPAD;
		m_rumbleManager = new ControllerRumbleManager();
		m_cursorManager = GetComponent<CursorManager>();
		m_gameInputActions = new GameInputActions();
		m_gameInputActions.Game.Enable();
		m_gameInputActions.MenuToggles.Enable();
		m_gameInputActions.Debug.Enable();
		m_companionInputActions = new CompanionInputActions();
		m_companionInputActions.Companion.Enable();
		SetActionMapMode(ActiveActionMapMode.PlayerControl);
		m_mainPlayerInputUser = InputUser.CreateUserWithoutPairedDevices();
		m_mainPlayerInputUser.AssociateActionsWithUser(m_gameInputActions);
		m_compaionPlayerInputUser = InputUser.CreateUserWithoutPairedDevices();
		m_compaionPlayerInputUser.AssociateActionsWithUser(m_companionInputActions);
		InputUser.listenForUnpairedDeviceActivity = 1;
		InputUser.onUnpairedDeviceUsed += InputUnpairedDeviceUsed;
		InputSystem.onDeviceChange += InputSystemOnDeviceChanged;
		m_keyboardControlScheme = m_actionAsset.FindControlScheme("Keyboard&Mouse").Value;
		m_gamepadControlScheme = m_actionAsset.FindControlScheme("Gamepad").Value;
		m_inputBackHandlers = new List<IInputBackHandler>();
		m_gameInputActions.UI.Cancel.performed += OnBackInput;
		UserPreferences userPreferences = GlobalReferences.Instance.UserPreferences;
		if (userPreferences != null && !string.IsNullOrEmpty(userPreferences.InputJson))
		{
			GameInputActions.LoadBindingOverridesFromJson(userPreferences.InputJson);
		}
	}

	private void OnDestroy()
	{
		m_mainPlayerInputUser.UnpairDevices();
		m_compaionPlayerInputUser.UnpairDevices();
		InputSystem.onDeviceChange -= InputSystemOnDeviceChanged;
		InputUser.listenForUnpairedDeviceActivity = 0;
		InputUser.onUnpairedDeviceUsed -= InputUnpairedDeviceUsed;
		m_gameInputActions.Dispose();
		m_gameInputActions = null;
		m_companionInputActions.Dispose();
		m_companionInputActions = null;
	}

	private void OnEnable()
	{
		DebugConsole.m_onConsoleWindowOpened = (UnityAction<bool>)Delegate.Combine(DebugConsole.m_onConsoleWindowOpened, new UnityAction<bool>(SetDebugConsoleOpen));
		m_anchor.Set(this);
		InputState inputState = m_inputState;
		inputState.OnInputModeChanged = (UnityAction<InputState.Mode>)Delegate.Combine(inputState.OnInputModeChanged, new UnityAction<InputState.Mode>(InputModeChanged));
		GameMenuState gameMenuState = m_gameMenuState;
		gameMenuState.OnGameMenuChanged = (UnityAction<GameMenuState.GameMenu>)Delegate.Combine(gameMenuState.OnGameMenuChanged, new UnityAction<GameMenuState.GameMenu>(SetInMenu));
		GlobalReferences.Instance.Anchors.Input.CursorOverridesAnchor.Register(CursorOverrideChanged);
		GlobalReferences.Instance.EventChannels.Generic.CameraShake.Register(OnCameraShakeEvent);
		GlobalReferences.Instance.EventChannels.Generic.ControllerRumble.Register(OnControllerRumbleEvent);
	}

	private void OnDisable()
	{
		DebugConsole.m_onConsoleWindowOpened = (UnityAction<bool>)Delegate.Remove(DebugConsole.m_onConsoleWindowOpened, new UnityAction<bool>(SetDebugConsoleOpen));
		m_anchor.Set(null);
		InputState inputState = m_inputState;
		inputState.OnInputModeChanged = (UnityAction<InputState.Mode>)Delegate.Remove(inputState.OnInputModeChanged, new UnityAction<InputState.Mode>(InputModeChanged));
		GameMenuState gameMenuState = m_gameMenuState;
		gameMenuState.OnGameMenuChanged = (UnityAction<GameMenuState.GameMenu>)Delegate.Remove(gameMenuState.OnGameMenuChanged, new UnityAction<GameMenuState.GameMenu>(SetInMenu));
		GlobalReferences.Instance.Anchors.Input.CursorOverridesAnchor.Unregister(CursorOverrideChanged);
		GlobalReferences.Instance.EventChannels.Generic.CameraShake.Unregister(OnCameraShakeEvent);
		GlobalReferences.Instance.EventChannels.Generic.ControllerRumble.Unregister(OnControllerRumbleEvent);
	}

	private void Update()
	{
		if (m_isOnlyGamepadMode != ONLY_GAMEPAD)
		{
			m_isOnlyGamepadMode = ONLY_GAMEPAD;
			UnpairEverything();
		}
		if (m_inputState.InputMode == InputState.Mode.Gamepad)
		{
			m_rumbleManager.Update();
		}
	}

	private void InputUnpairedDeviceUsed(InputControl control, InputEventPtr eventPtr)
	{
		PairWithDevice(control.device, pairForCompanion: false, allowDeviceSwap: true);
	}

	private void InputSystemOnDeviceChanged(InputDevice device, InputDeviceChange changeType)
	{
		if (!(device is Mouse))
		{
			switch (changeType)
			{
			case InputDeviceChange.Added:
				PairWithDevice(device, pairForCompanion: false, !GlobalReferences.Instance.GameState.IsInState(GameState.GameStateFlag.GameActive));
				break;
			case InputDeviceChange.Removed:
				CheckForReset(device);
				break;
			}
		}
	}

	private void CheckForReset(InputDevice lostDevice)
	{
		bool flag = false;
		foreach (InputDevice pairedDevice in m_mainPlayerInputUser.pairedDevices)
		{
			if (pairedDevice == lostDevice)
			{
				flag = true;
				break;
			}
		}
		if (m_mainPlayerDevice == lostDevice)
		{
			m_mainPlayerDevice = null;
		}
		if (m_companionPlayerDevice == lostDevice)
		{
			m_compaionPlayerInputUser.UnpairDevices();
			m_companionPlayerDevice = null;
			OnCompanionPlayerControlledStateChanged?.Invoke(arg0: false);
		}
		if (flag)
		{
			PairWithDevice(Keyboard.current, pairForCompanion: false, allowDeviceSwap: true);
		}
	}

	private void PairWithDevice(InputDevice device, bool pairForCompanion, bool allowDeviceSwap)
	{
		if ((ONLY_GAMEPAD && (device is Keyboard || device is Mouse)) || (!(device is Keyboard) && !(device is Mouse) && !(device is Gamepad)))
		{
			return;
		}
		if (m_mainPlayerDevice == null || allowDeviceSwap)
		{
			Debug.Log("Paired main player with device: " + device);
			m_mainPlayerDevice = device;
			m_mainPlayerInputUser.UnpairDevices();
			m_mainPlayerInputUser = InputUser.PerformPairingWithDevice(device, m_mainPlayerInputUser);
			InputState.Mode inputMode = InputState.Mode.None;
			InputControlScheme scheme = default(InputControlScheme);
			if (device is Keyboard)
			{
				m_mainPlayerInputUser = InputUser.PerformPairingWithDevice(Mouse.current, m_mainPlayerInputUser);
				inputMode = InputState.Mode.KeyboardMouse;
				scheme = m_keyboardControlScheme;
			}
			else if (device is Mouse)
			{
				m_mainPlayerInputUser = InputUser.PerformPairingWithDevice(Keyboard.current, m_mainPlayerInputUser);
				inputMode = InputState.Mode.KeyboardMouse;
				scheme = m_keyboardControlScheme;
			}
			else if (device is Gamepad)
			{
				inputMode = InputState.Mode.Gamepad;
				scheme = m_gamepadControlScheme;
			}
			m_inputState.InputMode = inputMode;
			m_mainPlayerInputUser.ActivateControlScheme(scheme);
		}
		else if (m_companionPlayerDevice == null && pairForCompanion)
		{
			Debug.Log("Paired companion player with device: " + device);
			m_companionPlayerDevice = device;
			m_compaionPlayerInputUser.UnpairDevices();
			m_compaionPlayerInputUser = InputUser.PerformPairingWithDevice(device, m_compaionPlayerInputUser);
			OnCompanionPlayerControlledStateChanged?.Invoke(arg0: true);
		}
	}

	private void SetActionMapMode(ActiveActionMapMode mode)
	{
		if (m_actionMapMode == mode)
		{
			return;
		}
		StopAllCoroutines();
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		bool flag5 = false;
		bool flag6 = false;
		bool flag7 = false;
		bool flag8 = false;
		switch (m_actionMapMode)
		{
		case ActiveActionMapMode.PlayerControl:
			flag = true;
			flag2 = true;
			flag3 = false;
			flag4 = true;
			break;
		case ActiveActionMapMode.GameUI:
			flag = true;
			flag2 = false;
			flag3 = true;
			break;
		}
		m_actionMapMode = mode;
		switch (mode)
		{
		case ActiveActionMapMode.PlayerControl:
			flag5 = true;
			flag6 = true;
			flag8 = true;
			break;
		case ActiveActionMapMode.GameUI:
			flag5 = true;
			flag7 = true;
			break;
		}
		if (flag != flag5)
		{
			if (flag5)
			{
				m_gameInputActions.Game.Enable();
				m_gameInputActions.MenuToggles.Enable();
			}
			else
			{
				m_gameInputActions.Game.Disable();
				m_gameInputActions.MenuToggles.Disable();
			}
		}
		if (flag2 != flag6)
		{
			if (flag6)
			{
				StartCoroutine(EnableGameInputDelayed());
			}
			else
			{
				m_gameInputActions.Player.Disable();
			}
		}
		if (flag3 != flag7)
		{
			if (flag7)
			{
				m_gameInputActions.UI.Enable();
				m_gameInputActions.Minigame.Enable();
			}
			else
			{
				m_gameInputActions.UI.Disable();
				m_gameInputActions.Minigame.Disable();
			}
		}
		if (flag4 != flag8)
		{
			if (flag8)
			{
				m_companionInputActions.Companion.Enable();
			}
			else
			{
				m_companionInputActions.Companion.Disable();
			}
		}
		m_cursorManager.UpdateCursorState();
	}

	private IEnumerator EnableGameInputDelayed()
	{
		m_gameInputActions.Player.Enable();
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		StartCoroutine(GameUtils.TempBlockInputActionIfPressed(m_gameInputActions.Player.Dodge));
		StartCoroutine(GameUtils.TempBlockInputActionIfPressed(m_gameInputActions.Player.Jump));
		StartCoroutine(GameUtils.TempBlockInputActionIfPressed(m_gameInputActions.Player.Interact));
	}

	public void SetDebugConsoleOpen(bool open)
	{
		if (open)
		{
			m_inDebugMenuCounter++;
		}
		else
		{
			m_inDebugMenuCounter--;
		}
		if (m_inDebugMenuCounter < 0)
		{
			Debug.LogError("Incorrect usage of SetDebugConsoleOpen. SetDebugConsoleOpen(false) was called without a matching open call!");
			m_inDebugMenuCounter = 0;
		}
		UpdateActionMap();
	}

	public void SetInputFieldActive(bool active)
	{
		if (active)
		{
			m_activeInputFieldMenuCounter++;
		}
		else
		{
			m_activeInputFieldMenuCounter--;
		}
		if (m_activeInputFieldMenuCounter < 0)
		{
			Debug.LogError("Incorrect usage of SetInputFieldActive. SetInputFieldActive(false) was called without a matching open call!");
			m_activeInputFieldMenuCounter = 0;
		}
		UpdateActionMap();
	}

	public void SetInMenu(GameMenuState.GameMenu menu)
	{
		UpdateActionMap();
		m_cursorManager.UpdateCursorState();
	}

	private void UpdateActionMap()
	{
		ActiveActionMapMode actionMapMode = ActiveActionMapMode.PlayerControl;
		if (m_inDebugMenuCounter > 0)
		{
			actionMapMode = ActiveActionMapMode.DebugConsole;
		}
		else if (m_activeInputFieldMenuCounter > 0)
		{
			actionMapMode = ActiveActionMapMode.InputField;
		}
		else if (m_gameMenuState.IsInAnyMenu())
		{
			actionMapMode = ActiveActionMapMode.GameUI;
		}
		SetActionMapMode(actionMapMode);
	}

	private void CursorOverrideChanged(ICursorOverrides arg0)
	{
		m_cursorManager.UpdateCursorState();
	}

	private void InputModeChanged(InputState.Mode newMode)
	{
		m_cursorManager.UpdateCursorState();
	}

	private void OnBackInput(InputAction.CallbackContext inputCallback)
	{
		if (inputCallback.performed && m_inputBackHandlers.Count > 0 && m_inputBackHandlers[0].OnInputBack())
		{
			m_inputBackHandlers.RemoveAt(0);
		}
	}

	[DebugCommand("unpair_input_devices", "Unpairs all input devices", "unpair_input_devices", null, false)]
	private void UnpairEverything()
	{
		Debug.Log("Unpairing all input devices");
		m_mainPlayerInputUser.UnpairDevices();
		m_compaionPlayerInputUser.UnpairDevices();
		m_mainPlayerDevice = null;
		m_companionPlayerDevice = null;
	}

	private void OnCameraShakeEvent(CameraShakeEventData shakeData)
	{
		if (shakeData.m_cameraShakeSettings.UseRumble)
		{
			m_rumbleManager.Rumble(shakeData.m_cameraShakeSettings.Rumble, shakeData.m_position);
		}
	}

	private void OnControllerRumbleEvent(ControllerRumbleEventData rumbleData)
	{
		m_rumbleManager.Rumble(rumbleData.m_controllerRumbleSettings, rumbleData.m_position);
	}
}
