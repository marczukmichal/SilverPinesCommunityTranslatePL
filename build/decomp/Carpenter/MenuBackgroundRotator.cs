using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MenuBackgroundRotator : MonoBehaviour
{
	[SerializeField]
	private Sprite[] m_sprites;

	[SerializeField]
	private Image[] m_imagePanels;

	[SerializeField]
	private float m_fadeTime = 4f;

	[SerializeField]
	private float m_scaleAmount = 1.25f;

	[SerializeField]
	private float m_scaleTime = 7f;

	[SerializeField]
	private float m_waitTime = 5f;

	private IEnumerator Start()
	{
		int panelIndex = 0;
		int spriteIndex = 0;
		int num = -1;
		while (true)
		{
			m_imagePanels[panelIndex].sprite = m_sprites[spriteIndex];
			spriteIndex++;
			if (spriteIndex >= m_sprites.Length)
			{
				spriteIndex = 0;
			}
			m_imagePanels[panelIndex].transform.SetAsLastSibling();
			Color white = Color.white;
			white.a = 0f;
			m_imagePanels[panelIndex].color = white;
			if (num != -1)
			{
				m_imagePanels[num].DOFade(0f, m_fadeTime).SetUpdate(isIndependentUpdate: true).WaitForCompletion();
			}
			m_imagePanels[panelIndex].transform.localScale = Vector3.one;
			m_imagePanels[panelIndex].transform.DOScale(m_scaleAmount, m_scaleTime);
			yield return m_imagePanels[panelIndex].DOFade(1f, m_fadeTime).SetUpdate(isIndependentUpdate: true).WaitForCompletion();
			yield return new WaitForSeconds(m_waitTime);
			num = panelIndex;
			panelIndex++;
			if (panelIndex >= m_imagePanels.Length)
			{
				panelIndex = 0;
			}
		}
	}
}
