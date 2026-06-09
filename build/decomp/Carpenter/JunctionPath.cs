using System.Collections.Generic;
using UnityEngine;

public class JunctionPath : MonoBehaviour
{
	[SerializeField]
	private Collider2D m_targetCollider;

	private List<LegacyCharacterMovement> m_trackedCharacters = new List<LegacyCharacterMovement>();

	public Collider2D TargetCollider => m_targetCollider;

	private void Start()
	{
		if (m_targetCollider == null)
		{
			Debug.LogWarning("Junction path " + base.gameObject.name + " is missing a target collider, will disable", this);
			base.gameObject.SetActive(value: false);
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		LegacyCharacterMovement componentInParent = collision.GetComponentInParent<LegacyCharacterMovement>();
		if ((object)componentInParent != null)
		{
			AddCharacter(componentInParent);
		}
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		LegacyCharacterMovement componentInParent = collision.GetComponentInParent<LegacyCharacterMovement>();
		if ((object)componentInParent != null)
		{
			RemoveCharacter(componentInParent);
		}
	}

	private void AddCharacter(LegacyCharacterMovement movement)
	{
		m_trackedCharacters.Add(movement);
		movement.RegisterJunctionPath(this);
	}

	private void RemoveCharacter(LegacyCharacterMovement movement)
	{
		m_trackedCharacters.Remove(movement);
		movement.DeregisterJunctionPath(this);
	}
}
