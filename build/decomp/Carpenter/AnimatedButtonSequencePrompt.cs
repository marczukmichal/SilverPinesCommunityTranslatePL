using DG.Tweening;
using UnityEngine;

public class AnimatedButtonSequencePrompt : MonoBehaviour
{
	[SerializeField]
	private RectTransform m_rectTransform;

	[SerializeField]
	private GameObject m_container;

	private AnimatedButtonSequence m_activeSequence;

	private void OnEnable()
	{
		GlobalReferences.Instance.EventChannels.AnimatedButtonSequence.ActiveAnimatedButtonSequenceChanged.Register(OnABSChanged);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.AnimatedButtonSequence.ActiveAnimatedButtonSequenceChanged.Unregister(OnABSChanged);
	}

	private void OnABSChanged(AnimatedButtonSequence newSequence)
	{
		if (m_activeSequence != newSequence)
		{
			m_activeSequence = newSequence;
			if (m_activeSequence != null && !GlobalReferences.Instance.UserPreferences.SkipButtonMashEvents)
			{
				Animate();
			}
			else
			{
				Hide();
			}
		}
	}

	private void Start()
	{
		Hide();
	}

	private void Animate()
	{
		m_container.gameObject.SetActive(value: true);
		m_rectTransform.localScale = Vector3.one;
		m_rectTransform.anchoredPosition = Vector2.zero;
		m_rectTransform.DOScale(0.6f, 0.2f).SetLoops(-1, LoopType.Restart);
		m_rectTransform.DOAnchorPosY(-4f, 0.2f).SetLoops(-1, LoopType.Restart);
	}

	private void Hide()
	{
		DOTween.Kill(m_rectTransform);
		m_container.gameObject.SetActive(value: false);
	}
}
