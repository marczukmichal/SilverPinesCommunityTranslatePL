using System;
using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.UI;

public class MinigameInteractiveDial : BaseMinigameHoldInteract
{
	public enum GamepadMode
	{
		Rotate,
		Horizontal,
		Vertical
	}

	public enum KeyboardMouseMode
	{
		MouseHorizontal,
		MoveInputHorizontal,
		MoveInputVertical
	}

	[SerializeField]
	private float m_valueSensivity = 0.1f;

	[SerializeField]
	private float m_gamepadScalar = 1f;

	[SerializeField]
	private float m_mouseRotationScalar = 100f;

	[SerializeField]
	private float m_keyboardInputScalar = 1f;

	[SerializeField]
	private Image m_knob;

	[SerializeField]
	private GamepadMode m_gamepadMode;

	[SerializeField]
	private KeyboardMouseMode m_keyboardMouseMode;

	[Header("Input Prompt Settings")]
	[SerializeField]
	private LocalizedString m_inputPromptString;

	[Header("Animated Input Prompt")]
	[SerializeField]
	private CanvasGroup m_inputPromptContainer;

	[SerializeField]
	private AnimatedInputPrompt m_gamepadInputPrompt;

	[SerializeField]
	private AnimatedInputPrompt m_keyboardInputPrompt;

	[Header("Looping Audio")]
	[SerializeField]
	private EventReference m_loopingAudioEvent;

	[SerializeField]
	private float m_audioDecayRate = 1f;

	private EventInstance m_loopingAudioInstance;

	private float m_loopingAudioVolume;

	public UnityAction<float> OnDialRotated;

	private bool m_gamepadSetInitialValue;

	private Vector2 m_gamepadPreviousDirection = Vector2.zero;

	private void Start()
	{
		if (m_inputPromptContainer != null)
		{
			m_inputPromptContainer.alpha = 0f;
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		InputState inputState = GlobalReferences.Instance.InputState;
		inputState.OnInputModeChanged = (UnityAction<InputState.Mode>)Delegate.Combine(inputState.OnInputModeChanged, new UnityAction<InputState.Mode>(OnInputModeChanged));
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		InputState inputState = GlobalReferences.Instance.InputState;
		inputState.OnInputModeChanged = (UnityAction<InputState.Mode>)Delegate.Remove(inputState.OnInputModeChanged, new UnityAction<InputState.Mode>(OnInputModeChanged));
	}

	private void OnInputModeChanged(InputState.Mode mode)
	{
		UpdateShownInputPrompt();
	}

	protected override void OnHoldStart()
	{
		base.OnHoldStart();
		m_gamepadSetInitialValue = false;
		m_gamepadPreviousDirection = Vector2.zero;
		if (m_inputPromptContainer != null)
		{
			m_inputPromptContainer.gameObject.SetActive(value: true);
			DOTween.Kill(m_inputPromptContainer);
			m_inputPromptContainer.DOFade(1f, 0.5f).SetUpdate(isIndependentUpdate: true);
			UpdateShownInputPrompt();
		}
		if (!m_loopingAudioEvent.IsNull)
		{
			m_loopingAudioInstance = RuntimeManager.CreateInstance(m_loopingAudioEvent);
			SetLoopingAudioVolume(0f);
			m_loopingAudioInstance.start();
		}
		GameInputManager.GameInputActions.Player.Move.Enable();
	}

	private void UpdateShownInputPrompt()
	{
		bool flag = GlobalReferences.Instance.InputState.InputMode == InputState.Mode.Gamepad;
		m_gamepadInputPrompt.gameObject.SetActive(flag);
		m_keyboardInputPrompt.gameObject.SetActive(!flag);
	}

	protected override void OnHoldingInput()
	{
		base.OnHoldingInput();
		float num = 0f;
		if (GlobalReferences.Instance.InputState.InputMode == InputState.Mode.Gamepad)
		{
			Vector2 value = Gamepad.current.leftStick.value;
			switch (m_gamepadMode)
			{
			case GamepadMode.Horizontal:
				if (Mathf.Abs(value.x) > Mathf.Epsilon)
				{
					num = value.x * m_gamepadScalar;
				}
				break;
			case GamepadMode.Vertical:
				if (Mathf.Abs(value.y) > Mathf.Epsilon)
				{
					num = value.y * m_gamepadScalar;
				}
				break;
			case GamepadMode.Rotate:
				if (value.magnitude > 0.5f)
				{
					Vector2 normalized = value.normalized;
					float num2 = Vector2.SignedAngle(normalized, m_gamepadPreviousDirection);
					if (m_gamepadSetInitialValue)
					{
						num = num2 * m_gamepadScalar;
					}
					m_gamepadSetInitialValue = true;
					m_gamepadPreviousDirection = normalized;
				}
				else
				{
					m_gamepadSetInitialValue = false;
				}
				break;
			}
		}
		else
		{
			switch (m_keyboardMouseMode)
			{
			case KeyboardMouseMode.MouseHorizontal:
				num = Mouse.current.delta.x.value / (float)Screen.width * m_mouseRotationScalar;
				break;
			case KeyboardMouseMode.MoveInputHorizontal:
				num = GameInputManager.GameInputActions.Player.Move.ReadValue<Vector2>().x * m_keyboardInputScalar;
				break;
			case KeyboardMouseMode.MoveInputVertical:
				num = GameInputManager.GameInputActions.Player.Move.ReadValue<Vector2>().y * m_keyboardInputScalar;
				break;
			}
		}
		if (Mathf.Abs(num) > Mathf.Epsilon)
		{
			SetLoopingAudioVolume(1f);
			m_knob.transform.Rotate(0f, 0f, 0f - num);
			OnDialRotated?.Invoke(num * m_valueSensivity);
		}
		else
		{
			m_loopingAudioVolume -= Time.deltaTime * m_audioDecayRate;
			m_loopingAudioVolume = Mathf.Max(0f, m_loopingAudioVolume);
			SetLoopingAudioVolume(m_loopingAudioVolume);
		}
	}

	private void SetLoopingAudioVolume(float volume)
	{
		if (m_loopingAudioInstance.isValid())
		{
			m_loopingAudioVolume = volume;
			m_loopingAudioInstance.setVolume(m_loopingAudioVolume);
		}
	}

	protected override void OnHoldEnd()
	{
		base.OnHoldEnd();
		if (m_inputPromptContainer != null)
		{
			DOTween.Kill(m_inputPromptContainer);
			m_inputPromptContainer.DOFade(0f, 0.5f).SetUpdate(isIndependentUpdate: true);
		}
		if (m_loopingAudioInstance.isValid())
		{
			m_loopingAudioInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			m_loopingAudioInstance.clearHandle();
		}
	}

	public override void PopulateCustomInputs(ref MinigameHoldCustomInputPrompt action1, ref MinigameHoldCustomInputPrompt action2)
	{
		if (m_interacting)
		{
			action1.m_inputAction = GameInputManager.GameInputActions.Minigame.HorizontalMove;
			action1.m_inputString = m_inputPromptString;
		}
	}
}
