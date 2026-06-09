using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class VisualEffectTrigger : MonoBehaviour
{
	[SerializeField]
	private bool m_playOnEnable;

	[SerializeField]
	private ParticleSystem[] m_particles;

	[SerializeField]
	private VisualEffect[] m_effects;

	private void Reset()
	{
		ParticleSystem[] componentsInChildren = GetComponentsInChildren<ParticleSystem>();
		List<ParticleSystem> list = new List<ParticleSystem>(componentsInChildren);
		ParticleSystem[] array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			ParticleSystem.SubEmittersModule subEmitters = array[i].subEmitters;
			for (int j = 0; j < subEmitters.subEmittersCount; j++)
			{
				ParticleSystem subEmitterSystem = subEmitters.GetSubEmitterSystem(j);
				list.Remove(subEmitterSystem);
			}
		}
		m_particles = list.ToArray();
		m_effects = GetComponentsInChildren<VisualEffect>();
	}

	private void OnEnable()
	{
		if (m_playOnEnable)
		{
			PlayEffect();
		}
	}

	public void PlayEffect()
	{
		ParticleSystem[] particles = m_particles;
		for (int i = 0; i < particles.Length; i++)
		{
			particles[i].Play();
		}
		VisualEffect[] effects = m_effects;
		for (int i = 0; i < effects.Length; i++)
		{
			effects[i].Play();
		}
	}
}
