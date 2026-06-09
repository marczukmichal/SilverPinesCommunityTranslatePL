using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("AI")]
public class ConfirmAIAbility : FsmStateAction
{
	public AIAbility m_aiAbility;

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
			m_strategy.ConfirmAbility(m_aiAbility);
			Finish();
		}
	}
}
