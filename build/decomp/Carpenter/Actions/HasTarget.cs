using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("AI")]
public class HasTarget : FsmStateAction
{
	[HutongGames.PlayMaker.Tooltip("Event to trigger if has target")]
	public FsmEvent m_hasTarget;

	[HutongGames.PlayMaker.Tooltip("Event to trigger if is suspicious")]
	public FsmEvent m_isSuspicious;

	[HutongGames.PlayMaker.Tooltip("Event to trigger if has a target")]
	public FsmEvent m_hasNoTarget;

	private AISenses m_aiSenses;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_aiSenses = base.Owner.GetComponent<AISenses>();
		}
	}

	public override void OnEnter()
	{
		Check();
	}

	public override void OnUpdate()
	{
		Check();
	}

	private void Check()
	{
		if (!m_aiSenses)
		{
			Debug.LogWarning("Has Target node used but character has no AISesnse component! " + base.Owner.name, base.Owner);
			if (m_hasNoTarget != null)
			{
				base.Fsm.Event(m_hasNoTarget);
			}
			return;
		}
		switch (m_aiSenses.CurrentTargetState)
		{
		case AISenses.TargetState.None:
			if (m_hasNoTarget != null)
			{
				base.Fsm.Event(m_hasNoTarget);
			}
			break;
		case AISenses.TargetState.Suspicious:
			if (m_isSuspicious != null)
			{
				base.Fsm.Event(m_isSuspicious);
			}
			break;
		case AISenses.TargetState.Detected:
			if (m_hasTarget != null)
			{
				base.Fsm.Event(m_hasTarget);
			}
			break;
		}
	}
}
