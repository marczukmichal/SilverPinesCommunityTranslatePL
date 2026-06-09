using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("Combat")]
public class EnableDamageCollider : FsmStateAction
{
	public DamageCollider m_damageCollider;

	public bool m_resetOnExit = true;

	public bool m_activate = true;

	public override void OnEnter()
	{
		base.OnEnter();
		m_damageCollider.SetDamageEnabled(m_activate);
		if (m_activate)
		{
			m_damageCollider.gameObject.SetActive(value: true);
		}
		Finish();
	}

	public override void OnExit()
	{
		if (m_resetOnExit)
		{
			m_damageCollider.SetDamageEnabled(!m_activate);
		}
	}
}
