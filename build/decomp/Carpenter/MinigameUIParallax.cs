using UnityEngine;
using UnityEngine.InputSystem;

public class MinigameUIParallax : MonoBehaviour
{
	[SerializeField]
	private float m_parallaxStrength = 1f;

	private float m_smoothing = 5f;

	private GamepadCursorController m_cursorController;

	private Vector3 m_initialPosition;

	private Vector3 GetCursorPosition()
	{
		if (GlobalReferences.Instance.InputState.InputMode == InputState.Mode.KeyboardMouse)
		{
			return Mouse.current.position.ReadValue();
		}
		return m_cursorController.CursorTransform.position;
	}

	private bool ShouldUpdateParallax()
	{
		if (GlobalReferences.Instance.InputState.InputMode == InputState.Mode.KeyboardMouse)
		{
			return CursorManager.IsShowingCursor;
		}
		return m_cursorController.IsVisible;
	}

	private void Start()
	{
		m_cursorController = Object.FindAnyObjectByType<GamepadCursorController>();
		m_initialPosition = base.transform.localPosition;
		Vector3 cursorPosition = GetCursorPosition();
		Vector3 vector = new Vector3((float)Screen.width * 0.5f, (float)Screen.height * 0.5f, 0f);
		Vector3 vector2 = (cursorPosition - vector) * m_parallaxStrength;
		base.transform.localPosition = m_initialPosition + vector2;
	}

	private void Update()
	{
		if (ShouldUpdateParallax())
		{
			Vector3 cursorPosition = GetCursorPosition();
			Vector3 vector = new Vector3((float)Screen.width * 0.5f, (float)Screen.height * 0.5f, 0f);
			Vector3 vector2 = (cursorPosition - vector) * m_parallaxStrength;
			Vector3 localPosition = Vector3.Lerp(base.transform.localPosition, m_initialPosition + vector2, m_smoothing * Time.unscaledDeltaTime);
			base.transform.localPosition = localPosition;
		}
	}
}
