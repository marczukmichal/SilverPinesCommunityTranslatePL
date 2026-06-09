using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory(ActionCategory.Character)]
public class IsGrounded : FsmStateAction
{
	[HutongGames.PlayMaker.Tooltip("Event to trigger if touching the ground and not falling")]
	public FsmEvent m_onGroundedEvent;

	public float m_minAllowedVelocity = -0.1f;

	public float m_maxAllowedVelocity = 0.1f;

	public bool m_onlyOnce;

	private CharacterMovement m_characterMovement;

	private Rigidbody2D m_rigidbody2D;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_rigidbody2D = base.Owner.GetComponent<Rigidbody2D>();
			m_characterMovement = base.Owner.GetComponent<CharacterMovement>();
		}
	}

	public override void OnEnter()
	{
		Check();
		if (m_onlyOnce)
		{
			Finish();
		}
	}

	public override void OnUpdate()
	{
		Check();
	}

	private void Check()
	{
		bool flag = !m_characterMovement.IsForceFall && m_characterMovement.IsGrounded && m_characterMovement.PreviousVelocity.y >= m_minAllowedVelocity && m_characterMovement.PreviousVelocity.y <= m_maxAllowedVelocity;
		if (m_onGroundedEvent != null && flag)
		{
			base.Fsm.Event(m_onGroundedEvent);
		}
	}
}
