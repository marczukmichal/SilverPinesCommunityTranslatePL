using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory(ActionCategory.Character)]
public class IsFacingPosition : FsmStateAction
{
	[RequiredField]
	[UIHint(UIHint.Variable)]
	[HutongGames.PlayMaker.Tooltip("The Vector3 variable of the target position.")]
	public FsmVector3 m_targetPosition;

	[HutongGames.PlayMaker.Tooltip("Event to trigger when facing target")]
	public FsmEvent m_facingTargetEvent;

	[HutongGames.PlayMaker.Tooltip("Event to trigger when facing target")]
	public FsmEvent m_notFacingTargetEvent;

	public float m_minimumTime;

	private CharacterDirection m_direction;

	private float m_facingTimer;

	private float m_notFacingTimer;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_direction = base.Owner.GetComponent<CharacterDirection>();
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_facingTimer = 0f;
		m_notFacingTimer = 0f;
		if (m_minimumTime <= 0f)
		{
			if (Check())
			{
				base.Fsm.Event(m_facingTargetEvent);
			}
			else
			{
				base.Fsm.Event(m_notFacingTargetEvent);
			}
		}
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (Check())
		{
			m_facingTimer += Time.deltaTime;
			m_notFacingTimer = 0f;
			if (m_facingTimer > m_minimumTime)
			{
				base.Fsm.Event(m_facingTargetEvent);
			}
		}
		else
		{
			m_facingTimer = 0f;
			m_notFacingTimer += Time.deltaTime;
			if (m_notFacingTimer > m_minimumTime)
			{
				base.Fsm.Event(m_notFacingTargetEvent);
			}
		}
	}

	private bool Check()
	{
		return m_direction.IsFacingPoint(m_targetPosition.Value);
	}
}
