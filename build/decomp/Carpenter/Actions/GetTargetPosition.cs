using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("AI")]
public class GetTargetPosition : FsmStateAction
{
	[RequiredField]
	[UIHint(UIHint.Variable)]
	[HutongGames.PlayMaker.Tooltip("Vector3 variable to set.")]
	public FsmVector3 m_vector3Variable;

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

	private Vector3 GetPositionFromDetectable(Detectable detectable)
	{
		Vector3 offset = m_offset;
		if (m_randomVariance > 0f)
		{
			offset.x += Random.Range(0f - m_randomVariance, m_randomVariance);
			offset.y += Random.Range(0f - m_randomVariance, m_randomVariance);
		}
		return detectable.GetAimPosition(m_area) + offset;
	}

	private void Check()
	{
		if ((bool)m_aiSenses)
		{
			if (m_aiSenses.CurrentTarget != null)
			{
				m_vector3Variable.Value = GetPositionFromDetectable(m_aiSenses.CurrentTarget);
			}
			return;
		}
		GameObject item = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item;
		if (item != null)
		{
			Detectable component = item.GetComponent<Detectable>();
			if (component != null)
			{
				m_vector3Variable.Value = GetPositionFromDetectable(component);
			}
		}
	}
}
