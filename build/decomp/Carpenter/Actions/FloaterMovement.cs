using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory(ActionCategory.Movement)]
public class FloaterMovement : FsmStateAction
{
	public FsmFloat m_force;

	private CharacterDirection m_direction;

	private Rigidbody2D m_rigidbody;

	private AISenses m_aiSenses;

	public override void Awake()
	{
		base.Awake();
		if (!(base.Owner == null))
		{
			m_direction = base.Owner.GetComponent<CharacterDirection>();
			m_rigidbody = base.Owner.GetComponent<Rigidbody2D>();
			m_aiSenses = base.Owner.GetComponent<AISenses>();
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
	}

	private Detectable GetTarget()
	{
		return m_aiSenses.CurrentTarget;
	}

	public override void OnUpdate()
	{
		Detectable target = GetTarget();
		Vector2 vector = Vector2.zero;
		if (target != null)
		{
			vector = (Vector2)(target.GetAimPosition(Detectable.TargetArea.Chest) - m_rigidbody.transform.position).normalized * m_force.Value;
		}
		if (vector != Vector2.zero)
		{
			m_direction.DesiredDirection = ((vector.x > 0f) ? CharacterDirection.Facing.Right : CharacterDirection.Facing.Left);
			m_rigidbody.AddForce(vector);
		}
	}

	public override void OnExit()
	{
		base.OnExit();
	}
}
