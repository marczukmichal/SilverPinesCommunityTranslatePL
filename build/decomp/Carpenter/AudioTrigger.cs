using System.Collections;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.Serialization;

public class AudioTrigger : MonoBehaviour
{
	[SerializeField]
	private AudioEvent m_audioEvent;

	[FormerlySerializedAs("m_playOnStart")]
	[SerializeField]
	private bool m_playOnEnable;

	[SerializeField]
	private bool m_stopOnDisable;

	private EventInstance m_instance;

	private void OnEnable()
	{
		if (m_playOnEnable)
		{
			StartCoroutine(DelayedTrigger());
		}
	}

	private void OnDisable()
	{
		if (m_stopOnDisable)
		{
			StopAudio();
		}
	}

	private IEnumerator DelayedTrigger()
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		TriggerAudio();
	}

	public void TriggerAudio()
	{
		if (!(m_audioEvent == null) && base.isActiveAndEnabled)
		{
			Vector3 position = base.transform.position;
			m_instance = RuntimeManager.CreateInstance(m_audioEvent.FMODEvent);
			m_instance.set3DAttributes(position.To3DAttributes());
			m_instance.start();
		}
	}

	public void StopAudio()
	{
		if (m_instance.isValid())
		{
			m_instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			m_instance.release();
		}
	}
}
