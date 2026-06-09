using System;
using UnityEngine;
using UnityEngine.Events;

public class AimTarget : MonoBehaviour
{
	[SerializeField]
	private AimTargetSet m_aimTargetSet;

	[SerializeField]
	private Collider2D m_targetCollider;

	private CharacterHealth m_characterHealth;

	private bool m_isHidden;

	public Vector2 Position => base.transform.position;

	public bool IsHidden
	{
		get
		{
			return m_isHidden;
		}
		set
		{
			m_isHidden = value;
		}
	}

	private void Awake()
	{
		m_characterHealth = GetComponentInParent<CharacterHealth>();
		if (m_characterHealth != null)
		{
			m_characterHealth.OnDead.AddListener(OnDead);
			CharacterHealth characterHealth = m_characterHealth;
			characterHealth.OnRevive = (UnityAction)Delegate.Combine(characterHealth.OnRevive, new UnityAction(OnRevive));
		}
	}

	private void OnDead()
	{
		m_aimTargetSet.Remove(this);
	}

	private void OnRevive()
	{
		m_aimTargetSet.Add(this);
	}

	private void OnEnable()
	{
		if (m_characterHealth == null || !m_characterHealth.IsDead)
		{
			m_aimTargetSet.Add(this);
		}
	}

	private void OnDisable()
	{
		m_aimTargetSet.Remove(this);
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = ((m_aimTargetSet != null && m_aimTargetSet.Contains(this)) ? Color.red : Color.gray);
		Gizmos.DrawWireSphere(base.transform.position, 0.1f);
		Gizmos.color = Color.white;
	}

	public bool IsAimingAtTargetBounds(Vector2 aimRoot, Vector2 aimDirection)
	{
		if (m_targetCollider != null && m_targetCollider.isActiveAndEnabled)
		{
			Ray ray = new Ray(aimRoot, aimDirection);
			Bounds bounds = m_targetCollider.bounds;
			bounds.size = new Vector3(bounds.size.x, bounds.size.y, 1f);
			return bounds.IntersectRay(ray);
		}
		return false;
	}
}
