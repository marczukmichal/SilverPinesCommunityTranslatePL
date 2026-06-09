using DG.Tweening;
using UnityEngine;

public class MinigameActiveHighlightFocus : MonoBehaviour
{
	[SerializeField]
	private RectTransform m_focusRectTransform;

	[SerializeField]
	private float m_focusBoundSizeScale = 2f;

	[SerializeField]
	private float m_focusBoundsPadding = 10f;

	[SerializeField]
	private CanvasGroup m_canvasGroup;

	[SerializeField]
	private float m_fadeTime = 0.25f;

	private GameObject m_focusTarget;

	private RectTransform m_targetRectTransform;

	private void OnEnable()
	{
		m_canvasGroup.alpha = 0f;
		GlobalReferences.Instance.EventChannels.Minigames.MinigameHighlightInteract.Register(SetFocusTarget);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.Minigames.MinigameHighlightInteract.Unregister(SetFocusTarget);
	}

	private void SetFocusTarget(GameObject focustarget)
	{
		m_focusTarget = focustarget;
		DOTween.Kill(m_canvasGroup);
		if (m_focusTarget != null)
		{
			m_focusRectTransform.gameObject.SetActive(value: true);
			m_targetRectTransform = m_focusTarget.GetComponent<RectTransform>();
			m_canvasGroup.DOFade(1f, m_fadeTime).SetUpdate(isIndependentUpdate: true);
			UpdatePositionAndSize();
		}
		else
		{
			m_canvasGroup.DOFade(0f, m_fadeTime).SetUpdate(isIndependentUpdate: true);
		}
	}

	private void Update()
	{
		if (m_focusTarget != null)
		{
			UpdatePositionAndSize();
		}
	}

	private void UpdatePositionAndSize()
	{
		m_focusRectTransform.transform.position = m_focusTarget.transform.position;
		float num = Mathf.Max(m_targetRectTransform.rect.size.x, m_targetRectTransform.rect.size.y);
		num *= m_focusBoundSizeScale;
		num += m_focusBoundsPadding;
		m_focusRectTransform.sizeDelta = new Vector2(num, num);
	}
}
