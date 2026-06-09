using System.Collections.Generic;
using UnityEngine;

public class ParticlesCollisionAudioEvent : MonoBehaviour
{
	[SerializeField]
	private AudioEvent m_audioEvent;

	private ParticleSystem m_particleSystem;

	private List<ParticleCollisionEvent> m_collisionEvents = new List<ParticleCollisionEvent>();

	private void Start()
	{
		m_particleSystem = GetComponent<ParticleSystem>();
		ParticleSystem.CollisionModule collision = m_particleSystem.collision;
		collision.sendCollisionMessages = true;
	}

	private void OnParticleCollision(GameObject other)
	{
		if (!(m_particleSystem == null))
		{
			int collisionEvents = m_particleSystem.GetCollisionEvents(other, m_collisionEvents);
			for (int i = 0; i < collisionEvents; i++)
			{
				Vector3 intersection = m_collisionEvents[i].intersection;
				m_audioEvent.Play(intersection);
			}
		}
	}
}
