using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
	[SerializeField]
	protected Rigidbody2D m_rigidbody;

	protected List<LegacyCharacterMovement> m_activeCharacters;

	public Rigidbody2D Rigidbody => m_rigidbody;

	public Vector2 Velocity => m_rigidbody.linearVelocity;

	protected virtual void Awake()
	{
		m_activeCharacters = new List<LegacyCharacterMovement>();
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.gameObject != null)
		{
			LegacyCharacterMovement componentInParent = collision.gameObject.GetComponentInParent<LegacyCharacterMovement>();
			if (componentInParent != null)
			{
				componentInParent.AttachToMovingPlatform(this);
				m_activeCharacters.Add(componentInParent);
			}
		}
	}

	private void OnCollisionExit2D(Collision2D collision)
	{
		if (collision.gameObject != null)
		{
			LegacyCharacterMovement componentInParent = collision.gameObject.GetComponentInParent<LegacyCharacterMovement>();
			if (componentInParent != null)
			{
				componentInParent.DetachFromMovingPlatform(this);
				m_activeCharacters.Remove(componentInParent);
			}
		}
	}
}
