using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory(ActionCategory.Movement)]
public class GetCeilingPosition : FsmStateAction
{
	[RequiredField]
	[UIHint(UIHint.Variable)]
	[HutongGames.PlayMaker.Tooltip("Vector3 variable to set.")]
	public FsmVector3 m_vector3Variable;

	public float m_maxCeilingHeight = 6f;

	public FsmEvent m_foundCeilingEvent;

	public FsmEvent m_failedRaycastEvent;

	public float m_yOffset;

	public override void Awake()
	{
		base.Awake();
		_ = base.Owner == null;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		Vector2 origin = base.Owner.transform.position;
		origin.y += 0.25f;
		RaycastHit2D raycastHit2D = Physics2D.Raycast(origin, Vector2.up, m_maxCeilingHeight, GameLayers.EnvironmentMask);
		if ((bool)raycastHit2D)
		{
			Vector3 value = raycastHit2D.point;
			value.y += m_yOffset;
			value.z = base.Owner.transform.position.z;
			m_vector3Variable.Value = value;
			base.Fsm.Event(m_foundCeilingEvent);
		}
		else
		{
			base.Fsm.Event(m_failedRaycastEvent);
		}
		Finish();
	}
}
