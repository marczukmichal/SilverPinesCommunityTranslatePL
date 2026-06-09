using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("Combat")]
public class DeathRagdoll : FsmStateAction
{
	public Rigidbody2D m_mainRigidbody;

	public float m_minForce;

	public float m_maxForce;

	public float m_minRotation;

	public float m_maxRotation;

	private CharacterHealth m_health;

	private CharacterHitReact m_hitReact;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_health = base.Owner.GetComponent<CharacterHealth>();
			m_hitReact = base.Owner.GetComponent<CharacterHitReact>();
		}
	}

	public override void OnEnter()
	{
		if (!(m_mainRigidbody != null))
		{
			return;
		}
		Vector2 linearVelocity = m_mainRigidbody.linearVelocity;
		if (m_hitReact != null)
		{
			DamageInstance lastHit = m_hitReact.LastHit;
			if (lastHit != null)
			{
				linearVelocity += lastHit.Direction * Random.Range(m_minForce, m_maxForce);
			}
		}
		m_mainRigidbody.freezeRotation = false;
		m_mainRigidbody.angularVelocity = Random.Range(m_minRotation, m_maxRotation);
		m_mainRigidbody.linearVelocity = linearVelocity;
	}
}
