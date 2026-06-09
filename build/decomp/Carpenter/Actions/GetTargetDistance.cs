using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("AI")]
public class GetTargetDistance : FsmStateAction
{
	[RequiredField]
	[UIHint(UIHint.Variable)]
	public FsmFloat m_floatVariable;

	public float m_lookAheadTime;

	public bool m_ignoreHeight;

	public bool m_everyFrame;

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
		if (m_everyFrame)
		{
			Check();
		}
	}

	private void Check()
	{
		if ((bool)m_aiSenses && m_aiSenses.CurrentTarget != null)
		{
			m_floatVariable.Value = GetDistanceFromAttackTarget(m_aiSenses.CurrentTarget, m_lookAheadTime, m_ignoreHeight);
		}
	}

	private float GetDistanceFromAttackTarget(Detectable currentTarget, float lookaheadTime, bool ignoreHeight)
	{
		Vector2 b = currentTarget.transform.position;
		CharacterMovement component = currentTarget.GetComponent<CharacterMovement>();
		if (component != null)
		{
			b += component.PredictedPositionOffsetSmoothed * lookaheadTime;
		}
		else
		{
			Rigidbody2D component2 = currentTarget.GetComponent<Rigidbody2D>();
			if (component2 != null)
			{
				b += component2.linearVelocity * lookaheadTime;
			}
		}
		Vector2 a = base.Owner.transform.position;
		if (ignoreHeight)
		{
			a.y = 0f;
			b.y = 0f;
		}
		return Vector2.Distance(a, b);
	}
}
