using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;
using UnityEngine.UI;

public class GamepadCursorController : MonoBehaviour
{
	[SerializeField]
	private GameInputManagerAnchor m_gameInputManagerAnchor;

	[SerializeField]
	private InputState m_inputState;

	[SerializeField]
	private RectTransform m_cursorTransform;

	[SerializeField]
	private float m_cursorSpeed = 1000f;

	[SerializeField]
	private RectTransform m_canvasRectTransform;

	[SerializeField]
	private float m_padding;

	[SerializeField]
	private Image m_cursorGraphic;

	[Header("Cursor Snapping")]
	[SerializeField]
	private bool m_useUICursorSnapping;

	[SerializeField]
	private float m_snapDelay = 0.05f;

	[SerializeField]
	private float m_snapSpeed;

	[SerializeField]
	private float m_snapRectMargin;

	[Header("Size")]
	[SerializeField]
	private Sprite m_gamepadSpriteNormal;

	[SerializeField]
	private Sprite m_gamepadSpriteHighligt;

	private Mouse m_virtualMouse;

	private bool m_buttonPressed;

	private static bool m_active;

	private Vector2 m_previousMousePos;

	private List<MonoBehaviour> m_selectables;

	private bool m_isHoveringSelectable;

	private bool m_awaitingDpadReset;

	private MonoBehaviour m_snapTarget;

	private float m_cursorSnappingTimer;

	public RectTransform CursorTransform => m_cursorTransform;

	public static bool IsActive => m_active;

	public bool IsVisible => m_cursorGraphic.enabled;

	public bool IsHoveringSelectable
	{
		get
		{
			return m_isHoveringSelectable;
		}
		set
		{
			m_isHoveringSelectable = value;
		}
	}

	private float CursorSpeed
	{
		get
		{
			float num = (float)Screen.height / 1080f;
			return m_cursorSpeed * num;
		}
	}

	public void RefreshSelectables(GameObject root)
	{
		m_selectables = GameUtils.GetMinigameSelectables(root);
	}

	private void OnEnable()
	{
		if (m_cursorTransform != null)
		{
			m_cursorTransform.gameObject.SetActive(value: false);
		}
		InputState inputState = m_inputState;
		inputState.OnInputModeChanged = (UnityAction<InputState.Mode>)Delegate.Combine(inputState.OnInputModeChanged, new UnityAction<InputState.Mode>(OnInputModeChanged));
		if (m_inputState.InputMode == InputState.Mode.Gamepad)
		{
			EnableGamepadCursor();
		}
		GlobalReferences.Instance.Anchors.Input.CursorOverridesAnchor.Register(OnCursorOverrideChanged);
	}

	private void OnDisable()
	{
		InputState inputState = m_inputState;
		inputState.OnInputModeChanged = (UnityAction<InputState.Mode>)Delegate.Remove(inputState.OnInputModeChanged, new UnityAction<InputState.Mode>(OnInputModeChanged));
		DisableGamepadCursor();
		GlobalReferences.Instance.Anchors.Input.CursorOverridesAnchor.Unregister(OnCursorOverrideChanged);
	}

	private void OnCursorOverrideChanged(ICursorOverrides arg0)
	{
		if (m_active)
		{
			UpdateCursorVisibility();
		}
	}

	private void OnInputModeChanged(InputState.Mode mode)
	{
		RefreshState(mode);
	}

	private void RefreshState(InputState.Mode mode)
	{
		if (mode == InputState.Mode.Gamepad)
		{
			EnableGamepadCursor();
		}
		else
		{
			DisableGamepadCursor();
		}
	}

	private void EnableGamepadCursor()
	{
		if (m_active)
		{
			return;
		}
		m_active = true;
		m_buttonPressed = false;
		if (m_virtualMouse == null)
		{
			m_virtualMouse = (Mouse)InputSystem.AddDevice("VirtualMouse");
			if (m_cursorTransform != null)
			{
				UnityEngine.InputSystem.LowLevel.InputState.Change(state: new Vector2((float)Screen.width * 0.5f, (float)Screen.height * 0.5f), control: m_virtualMouse.position);
			}
		}
		else if (!m_virtualMouse.added)
		{
			InputSystem.AddDevice(m_virtualMouse);
			UnityEngine.InputSystem.LowLevel.InputState.Change(m_virtualMouse.position, m_previousMousePos);
			AnchorCursor(m_previousMousePos);
		}
		if (m_cursorTransform != null)
		{
			m_cursorTransform.gameObject.SetActive(value: true);
		}
		InputUser.PerformPairingWithDevice(m_virtualMouse, m_gameInputManagerAnchor.Item.MainPlayerInputUser);
		InputSystem.onAfterUpdate += UpdateMotion;
		UpdateCursorVisibility();
	}

