using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("AI")]
public class SetGoalToTarget : FsmStateAction
{
	private AIFSMAttackStrategy m_strategy;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_strategy = base.Owner.GetComponent<AIFSMAttackStrategy>();
		}
	}

	public override void OnUpdate()
	{
		m_strategy.SetGoalToDirectorNavigationPos();
	}
}
