using System.Collections;
using UnityEngine;

public class AudioTriggerCollision : MonoBehaviour, IDynamicallySpawnedEventHandler
{
	[SerializeField]
	private AudioEvent m_audioEvent;

	[SerializeField]
	private float m_refireTime = 1f;

	private bool m_canPlay = true;

	public void OnCollisionEnter2D(Collision2D collision)
	{
		if (base.enabled && m_canPlay && m_audioEvent != null)
		{
			GlobalReferences.Instance.EventChannels.Audio.PlayAudio.Raise(new PlayAudioEventData(m_audioEvent, collision.GetContact(0).point, useOcclusion: true));
			StartCoroutine(TempDisable(m_refireTime));
		}
	}

	private IEnumerator TempDisable(float time)
	{
		m_canPlay = false;
		yield return new WaitForSeconds(time);
		m_canPlay = true;
	}

	public void OnInitialSpawn()
	{
		base.enabled = true;
	}

	public void OnReloadSpawn()
	{
		base.enabled = false;
	}
}
