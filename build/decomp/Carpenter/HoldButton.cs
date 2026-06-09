using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HoldButton : Selectable
{
	[SerializeField]
	private float m_holdTime = 1f;

	[SerializeField]
	private Slider m_fillSlider;

	[SerializeField]
	private UnityEvent m_onHeldEvent;

	public UnityAction m_onClick;

	private float m_timer;

	private bool m_isHeld;

	private bool m_isHoldButtonEnabled;

	public bool HoldButtonEnabled
	{
		get
		{
			return m_isHoldButtonEnabled;
		}
		set
		{
			m_isHoldButtonEnabled = value;
		}
	}

	protected HoldButton()
	{
	}

	protected override void Start()
	{
		base.Start();
		m_isHoldButtonEnabled = base.interactable;
	}

	private void Update()
	{
		if (m_isHeld)
		{
			float timer = m_timer;
			m_timer += Time.unscaledDeltaTime;
			if ((double)m_holdTime > 0.0 && m_timer >= m_holdTime && timer < m_holdTime)
			{
				m_timer = 0f;
				m_isHeld = false;
				m_onClick?.Invoke();
			}
			m_onHeldEvent.Invoke();
		}
		else
		{
			m_timer = 0f;
		}
		UpdateSlider();
	}

	private void UpdateSlider()
	{
		if (!(m_fillSlider == null))
		{
			m_fillSlider.gameObject.SetActive(m_timer > 0f);
			if (m_timer > 0f)
			{
				m_fillSlider.value = Mathf.Clamp01(m_timer / m_holdTime);
			}
		}
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		base.OnPointerDown(eventData);
		if (m_isHoldButtonEnabled && IsActive() && IsInteractable())
		{
			m_isHeld = true;
		}
	}

	public override void OnPointerUp(PointerEventData eventData)
	{
		base.OnPointerUp(eventData);
		if (m_isHoldButtonEnabled && IsActive() && IsInteractable())
		{
			m_isHeld = false;
		}
	}

	public override void OnSelect(BaseEventData eventData)
	{
		base.OnSelect(eventData);
		if (m_isHoldButtonEnabled)
		{
			GameInputManager.GameInputActions.UI.Submit.performed += SubmitInput;
			GameInputManager.GameInputActions.UI.Submit.canceled += CancelInput;
		}
	}

	public override void OnDeselect(BaseEventData eventData)
	{
		base.OnDeselect(eventData);
		if (m_isHoldButtonEnabled)
		{
			m_isHeld = false;
			GameInputManager.GameInputActions.UI.Submit.performed -= SubmitInput;
			GameInputManager.GameInputActions.UI.Submit.canceled -= CancelInput;
		}
	}

	private void SubmitInput(InputAction.CallbackContext callback)
	{
		m_isHeld = true;
	}

	private void CancelInput(InputAction.CallbackContext callback)
	{
		m_isHeld = false;
	}
}
