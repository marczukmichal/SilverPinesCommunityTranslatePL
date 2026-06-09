using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class StatusEffectAlertBarPopup : MonoBehaviour
{
	[SerializeField]
	private LocalizeStringEvent m_stsringLocalizer;

	[SerializeField]
	private TextMeshProUGUI m_text;

	[SerializeField]
	private Image m_icon;

	[SerializeField]
	private Slider m_slider;

	[SerializeField]
	private Image m_fillBar;

	[SerializeField]
	private Color m_inactiveTextColor;

	[SerializeField]
	private GameObject m_fillContainer;

	private StatusEffectDefinition m_statusEffectDefinition;

	public StatusEffectDefinition StatusEffectDefinition => m_statusEffectDefinition;

	public void SetShown(bool shown)
	{
		base.gameObject.SetActive(shown);
		if (!shown)
		{
			base.gameObject.transform.SetAsLastSibling();
		}
	}

	public void SetStatusEffectDefinition(StatusEffectDefinition statusEffectDefinition)
	{
		m_statusEffectDefinition = statusEffectDefinition;
		if (m_icon != null)
		{
			m_icon.sprite = statusEffectDefinition.StatusEffectIcon;
		}
		m_stsringLocalizer.StringReference = statusEffectDefinition.ActiveName;
		m_fillBar.color = statusEffectDefinition.InactiveFillColor;
	}

	public void SetValues(float percentage, bool effectActive)
	{
		m_slider.value = Mathf.Clamp01(percentage);
		m_text.color = (effectActive ? m_statusEffectDefinition.ActiveFillColor : m_inactiveTextColor);
		m_fillBar.color = (effectActive ? m_statusEffectDefinition.ActiveFillColor : m_statusEffectDefinition.InactiveFillColor);
		bool flag = m_statusEffectDefinition.HideBarWhileActive && effectActive;
		m_fillContainer.gameObject.SetActive(!flag);
		m_stsringLocalizer.StringReference = (effectActive ? m_statusEffectDefinition.ActiveName : m_statusEffectDefinition.InactiveName);
		if (m_icon != null)
		{
			m_icon.color = (effectActive ? Color.white : Color.gray);
		}
	}
}
