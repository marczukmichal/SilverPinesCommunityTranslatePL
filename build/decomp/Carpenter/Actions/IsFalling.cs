using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory(ActionCategory.Character)]
public class IsFalling : FsmStateAction
{
	[HutongGames.PlayMaker.Tooltip("Event to trigger if falling")]
	public FsmEvent m_onFallingEvent;

	[HutongGames.PlayMaker.Tooltip("Event to trigger if not falling")]
	public FsmEvent m_onNotFallingEvent;

	private CharacterMovement m_characterMovement;

	public float m_requiredTimeThreshold = 0.1f;

	private float m_notFallingTime;

	private float m_isFallingTime;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_characterMovement = base.Owner.GetComponent<CharacterMovement>();
		}
	}

	public override void OnEnter()
	{
		Check();
	}

	public override void OnUpdate()
	{
		Check();
	}

	private void Check()
	{
		if (m_characterMovement != null)
		{
			bool flag = m_characterMovement.IsForceFall || (!m_characterMovement.IsGrounded && m_characterMovement.PreviousVelocity.y < -0.01f);
			if (flag)
			{
				m_isFallingTime += Time.deltaTime;
				m_notFallingTime = 0f;
			}
			else
			{
				m_notFallingTime += Time.deltaTime;
				m_isFallingTime = 0f;
			}
			if (m_onFallingEvent != null && flag && m_isFallingTime > m_requiredTimeThreshold)
			{
				base.Fsm.Event(m_onFallingEvent);
			}
			if (m_onNotFallingEvent != null && !flag && m_notFallingTime > m_requiredTimeThreshold)
			{
				base.Fsm.Event(m_onNotFallingEvent);
			}
		}
	}
}
