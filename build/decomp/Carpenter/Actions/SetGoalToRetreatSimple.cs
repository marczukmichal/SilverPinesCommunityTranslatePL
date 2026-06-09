using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("AI")]
public class SetGoalToRetreatSimple : FsmStateAction
{
	private AISenses m_aiSenses;

	private AIFSMAttackStrategy m_strategy;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_aiSenses = base.Owner.GetComponent<AISenses>();
			m_strategy = base.Owner.GetComponent<AIFSMAttackStrategy>();
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_strategy.SetRetreatMode(enabled: true);
	}

	public override void OnExit()
	{
		base.OnExit();
		m_strategy.SetRetreatMode(enabled: false);
	}

	public override void OnUpdate()
	{
		if ((bool)m_aiSenses.CurrentTarget)
		{
			Vector2 vector = m_aiSenses.CurrentTarget.transform.position;
			Vector2 vector2 = base.Owner.transform.position;
			Vector2 vector3 = vector - vector2;
			vector3.Normalize();
			m_strategy.SetMoveGoal(vector2 - vector3);
		}
	}
}
