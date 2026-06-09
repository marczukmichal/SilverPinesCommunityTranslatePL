using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("AI")]
public class IsTargetWithinRange : FsmStateAction
{
	public float m_randomMinRange;

	public float m_randomMaxRange;

	public bool m_allowEventsWhenTurning;

	public bool m_requireMatchingDirection;

	public float m_predictionLookAheadTime;

	[HutongGames.PlayMaker.Tooltip("Event to trigger when within range")]
	public FsmEvent m_inRangeEvent;

	[HutongGames.PlayMaker.Tooltip("Event to trigger when not within range")]
	public FsmEvent m_outRangeEvent;

	private AISenses m_aiSenses;

	private CharacterDirection m_charDirection;

	private float m_range;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_aiSenses = base.Owner.GetComponent<AISenses>();
			m_charDirection = base.Owner.GetComponent<CharacterDirection>();
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_range = Random.Range(m_randomMinRange, m_randomMaxRange);
	}

	public override void OnUpdate()
	{
		if ((m_charDirection.IsTurning && !m_allowEventsWhenTurning) || !(m_aiSenses.CurrentTarget != null))
		{
			return;
		}
		Vector2 vector = m_aiSenses.CurrentTarget.transform.position;
		CharacterMovement component = m_aiSenses.CurrentTarget.GetComponent<CharacterMovement>();
		if (component != null)
		{
			vector += component.PreviousVelocity * m_predictionLookAheadTime;
		}
		Vector2 a = base.Owner.transform.position;
		CharacterMovement component2 = base.Owner.GetComponent<CharacterMovement>();
		if (component2 != null)
		{
			a += component2.PreviousVelocity * m_predictionLookAheadTime;
		}
		if (Vector2.Distance(a, vector) < m_range)
		{
			if (!m_requireMatchingDirection || m_charDirection.IsFacingPoint(vector))
			{
				base.Fsm.Event(m_inRangeEvent);
			}
		}
		else
		{
			base.Fsm.Event(m_outRangeEvent);
		}
	}
}
