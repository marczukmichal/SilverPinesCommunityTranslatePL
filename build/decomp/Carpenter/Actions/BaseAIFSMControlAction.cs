using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("AI")]
public abstract class BaseAIFSMControlAction : FsmStateAction, IAIBrainControlAction
{
	public AIBrain m_aiBrain;

	public virtual Vector2 MoveDirection => Vector2.zero;

	public virtual CharacterDirection.Facing FacingDirectionInput => CharacterDirection.Facing.None;

	public virtual bool ShouldAttack => false;

	public virtual bool ShouldSprint => false;

	public virtual bool ShouldWalkBackwards => false;

	public virtual bool ShouldJump => false;

	public override void Awake()
	{
		base.Awake();
		if (base.Owner != null && m_aiBrain == null)
		{
			m_aiBrain = base.Owner.GetComponent<AIBrain>();
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_aiBrain.ActiveControlAction = this;
	}

	public override void OnExit()
	{
		base.OnExit();
		m_aiBrain.ActiveControlAction = null;
	}
}