	private void Update()
	{
		float num = (m_isHoveringSelectable ? 1f : 0.75f);
		m_cursorGraphic.transform.localScale = Vector3.MoveTowards(m_cursorGraphic.transform.localScale, Vector3.one * num, Time.deltaTime * 20f);
		m_cursorGraphic.sprite = (m_isHoveringSelectable ? m_gamepadSpriteHighligt : m_gamepadSpriteNormal);
	}

	private void UpdateCursorVisibility()
	{
		bool flag = true;
		if (GlobalReferences.Instance.Anchors.Input.CursorOverridesAnchor.Item != null)
		{
			switch (GlobalReferences.Instance.Anchors.Input.CursorOverridesAnchor.Item.ShouldShowCursor)
			{
			case ICursorOverrides.CursorOverrideOption.ForceOff:
				flag = false;
				break;
			case ICursorOverrides.CursorOverrideOption.ForceOn:
				flag = true;
				break;
			}
		}
		m_cursorGraphic.enabled = flag;
	}

	private void DisableGamepadCursor()
	{
		if (m_active)
		{
			if (m_virtualMouse != null)
			{
				m_previousMousePos = m_virtualMouse.position.value;
				InputSystem.RemoveDevice(m_virtualMouse);
				InputSystem.onAfterUpdate -= UpdateMotion;
			}
			m_active = false;
			if (m_cursorTransform != null)
			{
				m_cursorTransform.gameObject.SetActive(value: false);
			}
		}
	}

	private bool GetSnapSelectionTarget(Vector2 currentPosition, Vector2 direction)
	{
		if (m_selectables != null && m_selectables.Count > 0)
		{
			MonoBehaviour monoBehaviour = null;
			float num = float.PositiveInfinity;
			Vector3 lhs = direction.normalized;
			foreach (MonoBehaviour selectable in m_selectables)
			{
				Selectable component = selectable.GetComponent<Selectable>();
				if ((component != null && (!component.IsInteractable() || !component.enabled)) || !selectable.gameObject.activeInHierarchy)
				{
					continue;
				}
				Vector3 vector = (Vector2)selectable.transform.position - currentPosition;
				float num2 = Vector3.Dot(lhs, vector.normalized);
				if (num2 < 0.5f)
				{
					continue;
				}
				RectTransform component2 = selectable.GetComponent<RectTransform>();
				if (!IsCursorInside(currentPosition, component2))
				{
					float sqrMagnitude = vector.sqrMagnitude;
					if (sqrMagnitude < num)
					{
						monoBehaviour = selectable;
						num = sqrMagnitude;
					}
				}
			}
			if (monoBehaviour != null)
			{
				Debug.Log("Cursor snap to: " + monoBehaviour.gameObject.name);
				m_snapTarget = monoBehaviour;
				return true;
			}
		}
		return false;
	}

	private Vector2 GetMoveToSnapTargetDelta(Vector2 currentPosition)
	{
		Vector2 vector = m_snapTarget.transform.position;
		float num = Vector2.Distance(currentPosition, vector);
		float num2 = (float)Screen.width * Time.deltaTime;
		if (num < num2)
		{
			m_snapTarget = null;
			return vector - currentPosition;
		}
		return (vector - currentPosition).normalized * num2;
	}

	private void UpdateMotion()
	{
		if (m_virtualMouse == null || Gamepad.current == null)
		{
			return;
		}
		bool flag = true;
		if (GlobalReferences.Instance.Anchors.Input.CursorOverridesAnchor.Item != null && GlobalReferences.Instance.Anchors.Input.CursorOverridesAnchor.Item.ShouldShowCursor == ICursorOverrides.CursorOverrideOption.ForceOff)
		{
			flag = false;
		}
		if (flag)
		{
			Vector2 vector = Gamepad.current.dpad.ReadValue();
			Vector2 vector2 = m_virtualMouse.position.ReadValue();
			Vector2 vector3 = Vector2.zero;
			if (m_awaitingDpadReset && vector == Vector2.zero)
			{
				m_awaitingDpadReset = false;
			}
			if (vector != Vector2.zero && !m_awaitingDpadReset)
			{
				m_awaitingDpadReset = true;
				GetSnapSelectionTarget(vector2, vector);
			}
			else
			{
				vector3 = Gamepad.current.leftStick.ReadValue();
				vector3 *= CursorSpeed * Time.unscaledDeltaTime;
				if (GlobalReferences.Instance.Anchors.Map.UIMap3DPanelAnchor.Item != null)
				{
					vector2 = GlobalReferences.Instance.Anchors.Map.UIMap3DPanelAnchor.Item.GetGamepadCursorPosition();
				}
				else
				{
					vector2 += vector3;
					if (m_useUICursorSnapping)
					{
						vector2 = UpdateMinigameCursorSnapping(vector3, vector2);
					}
				}
			}
			if (m_snapTarget != null)
			{
				vector3 = GetMoveToSnapTargetDelta(vector2);
				vector2 += vector3;
			}
			vector2.x = Mathf.Clamp(vector2.x, m_padding, (float)Screen.width - m_padding);
			vector2.y = Mathf.Clamp(vector2.y, m_padding, (float)Screen.height - m_padding);
			UnityEngine.InputSystem.LowLevel.InputState.Change(m_virtualMouse.position, vector2);
			UnityEngine.InputSystem.LowLevel.InputState.Change(m_virtualMouse.delta, vector3);
			AnchorCursor(vector2);
		}
		bool flag2 = GameInputManager.GameInputActions.UI.SubmitGamepadOnly.IsPressed() && m_snapTarget == null;
		if (m_buttonPressed != flag2)
		{
			m_virtualMouse.CopyState<MouseState>(out var state);
			state.WithButton(MouseButton.Left, flag2);
			UnityEngine.InputSystem.LowLevel.InputState.Change(m_virtualMouse, state);
			m_buttonPressed = flag2;
		}
	}

