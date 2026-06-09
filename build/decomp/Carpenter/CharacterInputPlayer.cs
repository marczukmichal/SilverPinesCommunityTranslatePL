using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class CharacterInputPlayer : BaseCharacterInput, GameInputActions.IPlayerActions
{
	public enum ForceAimMode
	{
		None,
		ForceRight,
		ForceLeft
	}

	[SerializeField]
	private float m_jumpWindow = 0.1f;

	[SerializeField]
	private Transform m_aimCenterTransform;

	[SerializeField]
	private BoolGameEventChannel m_disableInputChannel;

	[SerializeField]
	private PlayerInputOverrideEventChannel m_playerInputOverrideChannel;

	[SerializeField]
	private InputState m_inputState;

	[SerializeField]
	private GameMenuState m_gameMenuState;

	[SerializeField]
	private MouseAimInputData m_mouseAimInputData;

	[Tooltip("This is a scalar based on Screen.Width")]
	[SerializeField]
	private float m_mouseAimRadius = 0.2f;

	[Tooltip("This is a scalar based on Screen.Width")]
	[SerializeField]
	private float m_mouseAimMinDistance = 0.05f;

	[DebugCommand("always_aim_input", "Force aim input always", "always_aim_input <true/false>", typeof(bool), false)]
	private static bool ALWAYS_AIM_INPUT;

	[DebugCommand("always_move_right_input", "Always move right input", "always_move_right_input <true/false>", typeof(bool), false)]
	private static bool ALWAYS_MOVE_RIGHT_INPUT;

	[DebugCommand("always_move_left_input", "Always move left input", "always_move_left_input <true/false>", typeof(bool), false)]
	private static bool ALWAYS_MOVE_LEFT_INPUT;

	[DebugCommand("always_move_circle", "Keeps moving left and right", "always_move_circle <true/false>", typeof(bool), false)]
	private static bool ALWAYS_MOVE_CIRCLE;

	[Header("Shortcuts")]
	[SerializeField]
	private ItemDefinition m_flashlightItem;

	[SerializeField]
	private ItemDefinition m_cameraItem;

	[SerializeField]
	private ItemDefinition m_digItem;

	[Header("Aiming Sensitivity")]
	[SerializeField]
	private float m_gamepadAimingSensitivity = 1f;

	private CharacterDirection m_characterDirection;

	private CharacterAiming m_characterAiming;

	private CharacterInventory m_characterInventory;

	private CharacterMovement m_characterMovement;

	private bool m_isCrouching;

	private bool m_isSprintButtonDown;

	private bool m_isSprinting;

	private Vector2 m_movementInput;

	private bool m_requestedJump;

	private Coroutine m_jumpCoroutine;

	private bool m_isFiring;

	private bool m_isAiming;

	private ForceAimMode m_forceAimingInput;

	private bool m_isThrowAiming;

	private Vector2 m_aimDirection;

	private Vector2 m_gamepadRawAimInput;

	private bool m_isUsingQuickMap;

	private bool m_inputTempDisable;

	private bool m_inputPaused;

	private bool m_itemWheelOpen;

	private float m_inputBlockTimer;

	private bool m_isTryingToShowQuickItemPanel;

	private PlayerInputOverride m_inputOverride;

	private float m_parryInputTime;

	public UnityAction OnRequestItemWheel;

	public UnityAction OnRequestCloseItemWheel;

	public UnityAction<bool> OnRequestQuickItemPanel;

	public UnityAction OnCycleNextWeapon;

	public UnityAction OnCyclePreviousWeapon;

	private Vector2 m_mouseAimPosition;

	private bool m_turnDisabled;

	public override bool IsCrouching => m_isCrouching;

	public override bool IsSprinting
	{
		get
		{
			if (!IsSprintAllowed())
			{
				return false;
			}
			switch (m_inputOverride)
			{
			case PlayerInputOverride.WalkRight:
			case PlayerInputOverride.WalkLeft:
				return false;
			case PlayerInputOverride.SprintRight:
			case PlayerInputOverride.SprintLeft:
				return true;
			default:
				return m_isSprinting;
			}
		}
	}

	public override Vector2 MovementInput
	{
		get
		{
			if (m_characterInventory != null && (m_characterInventory.IsShowingItemWheel || m_characterInventory.IsShowingQuickItemPanel))
			{
				return Vector2.zero;
			}
			switch (m_inputOverride)
			{
			case PlayerInputOverride.WalkLeft:
			case PlayerInputOverride.SprintLeft:
				return new Vector2(-1f, 0f);
			case PlayerInputOverride.WalkRight:
			case PlayerInputOverride.SprintRight:
				return new Vector2(1f, 0f);
			case PlayerInputOverride.Static:
				return Vector2.zero;
			case PlayerInputOverride.Up:
				return new Vector2(0f, 1f);
			case PlayerInputOverride.Down:
				return new Vector2(0f, -1f);
			default:
				if (GameCutsceneManager.CutsceneActive)
				{
					return Vector2.zero;
				}
				if (ALWAYS_MOVE_RIGHT_INPUT)
				{
					return new Vector2(1f, 0f);
				}
				if (ALWAYS_MOVE_LEFT_INPUT)
				{
					return new Vector2(-1f, 0f);
				}
				if (m_movementInput == Vector2.zero && ALWAYS_MOVE_CIRCLE)
				{
					if (Time.time % 10f < 5f)
					{
						return new Vector2(1f, 0f);
					}
					return new Vector2(-1f, 0f);
				}
				if (m_characterMovement.AttachedStairs != null)
				{
					Stairs attachedStairs = m_characterMovement.AttachedStairs;
					if (m_movementInput.y > GameUtils.Constants.s_inputMoveDeadzoneMinValue)
					{
						if (attachedStairs.UpDirection.x > 0f)
						{
							return new Vector2(1f, 0f);
						}
						return new Vector2(-1f, 0f);
					}
					if (m_movementInput.y < 0f - GameUtils.Constants.s_inputMoveDeadzoneMinValue)
					{
						if (attachedStairs.DownDirection.x > 0f)
						{
							return new Vector2(1f, 0f);
						}
						return new Vector2(-1f, 0f);
					}
				}
				return m_movementInput;
			}
		}
	}

	public override bool IsJumping
	{
		get
		{
			if (IsJumpAllowed())
			{
				return m_requestedJump;
			}
			return false;
		}
	}

	public override bool IsFiring => m_isFiring;

	public override bool IsAiming
	{
		get
		{
			if (IsAimingAllowed())
			{
				if (!m_isAiming && !ALWAYS_AIM_INPUT)
				{
					return m_forceAimingInput != ForceAimMode.None;
				}
				return true;
			}
			return false;
		}
	}

	public override bool IsThrowAiming => m_isThrowAiming;

	public override Vector2 AimDirectionInput
	{
		get
		{
			if (m_aimDirection.magnitude > 0f)
			{
				return m_aimDirection;
			}
			if (m_movementInput.magnitude > 0f)
			{
				return m_movementInput;
			}
			return ExtensionMethods.Rotate(m_characterDirection.GetForwardVector(), (m_characterDirection.CurrentDirection == CharacterDirection.Facing.Right) ? m_characterAiming.StartAimAngle : (0f - m_characterAiming.StartAimAngle));
		}
	}

	public override bool IsUsingQuickMap => m_isUsingQuickMap;

	public float ParryInputTime => m_parryInputTime;

	public PlayerInputOverride InputOverride
	{
		get
		{
			return m_inputOverride;
		}
		set
		{
			m_inputOverride = value;
		}
	}

	public override bool TurnDisabled => m_turnDisabled;

	private RestrictPlayerActions.Mode GetRestrictionsMode()
	{
		return GlobalReferences.Instance.Sets.Input.RestrictPlayerActionsSet.GetMode();
	}

	private bool IsSprintAllowed()
	{
		if (GetRestrictionsMode() == RestrictPlayerActions.Mode.WalkOnly)
		{
			return false;
		}
		return true;
	}

	private bool IsAimingAllowed()
	{
		if (GetRestrictionsMode() == RestrictPlayerActions.Mode.WalkOnly)
		{
			return false;
		}
		return true;
	}

	private bool IsJumpAllowed()
	{
		if (GetRestrictionsMode() == RestrictPlayerActions.Mode.WalkOnly)
		{
			return false;
		}
		return true;
	}

	private bool IsFiringAllowed()
	{
		if (GetRestrictionsMode() == RestrictPlayerActions.Mode.WalkOnly)
		{
			return false;
		}
		return true;
	}

	private bool IsDodgeAllowed()
	{
		if (GetRestrictionsMode() == RestrictPlayerActions.Mode.WalkOnly)
		{
			return false;
		}
		return true;
	}

	public void SetForceAimingInput(ForceAimMode active)
	{
		m_forceAimingInput = active;
	}

	public bool IsForceAiming()
	{
		return m_forceAimingInput != ForceAimMode.None;
	}

	public void SetTurnDisabled(bool disabled)
	{
		m_turnDisabled = disabled;
	}

	private void Awake()
	{
		m_characterDirection = GetComponent<CharacterDirection>();
		m_characterAiming = GetComponent<CharacterAiming>();
		m_characterInventory = GetComponent<CharacterInventory>();
		m_characterMovement = GetComponent<CharacterMovement>();
	}

	private bool CanDoInput(bool allowItemWheel = false, bool ignoreInputBlock = false)
	{
		if (m_inputTempDisable || m_inputOverride != 0 || m_inputPaused || (m_itemWheelOpen && !allowItemWheel) || (m_inputBlockTimer > 0f && !ignoreInputBlock))
		{
			return false;
		}
		return true;
	}

	private bool ShouldAutoSprint()
	{
		return m_movementInput.magnitude > 0.8f;
	}

	private bool CanCancelToggleSprint()
	{
		if (!m_isSprintButtonDown)
		{
			return m_movementInput.magnitude < 0.1f;
		}
		return false;
	}

	private void OnEnable()
	{
		m_disableInputChannel.Register(DisableInputStateChanged);
		m_playerInputOverrideChannel.Register(SetPlayerInputOverride);
		InputState inputState = m_inputState;
		inputState.OnInputModeChanged = (UnityAction<InputState.Mode>)Delegate.Combine(inputState.OnInputModeChanged, new UnityAction<InputState.Mode>(OnControlsChanged));
		GameInputManager.GameInputActions.MenuToggles.ItemWheel.performed += OnItemWheelInput;
		GameInputManager.GameInputActions.MenuToggles.ItemWheel.canceled += OnItemWheelInputRelease;
		GlobalReferences.Instance.EventChannels.Inventory.ShowItemWheel.Register(ShowItemWheel);
		GameInputManager.GameInputActions.MenuToggles.QuickItemPanel.performed += OnQuickItemPanelInput;
		GameInputManager.GameInputActions.MenuToggles.QuickItemPanel.canceled += OnQuickItemPanelRelease;
		RegisterPlayerInput(GameInputManager.GameInputActions);
		ReadDefaultStateInputs();
	}

	private void ReadDefaultStateInputs()
	{
		m_movementInput = GameInputManager.GameInputActions.Player.Move.ReadValue<Vector2>();
		m_isSprinting = GameInputManager.GameInputActions.Player.Sprint.ReadValue<float>() > 0.5f;
	}

	private void OnDisable()
	{
		m_disableInputChannel.Unregister(DisableInputStateChanged);
		m_playerInputOverrideChannel.Unregister(SetPlayerInputOverride);
		InputState inputState = m_inputState;
		inputState.OnInputModeChanged = (UnityAction<InputState.Mode>)Delegate.Remove(inputState.OnInputModeChanged, new UnityAction<InputState.Mode>(OnControlsChanged));
		GlobalReferences.Instance.EventChannels.Inventory.ShowItemWheel.Unregister(ShowItemWheel);
		if (GameInputManager.GameInputActions != null)
		{
			UnregisterPlayerInput(GameInputManager.GameInputActions);
			GameInputManager.GameInputActions.MenuToggles.ItemWheel.performed -= OnItemWheelInput;
			GameInputManager.GameInputActions.MenuToggles.ItemWheel.canceled -= OnItemWheelInputRelease;
			GameInputManager.GameInputActions.MenuToggles.QuickItemPanel.performed -= OnQuickItemPanelInput;
			GameInputManager.GameInputActions.MenuToggles.QuickItemPanel.canceled -= OnQuickItemPanelRelease;
		}
	}

	private float GetAimAngleFromAimVector2(Vector2 input)
	{
		float num = Mathf.Atan2(input.x, input.y) * 57.29578f;
		if (num < 0f)
		{
			num += 360f;
		}
		return num;
	}

	private Vector2 GetAimVector2FromAngle(float angle)
	{
		float f = angle * (MathF.PI / 180f);
		return new Vector2(Mathf.Sin(f), Mathf.Cos(f));
	}

	private void Update()
	{
		if (m_inputState.InputMode == InputState.Mode.KeyboardMouse)
		{
			if (m_characterAiming != null && m_characterAiming.enabled)
			{
				Vector2 vector = Mouse.current.delta.ReadValue();
				bool flag = false;
				if (Mathf.Abs(vector.y) >= float.Epsilon)
				{
					float num = m_mouseAimRadius * (float)Screen.width;
					Vector2 normalized = m_mouseAimPosition.normalized;
					float num2 = Vector2.SignedAngle(m_characterDirection.GetForwardVector(), normalized);
					float num3 = vector.y * GlobalReferences.Instance.UserPreferences.MouseAimingSensitivity * 0.25f;
					if (m_characterDirection.CurrentDirection == CharacterDirection.Facing.Left)
					{
						num3 *= -1f;
					}
					if (Mathf.Abs(num2 + num3) > 90f)
					{
						num3 = ((m_characterDirection.CurrentDirection != m_characterDirection.DesiredDirection) ? 0f : ((!(num2 > 0f)) ? (-90f - num2) : (90f - num2)));
					}
					normalized = normalized.Rotate(num3);
					m_mouseAimPosition = normalized * num;
					flag = true;
				}
				if (flag)
				{
					UpdateValuesForMouseAim();
				}
			}
		}
		else if (m_characterAiming != null && m_characterAiming.enabled)
		{
			Vector2 vector2 = ((m_gamepadRawAimInput == Vector2.zero) ? m_movementInput : m_gamepadRawAimInput);
			float aimAngleFromAimVector = GetAimAngleFromAimVector2(vector2);
			bool flag2 = false;
			float num4 = 20f;
			if (m_characterDirection.CurrentDirection == CharacterDirection.Facing.Right)
			{
				if (aimAngleFromAimVector > 180f + num4 && aimAngleFromAimVector < 360f - num4)
				{
					flag2 = true;
				}
				aimAngleFromAimVector = ((!(aimAngleFromAimVector > 270f)) ? Mathf.Clamp(aimAngleFromAimVector, 1f, 179f) : 1f);
			}
			else
			{
				if (aimAngleFromAimVector > num4 && aimAngleFromAimVector < 180f - num4)
				{
					flag2 = true;
				}
				aimAngleFromAimVector = ((!(aimAngleFromAimVector < 90f)) ? Mathf.Clamp(aimAngleFromAimVector, 181f, 359f) : 359f);
			}
			if (vector2.magnitude > 0.8f)
			{
				if ((Mathf.Sign(m_aimDirection.x) != Mathf.Sign(vector2.x) && Mathf.Abs(vector2.x) > GameUtils.Constants.s_inputMoveDeadzoneMinValue) || flag2)
				{
					m_aimDirection = vector2;
					m_aimDirection.y = 0f;
					m_aimDirection.Normalize();
				}
				else
				{
					float gamepadAimingSensitivity = GlobalReferences.Instance.UserPreferences.GamepadAimingSensitivity;
					float angle = Mathf.MoveTowardsAngle(GetAimAngleFromAimVector2(m_aimDirection), aimAngleFromAimVector, Time.deltaTime * m_gamepadAimingSensitivity * gamepadAimingSensitivity);
					Vector2 vector3 = (m_aimDirection = GetAimVector2FromAngle(angle));
				}
			}
		}
		if (GlobalReferences.Instance.UserPreferences.RunToggle && m_isSprinting && CanCancelToggleSprint())
		{
			m_isSprinting = false;
		}
		if (m_inputBlockTimer > 0f)
		{
			m_inputBlockTimer -= Time.deltaTime;
		}
	}

	private void RegisterPlayerInput(GameInputActions inputActions)
	{
		inputActions.Player.SetCallbacks(this);
	}

	private void UnregisterPlayerInput(GameInputActions inputActions)
	{
		inputActions.Player.SetCallbacks(null);
	}

	private void ShowItemWheel(bool shown)
	{
		m_itemWheelOpen = shown;
		if (shown)
		{
			m_inputBlockTimer = 0.1f;
		}
	}

	private void OnControlsChanged(InputState.Mode newMode)
	{
		m_aimDirection = Vector2.zero;
		m_movementInput = Vector2.zero;
	}

	public void OnCrouch(InputAction.CallbackContext context)
	{
		if (!CanDoInput())
		{
			return;
		}
		if (GlobalReferences.Instance.UserPreferences.CrouchToggle)
		{
			if (context.performed)
			{
				m_isCrouching = !m_isCrouching;
			}
		}
		else
		{
			m_isCrouching = context.performed;
		}
	}

	public void ForceSetCrouchToggle()
	{
		if (GlobalReferences.Instance.UserPreferences.CrouchToggle)
		{
			m_isCrouching = true;
		}
	}

	public void ClearCrouchToggle()
	{
		if (GlobalReferences.Instance.UserPreferences.CrouchToggle)
		{
			m_isCrouching = false;
		}
	}

	public void OnJump(InputAction.CallbackContext context)
	{
		if (CanDoInput() && m_forceAimingInput == ForceAimMode.None && context.performed)
		{
			m_isCrouching = false;
			if (m_jumpCoroutine != null)
			{
				StopCoroutine(m_jumpCoroutine);
				m_jumpCoroutine = null;
			}
			m_jumpCoroutine = StartCoroutine(JumpCoroutine());
		}
	}

	public void OnInteract(InputAction.CallbackContext context)
	{
		if (CanDoInput() && context.performed)
		{
			OnInteractAction?.Invoke();
		}
	}

	public void OnReload(InputAction.CallbackContext context)
	{
		if (CanDoInput() && context.performed)
		{
			OnReloadAction?.Invoke();
		}
	}

	public void ConsumeJumpInput()
	{
		if (CanDoInput())
		{
			if (m_jumpCoroutine != null)
			{
				StopCoroutine(m_jumpCoroutine);
				m_jumpCoroutine = null;
			}
			m_requestedJump = false;
		}
	}

	private IEnumerator JumpCoroutine()
	{
		m_requestedJump = true;
		yield return new WaitForSeconds(m_jumpWindow);
		m_requestedJump = false;
	}

	public void OnSprint(InputAction.CallbackContext context)
	{
		if (!CanDoInput())
		{
			return;
		}
		if (GlobalReferences.Instance.UserPreferences.AlwaysRun)
		{
			if (m_isSprinting && context.performed)
			{
				m_isSprinting = false;
			}
			else if (!m_isSprinting && !context.performed)
			{
				m_isSprinting = ShouldAutoSprint();
			}
		}
		else if (GlobalReferences.Instance.UserPreferences.RunToggle)
		{
			if (context.performed)
			{
				m_isSprinting = !m_isSprinting;
			}
		}
		else
		{
			m_isSprinting = context.performed;
		}
		m_isSprintButtonDown = context.performed;
	}

	public void OnMove(InputAction.CallbackContext context)
	{
		m_movementInput = context.action.ReadValue<Vector2>();
		if (GlobalReferences.Instance.UserPreferences.AlwaysRun)
		{
			m_isSprinting = ShouldAutoSprint() && !m_isSprintButtonDown;
		}
	}

	public void OnFire(InputAction.CallbackContext context)
	{
		if (CanDoInput() && IsFiringAllowed())
		{
			m_isFiring = context.performed;
			OnFireAction?.Invoke(context.performed);
		}
	}

	public void OnAimMode(InputAction.CallbackContext context)
	{
		if (CanDoInput() && IsAimingAllowed())
		{
			m_isAiming = context.performed;
		}
	}

	public void OnThrowAimMode(InputAction.CallbackContext context)
	{
		if (CanDoInput() && IsAimingAllowed())
		{
			m_isThrowAiming = context.performed;
		}
	}

	private Vector2 GetDefaultAimingForwardVector()
	{
		return ExtensionMethods.Rotate(m_characterDirection.GetForwardVector(m_characterDirection.CurrentDirection), (m_characterDirection.CurrentDirection == CharacterDirection.Facing.Right) ? m_characterAiming.StartAimAngle : (0f - m_characterAiming.StartAimAngle));
	}

	public void OnStartCharacterAiming()
	{
		if (m_inputState.InputMode == InputState.Mode.KeyboardMouse)
		{
			m_mouseAimPosition = GetDefaultAimingForwardVector();
			m_mouseAimPosition *= m_mouseAimRadius * (float)Screen.width;
			UpdateValuesForMouseAim();
		}
		else
		{
			m_gamepadRawAimInput = Vector2.zero;
			m_aimDirection = GetDefaultAimingForwardVector();
		}
	}

	public void FlipCharacterAim()
	{
		if (m_inputState.InputMode == InputState.Mode.KeyboardMouse)
		{
			m_mouseAimPosition.x *= -1f;
			UpdateValuesForMouseAim();
		}
	}

	public void OnDodge(InputAction.CallbackContext context)
	{
		if (CanDoInput() && IsDodgeAllowed() && m_forceAimingInput == ForceAimMode.None && context.performed)
		{
			OnDodgeAction?.Invoke();
		}
	}

	public void OnAimDirection(InputAction.CallbackContext context)
	{
		if (CanDoInput() && m_inputState.InputMode != InputState.Mode.KeyboardMouse)
		{
			Vector2 vector = (m_gamepadRawAimInput = context.ReadValue<Vector2>());
		}
	}

	private void UpdateValuesForMouseAim()
	{
		float magnitude = m_mouseAimPosition.magnitude;
		Vector2 normalized = m_mouseAimPosition.normalized;
		Vector2 mouseAimOffset = m_mouseAimPosition / (m_mouseAimRadius * (float)Screen.width);
		m_mouseAimInputData.m_mouseAimOffset = mouseAimOffset;
		if (!(magnitude < (float)Screen.width * m_mouseAimMinDistance))
		{
			m_aimDirection = normalized;
		}
	}

	private void DisableInputStateChanged(bool enabled)
	{
		m_inputPaused = !enabled;
	}

	public void SetInputDisabled(bool disabled)
	{
		if (disabled)
		{
			m_isFiring = false;
			m_isAiming = false;
			m_isUsingQuickMap = false;
			m_isSprinting = false;
		}
		else
		{
			ReadDefaultStateInputs();
		}
		m_inputTempDisable = disabled;
	}

	private void SetPlayerInputOverride(PlayerInputOverride overrideType)
	{
		SetInputDisabled(overrideType != PlayerInputOverride.None);
		m_inputOverride = overrideType;
	}

	public void OnTransitionInteractUp(InputAction.CallbackContext context)
	{
		if (CanDoInput() && context.performed)
		{
			OnInteractUpAction?.Invoke();
		}
	}

	public void OnTransitionInteractDown(InputAction.CallbackContext context)
	{
		if (CanDoInput() && context.performed)
		{
			OnInteractDownAction?.Invoke();
		}
	}

	public void OnStomp(InputAction.CallbackContext context)
	{
		if (CanDoInput(allowItemWheel: true) && context.performed)
		{
			OnStompAction?.Invoke();
		}
	}

	private void OnItemWheelInput(InputAction.CallbackContext context)
	{
		if (CanDoInput(allowItemWheel: true) && context.performed)
		{
			OnRequestItemWheel?.Invoke();
		}
	}

	private void OnItemWheelInputRelease(InputAction.CallbackContext context)
	{
		if (CanDoInput(allowItemWheel: true, ignoreInputBlock: true) && context.canceled)
		{
			OnRequestCloseItemWheel?.Invoke();
			OnCloseItemWheelInputBlock();
		}
	}

	private void OnQuickItemPanelInput(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			OnRequestQuickItemPanel?.Invoke(arg0: true);
			m_isTryingToShowQuickItemPanel = true;
		}
	}

	private void OnQuickItemPanelRelease(InputAction.CallbackContext context)
	{
		if (context.canceled)
		{
			OnRequestQuickItemPanel?.Invoke(arg0: false);
			m_isTryingToShowQuickItemPanel = false;
		}
	}

	private void OnCloseItemWheelInputBlock()
	{
		m_inputBlockTimer = 0.1f;
	}

	private void OnDrawGizmos()
	{
		if (m_aimCenterTransform != null)
		{
			Vector2 vector = m_mouseAimPosition / (m_mouseAimRadius * (float)Screen.width);
			Vector3 position = m_aimCenterTransform.transform.position;
			position.x += vector.x;
			position.y += vector.y;
			Gizmos.color = Color.red;
			Gizmos.DrawSphere(position, 0.1f);
			GizmoExtensions.DrawArrow(m_aimCenterTransform.transform.position, m_aimDirection, 1f, 0.1f, 0.1f);
			Gizmos.color = Color.white;
		}
	}

	public void OnQuickMap(InputAction.CallbackContext context)
	{
		if (!CanDoInput())
		{
			return;
		}
		if (GlobalReferences.Instance.UserPreferences.QuickMapToggle)
		{
			if (context.performed)
			{
				m_isUsingQuickMap = !m_isUsingQuickMap;
			}
		}
		else
		{
			m_isUsingQuickMap = context.performed;
		}
	}

	public void ClearQuickMapInput()
	{
		if (GlobalReferences.Instance.UserPreferences.QuickMapToggle)
		{
			m_isUsingQuickMap = false;
		}
	}

	public void OnPlayerCancel(InputAction.CallbackContext context)
	{
		ClearQuickMapInput();
	}

	public void OnCycleWeapon(InputAction.CallbackContext context)
	{
		if (CanDoInput(allowItemWheel: true, ignoreInputBlock: true) && context.performed)
		{
			float num = context.ReadValue<float>();
			if (num > 0.5f)
			{
				OnCycleNextWeapon?.Invoke();
			}
			else if (num < -0.5f)
			{
				OnCyclePreviousWeapon?.Invoke();
			}
		}
	}

	private bool CanPerformQuickShortcut(bool allowedDuringAiming)
	{
		if (m_isTryingToShowQuickItemPanel)
		{
			return false;
		}
		if (!allowedDuringAiming && m_isAiming)
		{
			return false;
		}
		return true;
	}

	public void OnShortcutFlashlight(InputAction.CallbackContext context)
	{
		if (context.performed && CanPerformQuickShortcut(allowedDuringAiming: true))
		{
			m_characterInventory.Inventory.PerformShortcut(m_flashlightItem);
		}
	}

	public void OnShortcutDig(InputAction.CallbackContext context)
	{
		if (context.performed && CanPerformQuickShortcut(allowedDuringAiming: false) && m_characterInventory.ComplexInventoryActionsAllowed == CharacterInventory.ComplexInventoryActionsMode.All)
		{
			m_characterInventory.Inventory.PerformShortcut(m_digItem);
		}
	}

	public void OnShortcutCamera(InputAction.CallbackContext context)
	{
		if (context.performed && CanPerformQuickShortcut(allowedDuringAiming: false) && m_characterInventory.ComplexInventoryActionsAllowed == CharacterInventory.ComplexInventoryActionsMode.All)
		{
			m_characterInventory.Inventory.PerformShortcut(m_cameraItem);
		}
	}

	public void OnQuickItemRight(InputAction.CallbackContext context)
	{
	}

	public void OnQuickItemUse(InputAction.CallbackContext context)
	{
	}

	public void OnQuickItemLeft(InputAction.CallbackContext context)
	{
	}
}
