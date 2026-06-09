using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("Sync Grab")]
public class ActiveGrab : FsmStateAction
{
	public FsmEvent m_grabInterruptedEvent;

	public FsmEvent m_grabFinishedEvent;

	private CharacterSyncGrab m_characterSyncGrab;

	private CharacterDirection m_characterDirection;

	private CharacterSyncGrab m_instigator;

	private CharacterSyncGrab m_target;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_characterSyncGrab = base.Owner.GetComponent<CharacterSyncGrab>();
			m_characterDirection = base.Owner.GetComponent<CharacterDirection>();
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_instigator = m_characterSyncGrab.GetInstigator();
		m_target = m_characterSyncGrab.GetTarget();
		if (m_characterSyncGrab.IsInstigator() || !m_characterDirection)
		{
			return;
		}
		CharacterDirection component = m_instigator.GetComponent<CharacterDirection>();
		if (component != null)
		{
			if (component.CurrentDirection == CharacterDirection.Facing.Right)
			{
				m_characterDirection.CurrentDirection = CharacterDirection.Facing.Left;
			}
			else
			{
				m_characterDirection.CurrentDirection = CharacterDirection.Facing.Right;
			}
		}
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (!m_characterSyncGrab.IsInstigator())
		{
			m_characterSyncGrab.SnapToInstigator();
		}
		if (m_characterSyncGrab.HasGrabBeenInterrupted())
		{
			base.Fsm.Event(m_grabInterruptedEvent);
		}
		else if (m_characterSyncGrab.ShouldEndGrab())
		{
			base.Fsm.Event(m_grabFinishedEvent);
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		m_characterSyncGrab.ExitGrab();
	}
}
