using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("Combat")]
public class BlockHitReact : FsmStateAction
{
	public bool m_useStatelessHitReact;

	public bool m_disableOnExitOnly;

	private CharacterHitReact m_hitReact;

	public override void Awake()
	{
		base.Awake();
		if (base.Owner != null)
		{
			m_hitReact = base.Owner.GetComponent<CharacterHitReact>();
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
		if (!m_disableOnExitOnly)
		{
			m_hitReact.HitReactsEventsBlocked = true;
			if (m_useStatelessHitReact)
			{
				m_hitReact.StatelessHitReactEnabled = true;
			}
		}
		Finish();
	}

	public override void OnExit()
	{
		base.OnExit();
		m_hitReact.HitReactsEventsBlocked = false;
		if (m_useStatelessHitReact)
		{
			m_hitReact.StatelessHitReactEnabled = false;
		}
	}
}
