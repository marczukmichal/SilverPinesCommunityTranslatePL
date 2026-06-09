using UnityEngine;
using UnityEngine.Events;

public class SideDoorLockVisuals : MonoBehaviour
{
	public enum LockVisualsType
	{
		None,
		Keypad,
		BreakablePadlock
	}

	[SerializeField]
	private LockVisualsType m_visualsType;

	[SerializeField]
	private UnityEvent m_lockedEvent;

	[SerializeField]
	private UnityEvent m_unlockedEvent;

	public UnityAction OnLockBroken;

	public LockVisualsType VisualsType => m_visualsType;

	public void SetUnlockedState(bool unlocked)
	{
		if (unlocked)
		{
			m_unlockedEvent.Invoke();
		}
		else
		{
			m_lockedEvent.Invoke();
		}
	}

	public void SetLockBroken()
	{
		OnLockBroken?.Invoke();
	}
}
