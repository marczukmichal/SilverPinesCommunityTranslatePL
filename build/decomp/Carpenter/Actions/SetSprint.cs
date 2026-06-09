using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("AI")]
public class SetSprint : FsmStateAction
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
		m_strategy.SetSprintflag(sprint: true);
		Finish();
	}

	public override void OnExit()
	{
		m_strategy.SetSprintflag(sprint: false);
	}
}
