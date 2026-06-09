using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SimpleUIImage : MonoBehaviour
{
	[Header("Animation Frames")]
	public Sprite[] m_frames;

	[Header("Animation Settings")]
	public float m_frameRate = 12f;

	public bool m_playOnStart = true;

	private int m_currentFrame;

	private Coroutine m_animationCoroutine;

	private Image m_targetImage;

	private void Awake()
	{
		m_targetImage = GetComponent<Image>();
	}

	private void OnEnable()
	{
		if (m_playOnStart)
		{
			Play();
		}
	}

	private void OnDisable()
	{
		Stop();
	}

	public void Play()
	{
		if (m_animationCoroutine != null)
		{
			StopCoroutine(m_animationCoroutine);
		}
		m_animationCoroutine = StartCoroutine(Animate());
	}

	public void Stop()
	{
		if (m_animationCoroutine != null)
		{
			StopCoroutine(m_animationCoroutine);
			m_animationCoroutine = null;
		}
	}

	private IEnumerator Animate()
	{
		if (m_targetImage == null || m_frames.Length == 0)
		{
			yield break;
		}
		float delay = 1f / m_frameRate;
		while (true)
		{
			m_targetImage.sprite = m_frames[m_currentFrame];
			m_currentFrame++;
			if (m_currentFrame >= m_frames.Length)
			{
				m_currentFrame = 0;
			}
			yield return new WaitForSeconds(delay);
		}
	}
}
