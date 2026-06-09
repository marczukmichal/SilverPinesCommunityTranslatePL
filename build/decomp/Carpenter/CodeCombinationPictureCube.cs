using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CodeCombinationPictureCube : BaseCodeCombinationValue
{
	[Serializable]
	private struct AnimationSide
	{
		public RectTransform m_rectTransform;

		public Image m_image;

		public float m_baseAnchorY;

		public float m_baseSizeY;

		public float m_moveAnchorY;

		public float m_moveSizeY;

		public Color m_baseColor;

		public Color m_moveColor;

		public void Animate(float duration, AnimationCurve curve)
		{
			m_rectTransform.DOAnchorPosY(m_moveAnchorY, duration).SetEase(curve);
			m_rectTransform.DOSizeDelta(new Vector2(m_rectTransform.sizeDelta.x, m_moveSizeY), duration).SetEase(curve);
			m_image.DOColor(m_moveColor, duration).SetEase(curve);
		}

		public void ResetState()
		{
			m_rectTransform.anchoredPosition = new Vector2(m_rectTransform.anchoredPosition.x, m_baseAnchorY);
			m_rectTransform.sizeDelta = new Vector2(m_rectTransform.sizeDelta.x, m_baseSizeY);
			m_image.color = m_baseColor;
		}
	}

	[Serializable]
	private struct SideOptions
	{
		public string m_id;

		public Sprite m_sprite;
	}

	[Header("Animation")]
	[SerializeField]
	private float m_animateDuration = 1f;

	[SerializeField]
	private AnimationSide m_frontFace;

	[SerializeField]
	private AnimationSide m_topFace;

	[SerializeField]
	private AnimationSide m_farFace;

	[SerializeField]
	private AnimationCurve m_facesCurve;

	[SerializeField]
	private AnimationCurve m_scaleCurve;

	[SerializeField]
	private float m_scale = 1.35f;

	[SerializeField]
	private SideOptions[] m_customOptions;

	public override string CurrentValue => m_customOptions[m_currentIndex].m_id;

	protected override IEnumerator AnimateInternal(bool increment)
	{
		if (!increment)
		{
			Debug.LogError("The Cube only rotates one way! This will look wonk!");
		}
		m_frontFace.Animate(m_animateDuration, m_facesCurve);
		m_topFace.Animate(m_animateDuration, m_facesCurve);
		m_farFace.Animate(m_animateDuration, m_facesCurve);
		base.transform.DOScaleY(m_scale, m_animateDuration).SetEase(m_scaleCurve);
		yield return new WaitForSeconds(m_animateDuration + 0.1f);
		UpdateDecoration();
	}

	private Sprite GetSpriteForValue(int value)
	{
		return m_customOptions[value].m_sprite;
	}

	protected override void UpdateDecoration()
	{
		m_frontFace.ResetState();
		m_topFace.ResetState();
		m_farFace.ResetState();
		m_frontFace.m_image.sprite = GetSpriteForValue(m_currentIndex);
		m_topFace.m_image.sprite = GetSpriteForValue(GetLoopedValue(m_currentIndex + 1));
		m_farFace.m_image.sprite = GetSpriteForValue(GetLoopedValue(m_currentIndex + 2));
		base.transform.localScale = Vector3.one;
	}
}
