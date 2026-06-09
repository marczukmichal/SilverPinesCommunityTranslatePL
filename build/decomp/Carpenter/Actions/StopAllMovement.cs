using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory(ActionCategory.Movement)]
public class StopAllMovement : FsmStateAction
{
	protected CharacterMovement m_movement;

	protected Rigidbody2D m_rigidbody;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_movement = base.Owner.GetComponent<CharacterMovement>();
			m_rigidbody = base.Owner.GetComponent<Rigidbody2D>();
		}
	}

	protected void EnterStopMovement()
	{
		m_movement.MovementOverriden = true;
		m_movement.SetPreviousVelocity(Vector3.zero);
		if (m_rigidbody != null)
		{
			m_rigidbody.linearVelocity = Vector2.zero;
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
		EnterStopMovement();
		Finish();
	}

	public override void OnExit()
	{
		base.OnExit();
		m_movement.MovementOverriden = false;
		if (m_rigidbody != null)
		{
			m_rigidbody.linearVelocity = Vector2.zero;
		}
	}
}
