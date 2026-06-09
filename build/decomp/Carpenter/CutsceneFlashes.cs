using System;
using UnityEngine;
using UnityEngine.UI;

public class CutsceneFlashes : MonoBehaviour
{
	[Serializable]
	public class BlackFlashSettings
	{
		[Header("Flash Settings")]
		public int m_flashCount;

		public AnimationCurve m_flashTimeCurve;

		public AnimationCurve m_flashIntensityOverTimeCurve;

		public AnimationCurve m_flashLoopAlphaCurve;
	}

	[Serializable]
	public class ImageFlashSettings
	{
		[Header("Image Settings")]
		public Sprite m_sprite;
	}

	[Header("UI Elements")]
	[SerializeField]
	private CanvasGroup m_canvasGroup;

	[SerializeField]
	private Image m_flashImage;

	public void Clear()
	{
		base.gameObject.SetActive(value: false);
		m_canvasGroup.alpha = 0f;
		m_flashImage.gameObject.SetActive(value: false);
	}

	public void ShowImage(ImageFlashSettings settings)
	{
		base.gameObject.SetActive(value: true);
		m_canvasGroup.alpha = 1f;
		m_flashImage.color = Color.white;
		m_flashImage.sprite = settings.m_sprite;
		m_flashImage.gameObject.SetActive(value: true);
		m_flashImage.transform.localScale = Vector3.one;
	}

	public void ShowFlashes(BlackFlashSettings settings, float normalisedTime)
	{
		base.gameObject.SetActive(value: true);
		float num = settings.m_flashTimeCurve.Evaluate(normalisedTime);
		m_flashImage.gameObject.SetActive(value: false);
		float num2 = 0f;
		int flashCount = settings.m_flashCount;
		float num3 = 1f / (float)flashCount;
		for (int i = 0; i < flashCount; i++)
		{
			num2 += num3;
			if (num2 > num)
			{
				float time = (num2 - num) / num3;
				m_canvasGroup.alpha = settings.m_flashIntensityOverTimeCurve.Evaluate(num) * settings.m_flashLoopAlphaCurve.Evaluate(time);
				break;
			}
		}
	}
}
