using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Localization;

public abstract class BaseMinigameHoldInteract : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, ICursorOverrides
{
	public struct MinigameHoldCustomInputPrompt
	{
		public InputAction m_inputAction;

		public LocalizedString m_inputString;
	}

	[SerializeField]
	private bool m_alwaysActive;

	public UnityAction<BaseMinigameHoldInteract> OnStartHoldInteractEvent;

	public UnityAction OnInputOptionsChangedEvent;

	[SerializeField]
	protected AudioEvent m_startHoldAudioEvent;

	[SerializeField]
	protected AudioEvent m_stopHoldAudioEvent;

	protected bool m_interacting;

	protected Vector2 m_previousMousePosition;

	private bool m_disableInteract;

	public ICursorOverrides.CursorOverrideOption ShouldShowCursor => ICursorOverrides.CursorOverrideOption.ForceOff;

	public void DisableInteract()
	{
		m_disableInteract = true;
		if (m_interacting)
		{
			StopInteract();
		}
	}

	public virtual void PopulateCustomInputs(ref MinigameHoldCustomInputPrompt action1, ref MinigameHoldCustomInputPrompt action2)
	{
	}

	protected virtual void OnHoldStart()
	{
		OnStartHoldInteractEvent?.Invoke(this);
		if (m_startHoldAudioEvent != null)
		{
			m_startHoldAudioEvent.Play2D();
		}
	}

	protected virtual void OnHoldEnd()
	{
		OnStartHoldInteractEvent?.Invoke(this);
		if (m_stopHoldAudioEvent != null)
		{
			m_stopHoldAudioEvent.Play2D();
		}
	}

	protected virtual void OnHoldingInput()
	{
	}

	protected virtual Vector2 GetMouseRestorePosition()
	{
		return m_previousMousePosition;
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (!m_alwaysActive && !m_disableInteract)
		{
			StartInteract();
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (!m_alwaysActive)
		{
			StopInteract();
		}
	}

	private void StartInteract()
	{
		m_interacting = true;
		m_previousMousePosition = Mouse.current.position.value;
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
		GlobalReferences.Instance.Anchors.Input.CursorOverridesAnchor.Set(this);
		OnHoldStart();
	}

	private void StopInteract()
	{
		GlobalReferences.Instance.Anchors.Input.CursorOverridesAnchor.Set(null);
		m_interacting = false;
		Cursor.lockState = CursorLockMode.None;
		OnHoldEnd();
		StartCoroutine(RestoreCursorPosition(GetMouseRestorePosition()));
	}

	protected virtual void OnEnable()
	{
		if (m_alwaysActive)
		{
			StartInteract();
		}
	}

	protected virtual void OnDisable()
	{
		if (m_interacting)
		{
			m_interacting = false;
			GlobalReferences.Instance.Anchors.Input.CursorOverridesAnchor.Set(null);
		}
	}

	private IEnumerator RestoreCursorPosition(Vector2 mousePosition)
	{
		Mouse.current.WarpCursorPosition(mousePosition);
		yield return new WaitForEndOfFrame();
		Mouse.current.WarpCursorPosition(mousePosition);
		Cursor.visible = true;
	}

	protected virtual void Update()
	{
		if (m_interacting)
		{
			OnHoldingInput();
		}
	}
}
