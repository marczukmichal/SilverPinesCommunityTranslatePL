using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MenuFade : MonoBehaviour
{
	[SerializeField]
	private Image m_image;

	[SerializeField]
	private float m_fadeTime;

	public IEnumerator FadeCoroutine()
	{
		m_image.enabled = true;
		Color color = m_image.color;
		color.a = 0f;
		m_image.color = color;
		yield return m_image.DOFade(1f, m_fadeTime).SetUpdate(isIndependentUpdate: true).WaitForCompletion();
	}
}
