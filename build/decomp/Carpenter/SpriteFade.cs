using DG.Tweening;
using UnityEngine;

[ExecuteInEditMode]
public class SpriteFade : MonoBehaviour
{
	[SerializeField]
	private float m_fadeSpeed = 0.25f;

	[SerializeField]
	private SpriteRenderer m_spriteRenderer;

	[SerializeField]
	private float m_targetAlpha;

	[SerializeField]
	private float m_fadeInAlpha = 1f;

	private void Reset()
	{
		m_spriteRenderer = GetComponent<SpriteRenderer>();
	}

	public void FadeIn()
	{
		m_spriteRenderer.DOKill();
		m_spriteRenderer.DOFade(m_fadeInAlpha, m_fadeSpeed);
	}

	public void FadeOut()
	{
		m_spriteRenderer.DOKill();
		m_spriteRenderer.DOFade(m_targetAlpha, m_fadeSpeed);
	}

	private void OnEnable()
	{
		if (!Application.isPlaying)
		{
			m_spriteRenderer.enabled = false;
		}
		else
		{
			m_spriteRenderer.enabled = true;
		}
	}
}
