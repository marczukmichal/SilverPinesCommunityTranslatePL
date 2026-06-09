using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class HealthStatusPanel : MonoBehaviour
{
	[SerializeField]
	private Image m_statusImage;

	[SerializeField]
	private FloatVariable m_healthPercentVariable;

	[SerializeField]
	private IntVariable m_healthValueVariable;

	[SerializeField]
	private IntVariable m_maxHealthValueVariable;

	[SerializeField]
	private HealthStatusSettings m_healthStatusSettings;

	[Header("New Health Status Thing")]
	[SerializeField]
	private TextMeshProUGUI m_healthStatusLabel;

	[SerializeField]
	private Image m_healthStatusColorIcon;

	private HealthStatusSettings.HealthStatusGroup m_currentStatus;

	private bool m_showingDetailedHealth;

	private void OnEnable()
	{
		m_currentStatus = null;
		UpdateHealthAndStuatus();
		LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
	}

	private void OnDisable()
	{
		LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
	}

	private void OnLocaleChanged(UnityEngine.Localization.Locale newLocale)
	{
		m_currentStatus = null;
		UpdateHealthAndStuatus();
	}

	public void Update()
	{
		UpdateHealthAndStuatus();
	}

	private void UpdateHealthAndStuatus()
	{
		HealthStatusSettings.HealthStatusGroup statusFromHealthPercent = m_healthStatusSettings.GetStatusFromHealthPercent(m_healthPercentVariable.Value);
		SetStatus(statusFromHealthPercent);
	}

	private void SetStatus(HealthStatusSettings.HealthStatusGroup statusGroup)
	{
		bool flag = GlobalReferences.Instance.MainInventory.HasActiveArtifactEffect(GlobalReferences.Instance.ArtifactEffects.Generic.ShowPlayerHealthBar);
		if (m_currentStatus != statusGroup || m_showingDetailedHealth != flag)
		{
			m_currentStatus = statusGroup;
			m_statusImage.sprite = statusGroup.m_image;
			if (flag)
			{
				m_healthStatusLabel.text = statusGroup.m_statusLabel.GetLocalizedString() + " " + m_healthValueVariable.Value + "\\" + m_maxHealthValueVariable.Value;
			}
			else
			{
				m_healthStatusLabel.text = statusGroup.m_statusLabel.GetLocalizedString();
			}
			m_healthStatusColorIcon.color = statusGroup.m_color;
			Color color = statusGroup.m_color;
			color.a = statusGroup.m_color.a;
			m_healthStatusLabel.color = color;
			m_showingDetailedHealth = flag;
		}
	}
}
