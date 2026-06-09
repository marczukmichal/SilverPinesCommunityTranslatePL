using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("AI")]
public class SetForcedMoveInput : FsmStateAction
{
	public Vector2 m_input;

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
		base.OnEnter();
		m_strategy.SetForceMoveInput(m_input);
	}

	public override void OnExit()
	{
		m_strategy.ResetForceMoveInput();
	}
}
