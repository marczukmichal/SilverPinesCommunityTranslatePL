using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("AI")]
public class AIAttackBehavior : FsmStateAction
{
	public enum CustomAbilityRequirementFlag
	{
		None,
		Custom1,
		Custom2
	}

	private AIFSMAttackStrategy m_strategy;

	public CustomAbilityRequirementFlag m_applyCustomAbilityRequirementFlag;

	public AIFSMAttackStrategy.AttackBehaviorMovementMode m_movementMode;

	public float m_updateMovementDelayTime;

	private float m_updateTimer;

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
		m_updateTimer = 0f;
		switch (m_applyCustomAbilityRequirementFlag)
		{
		case CustomAbilityRequirementFlag.Custom1:
			m_strategy.m_customRequirementFlag1 = true;
			break;
		case CustomAbilityRequirementFlag.Custom2:
			m_strategy.m_customRequirementFlag2 = true;
			break;
		}
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (m_updateTimer <= 0f)
		{
			m_updateTimer = m_updateMovementDelayTime;
			m_strategy.DoAttackBehavior(m_movementMode);
		}
		else
		{
			m_updateTimer -= Time.deltaTime;
			m_strategy.DoAttackBehavior(AIFSMAttackStrategy.AttackBehaviorMovementMode.NoMovementUpdate);
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		m_strategy.m_customRequirementFlag1 = false;
		m_strategy.m_customRequirementFlag2 = false;
		m_strategy.ExitAttackBehavior();
	}
}
