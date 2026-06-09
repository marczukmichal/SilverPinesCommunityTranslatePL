using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("Event Channels")]
public class VoidEventChannelListener : FsmStateAction
{
	public VoidGameEventChannel m_eventChannel;

	public FsmEvent m_onEvent;

	private bool m_registered;

	~VoidEventChannelListener()
	{
		if (m_registered)
		{
			m_eventChannel.Unregister(OnEvent);
		}
	}

	public override void OnEnter()
	{
		m_registered = true;
		m_eventChannel.Register(OnEvent);
	}

	public override void OnExit()
	{
		m_eventChannel.Unregister(OnEvent);
		m_registered = false;
	}

	private void OnEvent()
	{
		base.Fsm.Event(m_onEvent);
	}
}
