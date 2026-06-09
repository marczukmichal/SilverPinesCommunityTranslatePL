using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("Event Channels")]
public class RaiseVoidEventChannel : FsmStateAction
{
	public VoidGameEventChannel m_eventChannel;

	public bool m_onEnter;

	public bool m_onExit;

	public override void OnEnter()
	{
		if (m_onEnter)
		{
			m_eventChannel.Raise();
		}
		Finish();
	}

	public override void OnExit()
	{
		if (m_onExit)
		{
			m_eventChannel.Raise();
		}
	}
}
