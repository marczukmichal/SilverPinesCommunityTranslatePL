using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("AI")]
public class GetForwardPosition : FsmStateAction
{
	[RequiredField]
	[UIHint(UIHint.Variable)]
	[HutongGames.PlayMaker.Tooltip("Vector3 variable to set.")]
	public FsmVector3 m_vector3Variable;

	public Vector3 m_offset;

	public Vector2 m_randomVariance;

	private CharacterDirection m_direction;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_direction = base.Owner.GetComponent<CharacterDirection>();
		}
	}

	public override void OnEnter()
	{
		Check();
	}

	private void Check()
	{
		Vector3 offset = m_offset;
		offset.x += Random.Range((0f - m_randomVariance.x) * 0.5f, m_randomVariance.x * 0.5f);
		offset.y += Random.Range((0f - m_randomVariance.y) * 0.5f, m_randomVariance.y * 0.5f);
		Vector3 forwardVector = m_direction.GetForwardVector();
		offset.x *= forwardVector.x;
		m_vector3Variable.Value = base.Owner.transform.position + offset;
	}
}
