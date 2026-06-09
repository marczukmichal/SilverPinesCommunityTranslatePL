using System.Collections.Generic;
using UnityEngine;

public class LineRopeCollider : MonoBehaviour
{
	private LineRope m_parentLineRope;

	private List<Collider2D> m_activeColliders2D;

	private void Start()
	{
		m_parentLineRope = GetComponentInParent<LineRope>();
		m_activeColliders2D = new List<Collider2D>();
	}

	private void OnTriggerEnter2D(Collider2D collider)
	{
		if (!m_activeColliders2D.Contains(collider))
		{
			m_activeColliders2D.Add(collider);
			if (m_parentLineRope != null)
			{
				m_parentLineRope.OnRopeCollisionEnter();
			}
		}
	}

	private void OnTriggerExit2D(Collider2D collider)
	{
		m_activeColliders2D.Remove(collider);
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (!m_activeColliders2D.Contains(collision.collider))
		{
			m_activeColliders2D.Add(collision.collider);
		}
	}

	private void OnCollisionExit2D(Collision2D collision)
	{
		m_activeColliders2D.Remove(collision.collider);
	}

	private void FixedUpdate()
	{
		if (m_activeColliders2D.Count <= 0)
		{
			return;
		}
		Vector2 zero = Vector2.zero;
		foreach (Collider2D item in m_activeColliders2D)
		{
			CharacterMovement componentInParent = item.GetComponentInParent<CharacterMovement>();
			if ((object)componentInParent != null)
			{
				zero += componentInParent.PreviousVelocity;
			}
			else
			{
				zero += item.attachedRigidbody.linearVelocity;
			}
		}
		m_parentLineRope.AddCollisionVelocityFromPosition(zero, base.transform.position);
	}
}
