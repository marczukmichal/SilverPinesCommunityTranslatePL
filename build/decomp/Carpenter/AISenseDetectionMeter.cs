using UnityEngine;
using UnityEngine.UI;

public class AISenseDetectionMeter : MonoBehaviour
{
	public enum Style
	{
		Bar,
		Eye
	}

	[SerializeField]
	private AISenses m_aiSenses;

	[SerializeField]
	private AISensesSet m_aiSensesSet;

	[SerializeField]
	private CharacterHealth m_characterHealth;

	[SerializeField]
	private GameObject m_parent;

	[SerializeField]
	private Style m_style;

	[Header("Eye Style")]
	[SerializeField]
	private Sprite m_undetecedSprite;

	[SerializeField]
	private Sprite m_suspiciousSprite;

	[SerializeField]
	private Sprite m_detectedSprite;

	[SerializeField]
	private Image m_eyeImage;

	[SerializeField]
	private float m_eyeMinScale = 0.2f;

	[Header("Bar Style")]
	[SerializeField]
	private Image m_barFill;

	[Header("Colors")]
	[SerializeField]
	private Color m_normalColor;

	[SerializeField]
	private Color m_detectedColor;

	[SerializeField]
	private Color m_suspiciousColor;

	[DebugCommand("detection_bars", "Show detection bars on enemies", "detection_bars <true/false>", typeof(bool), false)]
	private static bool s_show_detection_bars_on_enemies;

	private float GetDetectionAmount()
	{
		if (m_aiSenses != null)
		{
			return m_aiSenses.DetectionPercent;
		}
		if (m_aiSensesSet != null)
		{
			return m_aiSensesSet.GetHighetstDetectionPercent();
		}
		return 0f;
	}

	private AISenses.TargetState GetDetectionState()
	{
		if (m_aiSenses != null)
		{
			return m_aiSenses.CurrentTargetState;
		}
		if (m_aiSensesSet != null)
		{
			return m_aiSensesSet.GetHighestTargetState();
		}
		return AISenses.TargetState.None;
	}

	private void Update()
	{
		bool flag = false;
		if (m_characterHealth != null)
		{
			flag = m_characterHealth.IsDead;
		}
		AISenses.TargetState detectionState = GetDetectionState();
		float detectionAmount = GetDetectionAmount();
		if (m_style == Style.Bar)
		{
			if (detectionState == AISenses.TargetState.None || flag || (!s_show_detection_bars_on_enemies && m_aiSenses != null))
			{
				m_parent.SetActive(value: false);
				return;
			}
			switch (detectionState)
			{
			case AISenses.TargetState.Detected:
				m_parent.SetActive(value: true);
				if (m_barFill != null)
				{
					m_barFill.color = m_detectedColor;
					m_barFill.rectTransform.anchorMin = new Vector2(0f, 0f);
					m_barFill.rectTransform.anchorMax = new Vector2(1f, 1f);
				}
				break;
			case AISenses.TargetState.Suspicious:
				m_parent.SetActive(value: true);
				m_barFill.color = m_suspiciousColor;
				m_barFill.rectTransform.anchorMin = new Vector2(Mathf.Lerp(0.5f, 0f, detectionAmount), 0f);
				m_barFill.rectTransform.anchorMax = new Vector2(Mathf.Lerp(0.5f, 1f, detectionAmount), 1f);
				break;
			}
		}
		else
		{
			if (m_style != Style.Eye)
			{
				return;
			}
			if (detectionState == AISenses.TargetState.None || flag || (!s_show_detection_bars_on_enemies && m_aiSenses != null))
			{
				m_eyeImage.color = m_normalColor;
				m_eyeImage.sprite = m_undetecedSprite;
				m_eyeImage.transform.localScale = Vector3.one;
				return;
			}
			switch (detectionState)
			{
			case AISenses.TargetState.Detected:
				m_eyeImage.color = m_detectedColor;
				m_eyeImage.sprite = m_detectedSprite;
				m_eyeImage.transform.localScale = Vector3.one;
				break;
			case AISenses.TargetState.Suspicious:
				m_eyeImage.color = m_suspiciousColor;
				m_eyeImage.sprite = m_suspiciousSprite;
				m_eyeImage.transform.localScale = new Vector3(1f, Mathf.Clamp(detectionAmount, m_eyeMinScale, 1f), 1f);
				break;
			}
		}
	}
}
