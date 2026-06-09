using System;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Misc/Game State")]
public class GameState : ScriptableObject
{
	[Flags]
	public enum GameStateFlag
	{
		None = 0,
		GameActive = 1
	}

	private GameStateFlag m_activeState;

	public UnityAction<GameStateFlag> OnGameStateFlagChanged;

	public void SetStateFlag(GameStateFlag state)
	{
		GameStateFlag activeState = m_activeState;
		m_activeState |= state;
		if (m_activeState != activeState)
		{
			OnGameStateFlagChanged?.Invoke(m_activeState);
		}
	}

	public void ClearStateFlag(GameStateFlag state)
	{
		GameStateFlag activeState = m_activeState;
		m_activeState &= ~state;
		if (m_activeState != activeState)
		{
			OnGameStateFlagChanged?.Invoke(m_activeState);
		}
	}

	public bool IsInState(GameStateFlag state)
	{
		return m_activeState.HasFlag(state);
	}

	private void OnEnable()
	{
		m_activeState = GameStateFlag.None;
	}
}
