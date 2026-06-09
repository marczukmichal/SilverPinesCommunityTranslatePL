using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CoinReturnVisuals : MonoBehaviour
{
	[SerializeField]
	private Image[] m_coins;

	[SerializeField]
	private float m_initialDelay = 0.5f;

	[SerializeField]
	private float m_delay = 0.5f;

	[SerializeField]
	private float m_fadeDuration = 0.5f;

	[SerializeField]
	private AudioEvent m_audioEvent;

	public IEnumerator AnimateCoins(int coinCount)
	{
		if (m_coins == null || m_coins.Length == 0)
		{
			yield return null;
		}
		int showCount = Mathf.Min(coinCount, m_coins.Length);
		for (int j = 0; j < m_coins.Length; j++)
		{
			if (!(m_coins[j] == null))
			{
				m_coins[j].gameObject.SetActive(value: false);
				Color color = m_coins[j].color;
				color.a = 1f;
				m_coins[j].color = color;
			}
		}
		List<int> indices = new List<int>();
		for (int k = 0; k < m_coins.Length; k++)
		{
			indices.Add(k);
		}
		for (int l = 0; l < indices.Count; l++)
		{
			int num = Random.Range(l, indices.Count);
			int index = l;
			List<int> list = indices;
			int index2 = num;
			int value = indices[num];
			int value2 = indices[l];
			indices[index] = value;
			list[index2] = value2;
		}
		for (int m = 0; m < showCount; m++)
		{
			Image image = m_coins[indices[m]];
			if (!(image == null))
			{
				image.gameObject.SetActive(value: true);
			}
		}
		if (coinCount > 0 && m_audioEvent != null)
		{
			m_audioEvent.Play2D();
		}
		yield return new WaitForSecondsRealtime(m_initialDelay);
		for (int i = 0; i < showCount; i++)
		{
			Image image2 = m_coins[indices[i]];
			if (!(image2 == null))
			{
				image2.gameObject.SetActive(value: true);
				yield return image2.DOFade(0f, m_fadeDuration).SetUpdate(isIndependentUpdate: true).SetDelay(m_delay)
					.WaitForCompletion();
			}
		}
	}
}
