using UnityEngine;

[ExecuteAlways]
public class AudioAutoDestroy : MonoBehaviour
{
	private AudioSource m_audioSource;

	private void Start()
	{
		m_audioSource = GetComponent<AudioSource>();
	}

	private void Update()
	{
		if (!m_audioSource.isPlaying)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
