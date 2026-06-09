using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class ToggleExtended : Toggle
{
	private bool m_isPointerInside;

	private CanvasGroup m_canvasGroup;

	private readonly float m_normal_alpha = 0.4f;

	private readonly float m_disabled_alpha = 0.25f;

	private readonly float m_selected_alpha = 1f;

	protected override void OnEnable()
	{
		base.OnEnable();
		InputState inputState = GlobalReferences.Instance.InputState;
		inputState.OnInputModeChanged = (UnityAction<InputState.Mode>)Delegate.Combine(inputState.OnInputModeChanged, new UnityAction<InputState.Mode>(InputModeChanged));
		if (m_canvasGroup == null)
		{
			m_canvasGroup = GetComponent<CanvasGroup>();
			if (m_canvasGroup != null)
			{
				m_canvasGroup.alpha = m_normal_alpha;
			}
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		InputState inputState = GlobalReferences.Instance.InputState;
		inputState.OnInputModeChanged = (UnityAction<InputState.Mode>)Delegate.Remove(inputState.OnInputModeChanged, new UnityAction<InputState.Mode>(InputModeChanged));
	}

	protected override void DoStateTransition(SelectionState state, bool instant)
	{
		switch (GlobalReferences.Instance.InputState.InputMode)
		{
		case InputState.Mode.KeyboardMouse:
			switch (state)
			{
			case SelectionState.Highlighted:
				state = SelectionState.Selected;
				break;
			case SelectionState.Selected:
				if (!m_isPointerInside)
				{
					state = SelectionState.Normal;
				}
				break;
			}
			break;
		case InputState.Mode.Gamepad:
			if (state == SelectionState.Highlighted)
			{
				state = SelectionState.Normal;
			}
			break;
		}
		if (m_canvasGroup != null)
		{
			switch (state)
			{
			case SelectionState.Selected:
				m_canvasGroup.alpha = m_selected_alpha;
				break;
			case SelectionState.Disabled:
				m_canvasGroup.alpha = m_disabled_alpha;
				break;
			default:
				m_canvasGroup.alpha = m_normal_alpha;
				break;
			}
		}
		base.DoStateTransition(state, instant);
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		m_isPointerInside = true;
		base.OnPointerEnter(eventData);
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		m_isPointerInside = false;
		base.OnPointerExit(eventData);
	}

	private void InputModeChanged(InputState.Mode mode)
	{
		DoStateTransition(base.currentSelectionState, instant: true);
	}
}
