using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;

public class EngagementUI : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup m_canvasGroup;

	[SerializeField]
	private InlineButtonPromptsText m_text;

	[SerializeField]
	private InputActionReference[] m_engageInputActions;

	[SerializeField]
	private LocalizedString m_stringReference;

	private bool m_showing;

	public IEnumerator ShowAsync()
	{
		if (!m_showing)
		{
			m_showing = true;
			m_canvasGroup.alpha = 0f;
			m_canvasGroup.gameObject.SetActive(value: true);
			m_text.SetText(m_stringReference.GetLocalizedString(), m_engageInputActions);
			yield return m_canvasGroup.DOFade(1f, 1f).WaitForCompletion();
		}
	}

	public IEnumerator HideAsync()
	{
		if (m_showing)
		{
			m_showing = false;
			yield return m_canvasGroup.DOFade(0f, 0.2f).WaitForCompletion();
			m_canvasGroup.gameObject.SetActive(value: false);
		}
	}
}
