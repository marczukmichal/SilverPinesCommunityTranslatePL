using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("AI")]
public class RequestAttack : FsmStateAction
{
	public bool m_isRequestingAttack;

	private AIAttackStrategy m_aiAttackStrategy;

	private float m_range;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_aiAttackStrategy = base.Owner.GetComponent<AIAttackStrategy>();
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_aiAttackStrategy.SetRequestingAttack(m_isRequestingAttack);
		Finish();
	}
}
