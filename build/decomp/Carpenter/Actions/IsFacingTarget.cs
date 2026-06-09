using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("AI")]
public class IsFacingTarget : FsmStateAction
{
	[HutongGames.PlayMaker.Tooltip("Event to trigger when stuck facing target")]
	public FsmEvent m_facingTargetEvent;

	[HutongGames.PlayMaker.Tooltip("Event to trigger when not facing target")]
	public FsmEvent m_notFacingTargetEvent;

	private CharacterDirection m_direction;

	private AISenses m_aiSenses;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_direction = base.Owner.GetComponent<CharacterDirection>();
			m_aiSenses = base.Owner.GetComponent<AISenses>();
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
		bool flag = false;
		if (m_aiSenses.CurrentTarget != null)
		{
			Vector2 position = m_aiSenses.CurrentTarget.transform.position;
			if (m_direction.IsFacingPoint(position))
			{
				flag = true;
			}
		}
		if (flag)
		{
			base.Fsm.Event(m_facingTargetEvent);
		}
		else
		{
			base.Fsm.Event(m_notFacingTargetEvent);
		}
	}
}
