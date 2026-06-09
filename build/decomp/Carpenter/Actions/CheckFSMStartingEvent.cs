using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("PersistentData")]
public class CheckFSMStartingEvent : FsmStateAction
{
	private PlaymakerFSMPersistent m_fsmPersistentData;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_fsmPersistentData = base.Owner.GetComponent<PlaymakerFSMPersistent>();
		}
	}

	public override void OnEnter()
	{
		if (m_fsmPersistentData != null && !string.IsNullOrEmpty(m_fsmPersistentData.StartingEvent))
		{
			base.Fsm.Event(m_fsmPersistentData.StartingEvent);
		}
		Finish();
	}
}
