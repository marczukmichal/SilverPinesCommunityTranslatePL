using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("AI")]
public class GetTargetOffset : FsmStateAction
{
	[UIHint(UIHint.Variable)]
	[HutongGames.PlayMaker.Tooltip("Vector3 variable to set.")]
	public FsmVector3 m_vector3Variable;

	[UIHint(UIHint.Variable)]
	public FsmFloat m_xVariable;

	[UIHint(UIHint.Variable)]
	public FsmFloat m_yVariable;

	public Detectable.TargetArea m_area;

	public Vector3 m_offset;

	public bool m_everyFrame;

	public float m_randomVariance;

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
			Vector3 offset = m_offset;
			if (m_randomVariance > 0f)
			{
				offset.x += Random.Range(0f - m_randomVariance, m_randomVariance);
				offset.y += Random.Range(0f - m_randomVariance, m_randomVariance);
			}
			Vector3 value = m_aiSenses.CurrentTarget.GetAimPosition(m_area) + offset - base.Owner.transform.position;
			if (m_vector3Variable != null)
			{
				m_vector3Variable.Value = value;
			}
			if (m_xVariable != null)
			{
				m_xVariable.Value = Mathf.Abs(value.x);
			}
			if (m_yVariable != null)
			{
				m_yVariable.Value = value.y;
			}
		}
	}
}
