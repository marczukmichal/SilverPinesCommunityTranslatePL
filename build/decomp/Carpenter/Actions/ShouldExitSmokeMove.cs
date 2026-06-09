using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("Special")]
public class ShouldExitSmokeMove : FsmStateAction
{
	public FsmEvent m_onExitEvent;

	public float m_minimumTime = 1f;

	public float m_heightThreshold = 0.3f;

	private CharacterMovement m_movement;

	private AISenses m_senses;

	private Collider2D[] results = new Collider2D[32];

	private float m_timeInState;

	public override void Awake()
	{
		base.Awake();
		if (!(base.Owner == null))
		{
			m_movement = base.Owner.GetComponent<CharacterMovement>();
			m_senses = base.Owner.GetComponent<AISenses>();
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_timeInState = 0f;
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		m_timeInState += Time.deltaTime;
		bool flag = true;
		if (m_timeInState < m_minimumTime)
		{
			flag = false;
		}
		else if (IsOverlappingSomething())
		{
			flag = false;
		}
		else if (!IsAtTargetHeight())
		{
			flag = false;
		}
		if (flag)
		{
			base.Fsm.Event(m_onExitEvent);
		}
	}

	private bool IsOverlappingSomething()
	{
		ContactFilter2D contactFilter = default(ContactFilter2D);
		contactFilter.useLayerMask = true;
		contactFilter.layerMask = m_movement.CollisionMask;
		return Physics2D.OverlapCollider(m_movement.Collider, contactFilter, results) > 0;
	}

	private bool IsAtTargetHeight()
	{
		Vector2 vector = m_senses.CurrentTarget.transform.position;
		return Mathf.Abs(((Vector2)base.Owner.transform.position).y - vector.y) < m_heightThreshold;
	}
}