	private void AnchorCursor(Vector2 position)
	{
		RectTransformUtility.ScreenPointToLocalPointInRectangle(m_canvasRectTransform, position, null, out var localPoint);
		m_cursorTransform.anchoredPosition = localPoint;
	}

	private Vector2 SnapToNearestButton(Vector2 currentPosition)
	{
		if (m_selectables == null)
		{
			return currentPosition;
		}
		MonoBehaviour monoBehaviour = null;
		float num = float.MaxValue;
		foreach (MonoBehaviour selectable2 in m_selectables)
		{
			if (selectable2 == null)
			{
				continue;
			}
			Selectable selectable = selectable2 as Selectable;
			if (!selectable2.isActiveAndEnabled || (selectable != null && !selectable.interactable))
			{
				continue;
			}
			RectTransform component = selectable2.GetComponent<RectTransform>();
			if (!(component != null) || !IsCursorInside(currentPosition, component))
			{
				continue;
			}
			GamepadCursorMagnetismTarget componentInChildren = component.GetComponentInChildren<GamepadCursorMagnetismTarget>();
			if (!(componentInChildren != null) || !componentInChildren.DisableMagnetism)
			{
				Vector3 worldPoint = (componentInChildren ? componentInChildren.Position : component.position);
				Vector3 vector = RectTransformUtility.WorldToScreenPoint(null, worldPoint);
				float num2 = Vector2.Distance(currentPosition, vector);
				if (num2 < num)
				{
					num = num2;
					monoBehaviour = selectable2;
				}
			}
		}
		if (monoBehaviour != null)
		{
			GamepadCursorMagnetismTarget componentInChildren2 = monoBehaviour.GetComponentInChildren<GamepadCursorMagnetismTarget>();
			Vector3 worldPoint2 = (componentInChildren2 ? componentInChildren2.Position : monoBehaviour.transform.position);
			Vector2 target = RectTransformUtility.WorldToScreenPoint(null, worldPoint2);
			currentPosition = Vector2.MoveTowards(currentPosition, target, Time.unscaledDeltaTime * m_snapSpeed);
		}
		return currentPosition;
	}

	private bool IsCursorInside(Vector2 cursorPos, RectTransform rect)
	{
		Vector3[] array = new Vector3[4];
		rect.GetWorldCorners(array);
		Vector2 vector = RectTransformUtility.WorldToScreenPoint(null, array[0]);
		Vector2 vector2 = RectTransformUtility.WorldToScreenPoint(null, array[2]);
		vector -= new Vector2(m_snapRectMargin, m_snapRectMargin);
		vector2 += new Vector2(m_snapRectMargin, m_snapRectMargin);
		if (cursorPos.x >= vector.x && cursorPos.x <= vector2.x && cursorPos.y >= vector.y)
		{
			return cursorPos.y <= vector2.y;
		}
		return false;
	}

	private Vector2 UpdateMinigameCursorSnapping(Vector2 deltaValue, Vector2 cursorPosition)
	{
		if (deltaValue.magnitude < 0.01f)
		{
			m_cursorSnappingTimer += Time.deltaTime;
			if (m_cursorSnappingTimer > m_snapDelay)
			{
				cursorPosition = SnapToNearestButton(cursorPosition);
			}
		}
		else
		{
			m_cursorSnappingTimer = 0f;
		}
		return cursorPosition;
	}
}
