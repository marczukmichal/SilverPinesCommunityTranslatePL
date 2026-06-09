using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class CycleButton : Selectable
{
	[SerializeField]
	private RectTransform m_leftButton;

	[SerializeField]
	private RectTransform m_rightButton;

	[SerializeField]
	private TextMeshProUGUI m_displayText;

	[SerializeField]
	private CanvasGroup m_canvasGroup;

	[SerializeField]
	private ItemListPips m_pips;

	[SerializeField]
	private LocalizeStringEvent m_labelLocalizeString;

	[SerializeField]
	private LocalizeStringEvent m_valueLocalizeString;

	[SerializeField]
	private Image[] m_selectedFadeElements;

	private readonly float m_normal_alpha = 0.4f;

	private readonly float m_disabled_alpha = 0.1f;

	private readonly float m_selected_alpha = 1f;

	private readonly float m_fadeElementsAlpha = 0.2f;

	private bool m_isPointerInside;

	protected List<string> m_rawStringOptions = new List<string>();

	protected List<LocalizedString> m_localizedStringOptions = new List<LocalizedString>();

	private int m_value;

	public UnityAction<int> m_onValueChanged;

	public UnityAction<bool> m_onValueChangedBool;

	private bool isSelected;

	public int OptionsCount
	{
		get
		{
			if (m_localizedStringOptions != null)
			{
				return m_localizedStringOptions.Count;
			}
			if (m_rawStringOptions != null)
			{
				return m_rawStringOptions.Count;
			}
			return 0;
		}
	}

	public int Value
	{
		get
		{
			return m_value;
		}
		set
		{
			value = Mathf.Clamp(value, 0, OptionsCount - 1);
			if (m_value != value)
			{
				m_value = value;
				UpdateDisplay();
			}
		}
	}

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
		Image[] selectedFadeElements = m_selectedFadeElements;
		foreach (Image obj in selectedFadeElements)
		{
			Color color = obj.color;
			color.a = m_fadeElementsAlpha;
			obj.color = color;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		InputState inputState = GlobalReferences.Instance.InputState;
		inputState.OnInputModeChanged = (UnityAction<InputState.Mode>)Delegate.Remove(inputState.OnInputModeChanged, new UnityAction<InputState.Mode>(InputModeChanged));
	}

	public void SetLabel(LocalizedString labelString)
	{
		m_labelLocalizeString.StringReference = labelString;
		m_labelLocalizeString.RefreshString();
	}

	private void InputModeChanged(InputState.Mode mode)
	{
		DoStateTransition(base.currentSelectionState, instant: true);
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
		bool flag = false;
		if (m_canvasGroup != null)
		{
			switch (state)
			{
			case SelectionState.Pressed:
			case SelectionState.Selected:
				m_canvasGroup.alpha = m_selected_alpha;
				flag = true;
				break;
			case SelectionState.Disabled:
				m_canvasGroup.alpha = m_disabled_alpha;
				break;
			default:
				m_canvasGroup.alpha = m_normal_alpha;
				break;
			}
		}
		Image[] selectedFadeElements = m_selectedFadeElements;
		foreach (Image obj in selectedFadeElements)
		{
			Color color = obj.color;
			color.a = (flag ? 1f : m_fadeElementsAlpha);
			obj.color = color;
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

	public void ClearOptions()
	{
		m_value = 0;
	}

	public void AddOptions(List<string> options)
	{
		m_rawStringOptions = options;
		m_localizedStringOptions = null;
		RefreshOption();
	}

	public void AddOptions(List<LocalizedString> options)
	{
		m_rawStringOptions = null;
		m_localizedStringOptions = options;
		RefreshOption();
	}

	protected virtual void RefreshOption()
	{
		Value = Mathf.Clamp(m_value, 0, OptionsCount - 1);
		UpdateDisplay();
		if (m_pips != null)
		{
			if (OptionsCount <= 8)
			{
				m_pips.SetPipsCount(OptionsCount);
				m_pips.SetSelectedPip(Value);
				m_pips.gameObject.SetActive(value: true);
			}
			else
			{
				m_pips.gameObject.SetActive(value: false);
			}
		}
	}

	private void CycleLeft()
	{
		if (base.interactable && OptionsCount != 0)
		{
			m_value = (m_value - 1 + OptionsCount) % OptionsCount;
			UpdateDisplay();
			m_onValueChanged?.Invoke(m_value);
			m_onValueChangedBool?.Invoke((m_value != 0) ? true : false);
		}
	}

	private void CycleRight()
	{
		if (base.interactable && OptionsCount != 0)
		{
			m_value = (m_value + 1) % OptionsCount;
			UpdateDisplay();
			m_onValueChanged?.Invoke(m_value);
			m_onValueChangedBool?.Invoke((m_value != 0) ? true : false);
		}
	}

	protected virtual void UpdateDisplay()
	{
		if (OptionsCount == 0)
		{
			return;
		}
		if (m_pips != null)
		{
			m_pips.SetSelectedPip(Value);
		}
		if (m_displayText != null)
		{
			if (m_rawStringOptions != null)
			{
				m_displayText.text = m_rawStringOptions[m_value].ToString();
				return;
			}
			m_valueLocalizeString.StringReference = m_localizedStringOptions[m_value];
			m_valueLocalizeString.RefreshString();
		}
	}

	public override void OnSelect(BaseEventData eventData)
	{
		base.OnSelect(eventData);
		isSelected = true;
	}

	public override void OnDeselect(BaseEventData eventData)
	{
		base.OnDeselect(eventData);
		isSelected = false;
	}

	public override void OnMove(AxisEventData eventData)
	{
		if (isSelected)
		{
			if (eventData.moveDir == MoveDirection.Left)
			{
				CycleLeft();
				eventData.Use();
			}
			else if (eventData.moveDir == MoveDirection.Right)
			{
				CycleRight();
				eventData.Use();
			}
			else
			{
				base.OnMove(eventData);
			}
		}
	}

	public override void OnPointerUp(PointerEventData eventData)
	{
		base.OnPointerUp(eventData);
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			if (RectTransformUtility.RectangleContainsScreenPoint(m_leftButton, eventData.position))
			{
				CycleLeft();
			}
			else if (RectTransformUtility.RectangleContainsScreenPoint(m_rightButton, eventData.position))
			{
				CycleRight();
			}
			else
			{
				CycleRight();
			}
		}
	}
}
