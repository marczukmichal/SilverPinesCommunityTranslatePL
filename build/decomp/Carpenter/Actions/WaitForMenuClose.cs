using System;
using HutongGames.PlayMaker;
using UnityEngine.Events;

namespace Actions;

[ActionCategory("Special")]
public class WaitForMenuClose : FsmStateAction
{
	public GameMenuState m_gameMenuState;

	public FsmEvent m_onMenuClosedEvent;

	public override void OnEnter()
	{
		base.OnEnter();
		GameMenuState gameMenuState = m_gameMenuState;
		gameMenuState.OnGameMenuChanged = (UnityAction<GameMenuState.GameMenu>)Delegate.Combine(gameMenuState.OnGameMenuChanged, new UnityAction<GameMenuState.GameMenu>(OnGameMenuChanged));
		Finish();
	}

	public override void OnExit()
	{
		base.OnExit();
		GameMenuState gameMenuState = m_gameMenuState;
		gameMenuState.OnGameMenuChanged = (UnityAction<GameMenuState.GameMenu>)Delegate.Remove(gameMenuState.OnGameMenuChanged, new UnityAction<GameMenuState.GameMenu>(OnGameMenuChanged));
	}

	private void OnGameMenuChanged(GameMenuState.GameMenu menu)
	{
		if (menu == GameMenuState.GameMenu.None)
		{
			base.Fsm.Event(m_onMenuClosedEvent);
		}
	}
}
