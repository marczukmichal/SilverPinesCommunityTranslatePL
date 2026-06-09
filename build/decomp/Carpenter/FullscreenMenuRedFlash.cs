using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FullscreenMenuRedFlash : MonoBehaviour
{
	[SerializeField]
	private GameObject m_container;

	[SerializeField]
	private Image m_image;

	[SerializeField]
	private Sprite[] m_sprites;

	[SerializeField]
	private float m_waitTime;

	[SerializeField]
	private AudioEvent m_audioEvent;

	public void Flash()
	{
		StopAllCoroutines();
		StartCoroutine(FlashCoroutine());
	}

	public IEnumerator FlashCoroutine()
	{
		m_container.gameObject.SetActive(value: true);
		if (m_audioEvent != null)
		{
			m_audioEvent.Play2D();
		}
		Sprite[] sprites = m_sprites;
		foreach (Sprite sprite in sprites)
		{
			m_image.sprite = sprite;
			yield return new WaitForSecondsRealtime(m_waitTime);
		}
		m_container.gameObject.SetActive(value: false);
	}
}
