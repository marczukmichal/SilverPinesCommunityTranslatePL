using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("AI")]
public class SetGoalToRandomTransform : FsmStateAction
{
	[RequiredField]
	[UIHint(UIHint.Variable)]
	public FsmArray m_nodes;

	public float m_goalDistance = 0.5f;

	[HutongGames.PlayMaker.Tooltip("Event to trigger on reaching goal")]
	public FsmEvent m_onReachGoal;

	private AIFSMAttackStrategy m_strategy;

	private Vector2 m_position;

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
		GameObject gameObject = m_nodes.Get(Random.Range(0, m_nodes.Length)) as GameObject;
		m_position = gameObject.transform.position;
	}

	public override void OnUpdate()
	{
		if (m_strategy.SetGoalToPosition(m_position, m_goalDistance))
		{
			base.Fsm.Event(m_onReachGoal);
			Finish();
		}
	}
}
