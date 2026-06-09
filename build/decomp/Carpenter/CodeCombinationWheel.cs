using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CodeCombinationWheel : BaseCodeCombinationValue
{
	public enum Mode
	{
		Text,
		Sprite
	}

	[Serializable]
	private struct CustomCodeOption
	{
		public string m_label;
	}

	[SerializeField]
	private Mode m_mode;

	[Header("Text")]
	[SerializeField]
	private TextMeshProUGUI m_plus2Text;

	[SerializeField]
	private TextMeshProUGUI m_plus1Text;

	[SerializeField]
	private TextMeshProUGUI m_middleText;

	[SerializeField]
	private TextMeshProUGUI m_minus1Text;

	[SerializeField]
	private TextMeshProUGUI m_minus2Text;

	[Header("Images & Sprites")]
	[SerializeField]
	private Image m_plus2Image;

	[SerializeField]
	private Image m_plus1Image;

	[SerializeField]
	private Image m_middleImage;

	[SerializeField]
	private Image m_minus1Image;

	[SerializeField]
	private Image m_minus2Image;

	[SerializeField]
	private Sprite[] m_codeNumberSprites;

	[SerializeField]
	private bool m_useCustomOptions;

	[Header("Animation")]
	[SerializeField]
	private RectTransform m_animationContainer;

	[SerializeField]
	private float m_anchorOffset;

	[SerializeField]
	private float m_animTime;

	[SerializeField]
	private CustomCodeOption[] m_customOptions;

	public override string CurrentValue
	{
		get
		{
			if (!m_useCustomOptions)
			{
				return m_currentIndex.ToString();
			}
			return m_customOptions[m_currentIndex].m_label;
		}
	}

	protected override IEnumerator AnimateInternal(bool increment)
	{
		yield return m_animationContainer.DOAnchorPosY(increment ? m_anchorOffset : (0f - m_anchorOffset), m_animTime).SetEase(Ease.OutBounce).WaitForCompletion();
		UpdateDecoration();
		m_animationContainer.anchoredPosition = Vector2.zero;
	}

	private string GetTextForValue(int value)
	{
		if (m_useCustomOptions)
		{
			return m_customOptions[value].m_label;
		}
		return value.ToString();
	}

	private Sprite GetSpriteForValue(int value)
	{
		return m_codeNumberSprites[value];
	}

	protected override void UpdateDecoration()
	{
		if (m_mode == Mode.Sprite)
		{
			m_minus2Image.sprite = GetSpriteForValue(GetLoopedValue(m_currentIndex - 2));
			m_minus1Image.sprite = GetSpriteForValue(GetLoopedValue(m_currentIndex - 1));
			m_middleImage.sprite = GetSpriteForValue(m_currentIndex);
			m_plus1Image.sprite = GetSpriteForValue(GetLoopedValue(m_currentIndex + 1));
			m_plus2Image.sprite = GetSpriteForValue(GetLoopedValue(m_currentIndex + 2));
		}
		else
		{
			m_minus2Text.text = GetTextForValue(GetLoopedValue(m_currentIndex - 2));
			m_minus1Text.text = GetTextForValue(GetLoopedValue(m_currentIndex - 1));
			m_middleText.text = GetTextForValue(m_currentIndex);
			m_plus1Text.text = GetTextForValue(GetLoopedValue(m_currentIndex + 1));
			m_plus2Text.text = GetTextForValue(GetLoopedValue(m_currentIndex + 2));
		}
	}
}
