using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("Combat")]
public class EnableStumbleCollider : FsmStateAction
{
	public StumbleCollider m_stumbleCollider;

	public bool m_resetOnExit = true;

	public bool m_activate = true;

	public override void OnEnter()
	{
		base.OnEnter();
		m_stumbleCollider.StumbleActive = m_activate;
		Finish();
	}

	public override void OnExit()
	{
		if (m_resetOnExit)
		{
			m_stumbleCollider.StumbleActive = !m_activate;
		}
	}
}
