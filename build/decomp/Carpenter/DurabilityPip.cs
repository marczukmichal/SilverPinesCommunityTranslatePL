using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class DurabilityPip : MonoBehaviour
{
	public enum PipFilledState
	{
		Init,
		Empty,
		Filled,
		HalfFilled
	}

	[SerializeField]
	private Image m_fillImage;

	[SerializeField]
	private Image m_borderImage;

	[SerializeField]
	private Color m_normalColor;

	[SerializeField]
	private Color m_brokenColor;

	[SerializeField]
	private Color m_lastDangerColor;

	[SerializeField]
	private RectTransform m_rectTransform;

	[SerializeField]
	private Color m_flashColor;

	private PipFilledState m_state;

	private DurabilityInfoBar.DurabilityBarState m_barState;

	public PipFilledState State => m_state;

	public void SetBarState(DurabilityInfoBar.DurabilityBarState barState)
	{
		if (m_barState != barState)
		{
			m_barState = barState;
			UpdateDecoration();
		}
	}

	public void SetFilledState(PipFilledState filledState, bool animateStateChange)
	{
		if (filledState != m_state)
		{
			m_state = filledState;
			UpdateDecoration();
			if (animateStateChange)
			{
				AnimateStateChange();
			}
		}
	}

	private void UpdateDecoration()
	{
		bool active = false;
		Color color = m_normalColor;
		switch (m_barState)
		{
		case DurabilityInfoBar.DurabilityBarState.LastPip:
			color = m_lastDangerColor;
			break;
		case DurabilityInfoBar.DurabilityBarState.Broken:
			color = m_brokenColor;
			break;
		}
		switch (m_state)
		{
		case PipFilledState.Filled:
			m_fillImage.fillAmount = 1f;
			active = true;
			break;
		case PipFilledState.HalfFilled:
			m_fillImage.fillAmount = 0.5f;
			active = true;
			break;
		case PipFilledState.Empty:
			color *= 0.6f;
			break;
		}
		m_fillImage.gameObject.SetActive(active);
		Color color4 = (m_borderImage.color = (m_fillImage.color = color));
	}

	public void AnimateStateChange()
	{
		m_rectTransform.localScale = Vector3.one * 0.5f;
		m_rectTransform.DOScale(Vector3.one, 1f).SetEase(Ease.OutBounce).WaitForCompletion();
	}

	public void AnimateSmallImpact()
	{
		StopAllCoroutines();
		StartCoroutine(AnimateStateImpactCoroutine());
	}

	private IEnumerator AnimateStateImpactCoroutine()
	{
		Color color2 = (m_borderImage.color = (m_fillImage.color = m_flashColor));
		yield return m_rectTransform.DOPunchAnchorPos(new Vector2(0f, -4f), 0.25f).WaitForCompletion();
		UpdateDecoration();
	}
}
