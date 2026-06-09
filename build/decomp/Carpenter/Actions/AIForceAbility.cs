using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("AI")]
public class AIForceAbility : FsmStateAction
{
	private AIFSMAttackStrategy m_strategy;

	public string m_abilityName;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_strategy = base.Owner.GetComponent<AIFSMAttackStrategy>();
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_strategy.ForceAbility(m_abilityName);
		Finish();
	}
}
