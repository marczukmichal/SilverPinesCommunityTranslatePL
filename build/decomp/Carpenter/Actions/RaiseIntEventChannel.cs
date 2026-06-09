using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("Event Channels")]
public class RaiseIntEventChannel : FsmStateAction
{
	public IntGameEventChannel m_eventChannel;

	public bool m_onEnter;

	public bool m_onExit;

	public int m_toSet;

	public override void OnEnter()
	{
		if (m_onEnter)
		{
			m_eventChannel.Raise(m_toSet);
		}
		Finish();
	}

	public override void OnExit()
	{
		if (m_onExit)
		{
			m_eventChannel.Raise(m_toSet);
		}
	}
}
