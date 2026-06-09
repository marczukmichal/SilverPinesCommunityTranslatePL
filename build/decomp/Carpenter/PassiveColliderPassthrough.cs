using System.Collections.Generic;
using UnityEngine;

public class PassiveColliderPassthrough : MonoBehaviour
{
	[SerializeField]
	private Rigidbody2D m_targetRigidbody;

	[SerializeField]
	private float m_forceScalar = 10f;

	private List<Collider2D> m_activeColliders2D = new List<Collider2D>();

	private void OnValidate()
	{
	}

	private void Start()
	{
		base.gameObject.layer = GameLayers.CollidesWithCharactersOnlyLayer;
	}

	private void OnTriggerEnter2D(Collider2D collider)
	{
		if ((bool)collider.GetComponentInParent<CharacterMovement>())
		{
			m_activeColliders2D.Add(collider);
		}
	}

	private void OnTriggerExit2D(Collider2D collider)
	{
		if ((bool)collider.GetComponentInParent<CharacterMovement>())
		{
			m_activeColliders2D.Remove(collider);
		}
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if ((bool)collision.collider.GetComponentInParent<CharacterMovement>())
		{
			m_activeColliders2D.Add(collision.collider);
		}
	}

	private void OnCollisionExit2D(Collision2D collision)
	{
		if ((bool)collision.collider.GetComponentInParent<CharacterMovement>())
		{
			m_activeColliders2D.Remove(collision.collider);
		}
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
				zero += componentInParent.PreviousVelocity * item.attachedRigidbody.mass;
			}
			else
			{
				zero += item.attachedRigidbody.linearVelocity * item.attachedRigidbody.mass;
			}
		}
		m_targetRigidbody.AddForceAtPosition(zero * m_forceScalar, base.transform.position, ForceMode2D.Force);
	}
}
