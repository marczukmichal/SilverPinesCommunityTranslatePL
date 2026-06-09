using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;

[CreateAssetMenu(menuName = "Hints/Basic Hint Info")]
public class HintInfo : ScriptableObject
{
	public enum HintInputActionType
	{
		InputActionReference,
		Inventory,
		Map,
		Notes
	}

	[SerializeField]
	private HintInputActionType m_inputActionType;

	[SerializeField]
	private InputActionReference m_inputAction;

	[Header("Inline text inputs {input_0}, {input_1} etc.")]
	[SerializeField]
	private InputActionReference[] m_inlineTextInputActions;

	[SerializeField]
	private LocalizedString m_hintStringReference;

	[SerializeField]
	private float m_minimumShowTime;

	[SerializeField]
	private bool m_disableTimeout;

	[SerializeField]
	private bool m_dontClearOnInput;

	public InputAction InputAction
	{
		get
		{
			switch (m_inputActionType)
			{
			case HintInputActionType.Inventory:
				return GameInputManager.GameInputActions.MenuToggles.Inventory;
			case HintInputActionType.Map:
				if (GlobalReferences.Instance.InputState.InputMode == InputState.Mode.KeyboardMouse)
				{
					return GameInputManager.GameInputActions.MenuToggles.Map;
				}
				return GameInputManager.GameInputActions.MenuToggles.Inventory;
			case HintInputActionType.Notes:
				if (GlobalReferences.Instance.InputState.InputMode == InputState.Mode.KeyboardMouse)
				{
					return GameInputManager.GameInputActions.MenuToggles.Notes;
				}
				return GameInputManager.GameInputActions.MenuToggles.Inventory;
			default:
				if (m_inputAction != null)
				{
					return m_inputAction.action;
				}
				return null;
			}
		}
	}

	public InputActionReference[] InlineTextInputActions => m_inlineTextInputActions;

	public string HintText => m_hintStringReference.GetLocalizedString();

	public float MinimumShowTime => m_minimumShowTime;

	public bool DisableTimeout => m_disableTimeout;

	public bool DontClearOnInput => m_dontClearOnInput;
}
