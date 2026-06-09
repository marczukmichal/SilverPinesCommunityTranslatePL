using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("AI")]
public class SetAllAbilityCooldown : FsmStateAction
{
	public float m_cooldown;

	public bool m_onEnter;

	public bool m_onExit;

	private AIFSMAttackStrategy m_strategy;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_strategy = base.Owner.GetComponent<AIFSMAttackStrategy>();
		}
	}

	public override void OnEnter()
	{
		if (!(m_strategy == null))
		{
			if (m_onEnter)
			{
				m_strategy.CharacterCooldownTimer = m_cooldown;
			}
			Finish();
		}
	}

	public override void OnExit()
	{
		if (!(m_strategy == null) && m_onExit)
		{
			m_strategy.CharacterCooldownTimer = m_cooldown;
		}
	}
}
