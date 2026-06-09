using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class SliderExtended : Selectable
{
	[SerializeField]
	private CanvasGroup m_canvasGroup;

	[SerializeField]
	private RectTransform m_leftButton;

	[SerializeField]
	private RectTransform m_rightButton;

	[SerializeField]
	private Image[] m_selectedFadeElements;

	[SerializeField]
	private TextMeshProUGUI m_valueLabel;

	[SerializeField]
	private Slider m_slider;

	public UnityAction<float> m_onExtendedValueChanged;

	public UnityAction m_onPlayAudioTrigger;

	private bool m_isPointerInside;

	private readonly float m_normal_alpha = 0.4f;

	private readonly float m_disabled_alpha = 0.1f;

	private readonly float m_selected_alpha = 1f;

	private readonly float m_fadeElementsAlpha = 0.2f;

	private SliderValueType m_sliderValueType;

	private float m_lastPlayedAudioTime;

	private float m_minValue;

	private float m_maxValue;

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

	public void Setup(SliderValueType sliderValueType, float minimumValue, float maximumValue, float startingValue, int stepCount)
	{
		m_sliderValueType = sliderValueType;
		m_slider.onValueChanged.AddListener(OnSliderValueChanged);
		m_minValue = minimumValue;
		m_maxValue = maximumValue;
		m_slider.wholeNumbers = true;
		m_slider.minValue = 0f;
		m_slider.maxValue = stepCount;
		m_slider.SetValueWithoutNotify(Mathf.Round(Mathf.InverseLerp(minimumValue, maximumValue, startingValue) * m_slider.maxValue));
		OnSliderValueChanged(m_slider.value);
	}

	public void SetValue(float newValue)
	{
		m_slider.value = Mathf.Round(Mathf.InverseLerp(m_minValue, m_maxValue, newValue) * m_slider.maxValue);
	}

	private void OnSliderValueChanged(float newValue)
	{
		float num = Mathf.Lerp(m_minValue, m_maxValue, newValue / m_slider.maxValue);
		switch (m_sliderValueType)
		{
		case SliderValueType.Normal:
			m_valueLabel.text = num.ToString("0.##");
			break;
		case SliderValueType.Percentilex100:
			m_valueLabel.text = (num * 100f).ToString("0.") + "%";
			break;
		}
		m_onExtendedValueChanged?.Invoke(num);
		if (Time.unscaledTime - m_lastPlayedAudioTime > 0.2f)
		{
			m_onPlayAudioTrigger?.Invoke();
			m_lastPlayedAudioTime = Time.unscaledTime;
		}
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

	public override void OnPointerUp(PointerEventData eventData)
	{
		base.OnPointerUp(eventData);
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			if (RectTransformUtility.RectangleContainsScreenPoint(m_leftButton, eventData.position))
			{
				m_slider.value--;
			}
			else if (RectTransformUtility.RectangleContainsScreenPoint(m_rightButton, eventData.position))
			{
				m_slider.value++;
			}
		}
	}

	public override void OnMove(AxisEventData eventData)
	{
		if (eventData.moveDir == MoveDirection.Left)
		{
			m_slider.value--;
			eventData.Use();
		}
		else if (eventData.moveDir == MoveDirection.Right)
		{
			m_slider.value++;
			eventData.Use();
		}
		else
		{
			base.OnMove(eventData);
		}
	}

	private void InputModeChanged(InputState.Mode mode)
	{
		DoStateTransition(base.currentSelectionState, instant: true);
	}
}
