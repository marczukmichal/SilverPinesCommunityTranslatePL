using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("AI")]
public class AIUseAbilityControlAction : BaseAIFSMControlAction, IAIBrainControlAction
{
	public bool m_shouldSprint;

	public bool m_moveTowardsTarget;

	public AIAbility m_aiAbility;

	private AISenses m_aiSenses;

	public override Vector2 MoveDirection
	{
		get
		{
			if (m_moveTowardsTarget)
			{
				if (m_aiSenses != null && m_aiSenses.CurrentTarget != null)
				{
					Vector2 vector = m_aiSenses.transform.position - base.Owner.transform.position;
					vector.y = 0f;
					return vector.normalized;
				}
				return Vector2.zero;
			}
			return Vector2.zero;
		}
	}

	public override void Awake()
	{
		base.Awake();
		if (base.Owner != null)
		{
			m_aiSenses = base.Owner.GetComponent<AISenses>();
		}
	}

	public bool IsDoingAIAbility(AIAbility ability)
	{
		return ability == m_aiAbility;
	}
}
