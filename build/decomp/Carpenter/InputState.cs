using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Misc/Input State")]
public class InputState : ScriptableObject
{
	public enum Mode
	{
		None,
		KeyboardMouse,
		Gamepad
	}

	private Mode m_mode;

	public UnityAction<Mode> OnInputModeChanged;

	public Mode InputMode
	{
		get
		{
			return m_mode;
		}
		set
		{
			if (m_mode != value)
			{
				m_mode = value;
				OnInputModeChanged?.Invoke(value);
			}
		}
	}
}
