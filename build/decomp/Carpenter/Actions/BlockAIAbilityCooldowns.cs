using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("AI")]
public class BlockAIAbilityCooldowns : FsmStateAction
{
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
			m_strategy.BlockCooldowns = true;
			Finish();
		}
	}

	public override void OnExit()
	{
		if (!(m_strategy == null))
		{
			m_strategy.BlockCooldowns = false;
		}
	}
}
