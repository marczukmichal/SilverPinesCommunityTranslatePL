using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SilverPinesLogoFlicker : MonoBehaviour
{
	[SerializeField]
	private Vector2 m_timesBetweenFlicker;

	[SerializeField]
	private float m_flickerFrameRate = 30f;

	[SerializeField]
	private Sprite[] m_flickerSprites;

	[SerializeField]
	private Image m_image;

	[SerializeField]
	private float m_maxMoveAround = 0.5f;

	[SerializeField]
	private Vector2 m_randomScale;

	private IEnumerator Start()
	{
		m_image.enabled = false;
		while (true)
		{
			yield return new WaitForSeconds(m_timesBetweenFlicker.GetRandom());
			Vector2 vector = new Vector2(Random.Range(0.5f - m_maxMoveAround * 0.5f, 0.5f + m_maxMoveAround * 0.5f), Random.Range(0.5f - m_maxMoveAround * 0.5f, 0.5f + m_maxMoveAround * 0.5f));
			Vector2 vector4 = (m_image.rectTransform.anchorMin = (m_image.rectTransform.anchorMax = vector));
			m_image.rectTransform.localScale = Vector3.one * m_randomScale.GetRandom();
			float delay = 1f / m_flickerFrameRate;
			Sprite[] flickerSprites = m_flickerSprites;
			foreach (Sprite sprite in flickerSprites)
			{
				m_image.enabled = true;
				m_image.sprite = sprite;
				yield return new WaitForSeconds(delay);
			}
			m_image.enabled = false;
		}
	}
}
