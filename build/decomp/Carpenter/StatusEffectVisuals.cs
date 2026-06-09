using UnityEngine;

public class StatusEffectVisuals : MonoBehaviour
{
	[SerializeField]
	private ParticleSystem[] m_particleSystems;

	private bool m_shouldCleanup;

	private SpriteRenderer m_attachedSpriteRenderer;

	public void AttachToEffectReceiver(StatusEffectReceiver receiver)
	{
		if ((bool)receiver && (bool)receiver.VisualsSpriteRenderer)
		{
			m_attachedSpriteRenderer = receiver.VisualsSpriteRenderer;
			ParticleSystem[] particleSystems = m_particleSystems;
			foreach (ParticleSystem obj in particleSystems)
			{
				ParticleSystem.ShapeModule shape = obj.shape;
				shape.spriteRenderer = m_attachedSpriteRenderer;
				ParticleSystem.EmissionModule emission = obj.emission;
				emission.enabled = true;
			}
		}
		m_shouldCleanup = false;
	}

	public void DisableEffects()
	{
		ParticleSystem[] particleSystems = m_particleSystems;
		for (int i = 0; i < particleSystems.Length; i++)
		{
			ParticleSystem.EmissionModule emission = particleSystems[i].emission;
			emission.enabled = false;
		}
		m_shouldCleanup = true;
	}

	private void Update()
	{
		ParticleSystem[] particleSystems;
		if ((bool)m_attachedSpriteRenderer)
		{
			particleSystems = m_particleSystems;
			for (int i = 0; i < particleSystems.Length; i++)
			{
				ParticleSystem.ShapeModule shape = particleSystems[i].shape;
				Vector3 scale = shape.scale;
				scale.x = Mathf.Abs(scale.x);
				if (m_attachedSpriteRenderer.flipX)
				{
					scale.x *= -1f;
				}
				shape.scale = scale;
			}
		}
		if (!m_shouldCleanup)
		{
			return;
		}
		bool flag = false;
		particleSystems = m_particleSystems;
		for (int i = 0; i < particleSystems.Length; i++)
		{
			if (particleSystems[i].isPlaying)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			DynamicallySpawnedObject component = GetComponent<DynamicallySpawnedObject>();
			if (component != null)
			{
				component.ReleaseSafe();
			}
		}
	}
}
