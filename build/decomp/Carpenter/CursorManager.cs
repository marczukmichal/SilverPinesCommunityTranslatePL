using UnityEngine;
using UnityEngine.InputSystem;

public class CursorManager : MonoBehaviour
{
	[Header("Gamepad Cursor")]
	[SerializeField]
	private GamepadCursorController m_gamepadCursorController;

	[Header("Mouse Cursor")]
	[SerializeField]
	private Texture2D m_cursorNormal;

	[SerializeField]
	private Vector2 m_cursorNormalHotspot;

	[SerializeField]
	private Texture2D m_cursorHighlight;

	[SerializeField]
	private Vector2 m_cursorHighlightHotspot;

	[SerializeField]
	private Texture2D m_cursorHidden;

	[DebugCommand("force_cursor", "Alaways show the UI mouse cursor", "force_cursor <true/false>", typeof(bool), true)]
	private static bool FORCE_CURSOR;

	private bool m_shouldShowCursor;

	private static bool m_systemCursorVisible;

	private GameInputManager m_gameInputManager;

	private bool m_gamepadCursorValid;

	public static bool IsShowingCursor => m_systemCursorVisible;

	private void Awake()
	{
		m_gameInputManager = GetComponent<GameInputManager>();
		m_gamepadCursorValid = true;
	}

	private void OnEnable()
	{
		GlobalReferences.Instance.EventChannels.Input.RefreshMouseCursorState.Register(UpdateCursorState);
		GlobalReferences.Instance.EventChannels.Generic.SetGamepadCursorAllowed.Register(SetGamepadCursorValid);
		GlobalReferences.Instance.Anchors.Map.UIMap3DPanelAnchor.Register(OnMapPanelChanged);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.Input.RefreshMouseCursorState.Unregister(UpdateCursorState);
		GlobalReferences.Instance.Anchors.Map.UIMap3DPanelAnchor.Unregister(OnMapPanelChanged);
		GlobalReferences.Instance.EventChannels.Generic.SetGamepadCursorAllowed.Unregister(SetGamepadCursorValid);
	}

	private void OnMapPanelChanged(UIMap3DPanel mapPanel)
	{
		UpdateCursorState();
	}

	private void Update()
	{
		if (!Application.isEditor && !m_shouldShowCursor && GlobalReferences.Instance.InputState.InputMode == InputState.Mode.Gamepad && !GamepadCursorController.IsActive)
		{
			Mouse.current.WarpCursorPosition(Vector2.zero);
		}
		Cursor.lockState = ((!m_shouldShowCursor && !GamepadCursorController.IsActive) ? CursorLockMode.Locked : CursorLockMode.Confined);
		Cursor.visible = m_shouldShowCursor && m_systemCursorVisible;
	}

	private bool ShouldShowGamepadController()
	{
		if (GlobalReferences.Instance.InputState.InputMode == InputState.Mode.Gamepad)
		{
			if (!m_gamepadCursorValid)
			{
				return false;
			}
			if (GlobalReferences.Instance.GameMenuState.MenuAllowsCursorOnGamepad())
			{
				return true;
			}
		}
		return false;
	}

	public void UpdateCursorState()
	{
		if (m_gameInputManager == null)
		{
			m_gameInputManager = GetComponent<GameInputManager>();
		}
		bool flag = ShouldShowGamepadController();
		m_gamepadCursorController.enabled = flag;
		if (Mouse.current == null)
		{
			return;
		}
		bool flag2 = false;
		bool flag3 = true;
		InputState.Mode inputMode = GlobalReferences.Instance.InputState.InputMode;
		if (FORCE_CURSOR || DebugConsole.IsShowing)
		{
			flag2 = true;
		}
		else if (m_gameInputManager.ActionMapMode != GameInputManager.ActiveActionMapMode.PlayerControl && (inputMode == InputState.Mode.KeyboardMouse || inputMode == InputState.Mode.None) && GlobalReferences.Instance.GameMenuState.MenuAllowsCursorOnPCMouseCursor())
		{
			flag2 = true;
		}
		if (!flag2 && inputMode != InputState.Mode.KeyboardMouse && !GamepadCursorController.IsActive)
		{
			flag3 = false;
		}
		if (flag3)
		{
			InputSystem.EnableDevice(Mouse.current);
		}
		else
		{
			InputSystem.DisableDevice(Mouse.current);
		}
		m_shouldShowCursor = flag2;
		if (!Application.isEditor && !flag2 && inputMode == InputState.Mode.Gamepad && !GamepadCursorController.IsActive)
		{
			Mouse.current.WarpCursorPosition(Vector2.zero);
		}
		Cursor.lockState = ((!flag2 && !GamepadCursorController.IsActive) ? CursorLockMode.Locked : CursorLockMode.Confined);
		m_systemCursorVisible = flag2;
		if (flag2 && GlobalReferences.Instance.Anchors.Input.CursorOverridesAnchor.Item != null)
		{
			switch (GlobalReferences.Instance.Anchors.Input.CursorOverridesAnchor.Item.ShouldShowCursor)
			{
			case ICursorOverrides.CursorOverrideOption.ForceOff:
				m_systemCursorVisible = false;
				break;
			case ICursorOverrides.CursorOverrideOption.ForceOn:
				m_systemCursorVisible = true;
				break;
			}
		}
		bool flag4 = false;
		CursorInteractHighlightController controller = GlobalReferences.Instance.Sets.Input.CursorInteractHighlightSet.GetController();
		UIMap3DPanel item = GlobalReferences.Instance.Anchors.Map.UIMap3DPanelAnchor.Item;
		if (item != null)
		{
			flag4 = item.IsHoveringIcon;
		}
		else if (controller != null)
		{
			flag4 = controller.IsHoveringInteractable;
		}
		Cursor.visible = m_systemCursorVisible;
		if (m_systemCursorVisible)
		{
			if (flag4)
			{
				Cursor.SetCursor(m_cursorHighlight, m_cursorHighlightHotspot, CursorMode.Auto);
			}
			else
			{
				Cursor.SetCursor(m_cursorNormal, m_cursorNormalHotspot, CursorMode.Auto);
			}
		}
		else
		{
			Cursor.SetCursor(m_cursorHidden, Vector2.zero, CursorMode.Auto);
		}
		if (!m_gamepadCursorController.enabled)
		{
			return;
		}
		m_gamepadCursorController.IsHoveringSelectable = flag4;
		if (controller != null)
		{
			GameObject root = null;
			if (controller != null)
			{
				root = controller.gameObject;
			}
			m_gamepadCursorController.RefreshSelectables(root);
		}
	}

	private void SetGamepadCursorValid(bool gamepadCursorValid)
	{
		m_gamepadCursorValid = gamepadCursorValid;
		UpdateCursorState();
	}
}
