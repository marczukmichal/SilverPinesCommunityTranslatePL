using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class EffectExtras : MonoBehaviour
{
	[SerializeField]
	private AudioEvent m_audioEvent;

	[SerializeField]
	private PlayAudioEventGameEventChannel m_audioEventChannel;

	private IEnumerator Start()
	{
		if (m_audioEvent != null && m_audioEventChannel != null)
		{
			m_audioEventChannel.Raise(new PlayAudioEventData
			{
				m_audioEvent = m_audioEvent,
				m_position = base.transform.position
			});
		}
		VisualEffect spawnedEffect = GetComponent<VisualEffect>();
		spawnedEffect.Play();
		yield return new WaitForSeconds(0.1f);
		yield return new WaitUntil(() => spawnedEffect.aliveParticleCount == 0);
		Object.Destroy(base.gameObject);
	}
}
