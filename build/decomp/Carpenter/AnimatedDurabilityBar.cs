using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class AnimatedDurabilityBar : MonoBehaviour
{
	[SerializeField]
	private RectTransform m_fill;

	[SerializeField]
	private RectTransform m_changeFill;

	[SerializeField]
	private RectTransform m_punchTransform;

	[SerializeField]
	private Image m_barFillImage;

	[SerializeField]
	private Image m_durabilityIconImage;

	[SerializeField]
	private Vector2 m_punchTarget = new Vector2(-1f, 1f);

	[SerializeField]
	private Color m_normalColor;

	[SerializeField]
	private Color m_lowDurabilityColor;

	[SerializeField]
	private Color m_brokenDurabilityColor;

	[SerializeField]
	private float m_animateSpeed = 1f;

	private bool m_isAnimating;

	private float m_targetProportion;

	private float m_delayTimer;

	private MeleeWeaponItemInstance m_trackedMeleeWeapon;

	private void Update()
	{
		if (m_isAnimating)
		{
			return;
		}
		if (m_delayTimer > 0f)
		{
			m_delayTimer -= Time.deltaTime;
			return;
		}
		float x = m_changeFill.anchorMax.x;
		float x2 = Mathf.MoveTowards(m_changeFill.anchorMax.x, m_targetProportion, Time.deltaTime * m_animateSpeed);
		m_changeFill.anchorMax = new Vector2(x2, 1f);
		if (x == m_changeFill.anchorMax.x)
		{
			m_isAnimating = false;
		}
	}

	public void SetDurabilityProportion(MeleeWeaponItemInstance meleeWeapon, float startingProportion, float endingProportion)
	{
		m_fill.anchorMax = new Vector2(endingProportion, 1f);
		m_changeFill.anchorMin = new Vector2(endingProportion, 0f);
		if (!m_isAnimating || m_trackedMeleeWeapon != meleeWeapon)
		{
			m_changeFill.anchorMax = new Vector2(startingProportion, 1f);
		}
		m_trackedMeleeWeapon = meleeWeapon;
		m_targetProportion = endingProportion;
		m_delayTimer = 0.5f;
		DOTween.Kill(m_changeFill);
		m_changeFill.DOAnchorMax(new Vector2(endingProportion, 1f), 0.5f).SetDelay(0.5f).SetEase(Ease.Linear);
		DOTween.Kill(m_punchTransform);
		m_punchTransform.anchoredPosition = Vector2.zero;
		m_punchTransform.DOPunchAnchorPos(m_punchTarget, 0.5f);
		Color color = m_normalColor;
		if (endingProportion <= 0f)
		{
			color = m_brokenDurabilityColor;
		}
		else if (endingProportion <= 0.25f)
		{
			color = m_lowDurabilityColor;
		}
		Color color4 = (m_barFillImage.color = (m_durabilityIconImage.color = color));
		m_isAnimating = true;
	}
}
