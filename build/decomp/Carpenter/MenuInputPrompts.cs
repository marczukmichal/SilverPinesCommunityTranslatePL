using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;

public class MenuInputPrompts : MonoBehaviour
{
	[Serializable]
	public struct AvaialbleInput
	{
		[SerializeField]
		private LocalizedString m_labelStringReference;

		public InputActionReference m_inputAction;

		public string Label
		{
			get
			{
				if (m_labelStringReference == null || m_labelStringReference.IsEmpty)
				{
					return "error";
				}
				return m_labelStringReference.GetLocalizedString();
			}
		}
	}

	[SerializeField]
	private InputPrompt[] m_inputPrompts;

	[SerializeField]
	private CanvasGroup m_canvasGroup;

	[SerializeField]
	private float m_fadeInTime;

	public void Show(bool skipAnimation)
	{
		m_canvasGroup.DOKill();
		if (skipAnimation)
		{
			m_canvasGroup.gameObject.SetActive(value: true);
			m_canvasGroup.alpha = 1f;
		}
		else
		{
			m_canvasGroup.gameObject.SetActive(value: true);
			m_canvasGroup.DOFade(1f, m_fadeInTime);
		}
	}

	public void Hide()
	{
		m_canvasGroup.DOKill();
		m_canvasGroup.DOFade(0f, m_fadeInTime).OnComplete(delegate
		{
			m_canvasGroup.gameObject.SetActive(value: false);
		});
	}

	public void SetInputs(AvaialbleInput[] availableInputs)
	{
		if (availableInputs == null)
		{
			for (int i = 0; i < m_inputPrompts.Length; i++)
			{
				m_inputPrompts[i].gameObject.SetActive(value: false);
			}
			return;
		}
		for (int j = 0; j < m_inputPrompts.Length; j++)
		{
			if (j < availableInputs.Length)
			{
				m_inputPrompts[j].gameObject.SetActive(value: true);
				m_inputPrompts[j].SetLabelText(availableInputs[j].Label);
				m_inputPrompts[j].SetInputAction(availableInputs[j].m_inputAction);
			}
			else
			{
				m_inputPrompts[j].gameObject.SetActive(value: false);
			}
		}
	}
}
