using System;
using HutongGames.PlayMaker;
using UnityEngine;
using UnityEngine.Events;

[ActionCategory(ActionCategory.Movement)]
public class CharacterMovementCollisionEvent : FsmStateAction
{
	[UIHint(UIHint.Variable)]
	public FsmVector2 m_relativeVelocity;

	public FsmEvent m_onLandOnGroundEvent;

	public FsmEvent m_onHorizontalCollisionEvent;

	public float m_minFallSpeedForFallEvent = 3f;

	protected LegacyCharacterMovement m_charMovement;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_charMovement = base.Owner.GetComponent<LegacyCharacterMovement>();
		}
	}

	public override void OnEnter()
	{
		Debug.LogWarning("Old CharacterMovementCollisionEvent used in state " + base.State.Name);
		if (m_charMovement != null)
		{
			LegacyCharacterMovement charMovement = m_charMovement;
			charMovement.m_onCollision = (UnityAction<Collision2D>)Delegate.Combine(charMovement.m_onCollision, new UnityAction<Collision2D>(OnCollision));
		}
		Finish();
	}

	public override void OnExit()
	{
		if (m_charMovement != null)
		{
			LegacyCharacterMovement charMovement = m_charMovement;
			charMovement.m_onCollision = (UnityAction<Collision2D>)Delegate.Remove(charMovement.m_onCollision, new UnityAction<Collision2D>(OnCollision));
		}
	}

	private void OnCollision(Collision2D collision)
	{
		if (collision == null)
		{
			if (m_relativeVelocity != null)
			{
				m_relativeVelocity.Value = Vector2.zero;
			}
			base.Fsm.Event(m_onLandOnGroundEvent);
			return;
		}
		bool flag = false;
		ContactPoint2D[] contacts = collision.contacts;
		foreach (ContactPoint2D contactPoint2D in contacts)
		{
			if (Vector2.Dot(contactPoint2D.normal, Vector2.up) > 0.8f)
			{
				flag = true;
				break;
			}
		}
		if (m_relativeVelocity != null)
		{
			m_relativeVelocity.Value = collision.relativeVelocity;
		}
		if (flag && collision.relativeVelocity.y > m_minFallSpeedForFallEvent)
		{
			base.Fsm.Event(m_onLandOnGroundEvent);
		}
		else if (Mathf.Abs(collision.relativeVelocity.x) > Mathf.Abs(collision.relativeVelocity.y))
		{
			base.Fsm.Event(m_onHorizontalCollisionEvent);
		}
	}
}
