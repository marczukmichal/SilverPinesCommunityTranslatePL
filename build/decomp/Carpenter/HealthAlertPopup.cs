using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthAlertPopup : MonoBehaviour
{
	[SerializeField]
	private FloatVariable m_healthPercentVariable;

	[SerializeField]
	private IntVariable m_healthValueVariable;

	[SerializeField]
	private IntVariable m_maxHealthValueVariable;

	[SerializeField]
	private HealthStatusSettings m_healthStatusSettings;

	[Header("UI")]
	[SerializeField]
	private CanvasGroup m_canvasGroup;

	[SerializeField]
	private TextMeshProUGUI m_currentStatusLabel;

	[SerializeField]
	private Image m_healthStatusIcon;

	[SerializeField]
	private GameObject m_bloodOverlay;

	[Header("Delta")]
	[SerializeField]
	private Image m_healthDeltaIcon;

	[SerializeField]
	private float m_largeHealthDeltaThreshold;

	[SerializeField]
	private Sprite m_healthSmallIncreaseSprite;

	[SerializeField]
	private Sprite m_healthLargeIncreaseSprite;

	[SerializeField]
	private float m_startFadeDeltaDelay;

	[SerializeField]
	private float m_fadeDeltaRate;

	[Header("Detailed Version")]
	[SerializeField]
	private Slider m_healthSlider;

	[SerializeField]
	private Image m_sliderFillImage;

	[Header("Animation")]
	[SerializeField]
	private float m_showTime;

	[SerializeField]
	private float m_fadeAnimationTime;

	private HealthStatusSettings.HealthStatusGroup m_currentStatus;

	private float m_currentHealth;

	private float m_timer;

	private bool m_isShowing;

	private float m_startTime;

	private float m_stopShowingHealthIncreaseTime;

	private StatusEffectReceiver m_statusEffectReceiver;

	private void Start()
	{
		m_startTime = Time.unscaledTime;
	}

	private void OnEnable()
	{
		m_canvasGroup.alpha = 0f;
		m_currentHealth = m_healthPercentVariable.Value;
		m_healthPercentVariable.RegisterListener(OnHealthChanged);
		UpdateHealthAndStatus();
		GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Register(OnPlayerChanged);
		GlobalReferences.Instance.EventChannels.Datastore.PersistentDataRefresh.Register(OnStartNewGame);
		GlobalReferences.Instance.EventChannels.Inventory.ShowItemWheel.Register(SetUsingQuickItems);
		GlobalReferences.Instance.EventChannels.Inventory.ShowQuickItemPanel.Register(SetUsingQuickItems);
		OnPlayerChanged(GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item);
	}

	private void OnDisable()
	{
		m_healthPercentVariable.UnregisterListener(OnHealthChanged);
		GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Unregister(OnPlayerChanged);
		GlobalReferences.Instance.EventChannels.Datastore.PersistentDataRefresh.Unregister(OnStartNewGame);
		GlobalReferences.Instance.EventChannels.Inventory.ShowItemWheel.Unregister(SetUsingQuickItems);
		GlobalReferences.Instance.EventChannels.Inventory.ShowQuickItemPanel.Unregister(SetUsingQuickItems);
	}

	private void OnPlayerChanged(GameObject player)
	{
		if (player != null)
		{
			m_statusEffectReceiver = player.GetComponent<StatusEffectReceiver>();
		}
		else
		{
			m_statusEffectReceiver = null;
		}
	}

	private void SetUsingQuickItems(bool showing)
	{
	}

	private void OnStartNewGame()
	{
		UpdateHealthAndStatus();
	}

	private bool CanShow()
	{
		return Time.unscaledTime - m_startTime > 1f;
	}

	private void OnHealthChanged(float newValue)
	{
		m_timer = m_showTime;
		bool num = GlobalReferences.Instance.MainInventory.HasActiveArtifactEffect(GlobalReferences.Instance.ArtifactEffects.Generic.ShowPlayerHealthBar);
		bool flag = m_healthStatusSettings.GetStatusFromHealthPercent(m_healthPercentVariable.Value) != m_currentStatus;
		bool flag2 = newValue > m_currentHealth;
		if ((num || flag || flag2) && CanShow())
		{
			SetShowing(showing: true);
		}
		UpdateHealthAndStatus();
	}

	private void SetShowing(bool showing)
	{
		if (m_isShowing != showing)
		{
			m_isShowing = showing;
			if (showing)
			{
				m_timer = m_showTime;
			}
			DOTween.Kill(m_canvasGroup);
			m_canvasGroup.DOFade(showing ? 1f : 0f, m_fadeAnimationTime);
		}
	}

	private bool HasStatusEffects()
	{
		if (m_statusEffectReceiver != null)
		{
			return m_statusEffectReceiver.IsAnyVisualStatusEffectActive();
		}
		return false;
	}

	public void Update()
	{
		bool flag = HasStatusEffects();
		if (m_isShowing)
		{
			m_bloodOverlay.gameObject.SetActive(m_currentStatus.m_alwaysShowBarPopup);
			if (!m_currentStatus.m_alwaysShowBarPopup && !flag)
			{
				m_timer -= Time.unscaledDeltaTime;
			}
			if (m_timer <= 0f)
			{
				SetShowing(showing: false);
			}
			UpdateHealthAndStatus();
		}
		else if (flag)
		{
			SetShowing(showing: true);
		}
	}

	private void UpdateHealthAndStatus()
	{
		HealthStatusSettings.HealthStatusGroup statusFromHealthPercent = m_healthStatusSettings.GetStatusFromHealthPercent(m_healthPercentVariable.Value);
		if (m_currentStatus != statusFromHealthPercent)
		{
			m_currentStatus = statusFromHealthPercent;
			m_currentStatusLabel.text = statusFromHealthPercent.m_statusLabel.GetLocalizedString();
			Image sliderFillImage = m_sliderFillImage;
			TextMeshProUGUI currentStatusLabel = m_currentStatusLabel;
			Color color2 = (m_healthStatusIcon.color = statusFromHealthPercent.m_color);
			Color color5 = (sliderFillImage.color = (currentStatusLabel.color = color2));
		}
		if (m_healthPercentVariable.Value != m_currentHealth)
		{
			bool flag = m_healthPercentVariable.Value > m_currentHealth;
			m_healthDeltaIcon.gameObject.SetActive(flag);
			if (flag)
			{
				if (m_healthPercentVariable.Value - m_currentHealth > m_largeHealthDeltaThreshold)
				{
					m_healthDeltaIcon.sprite = m_healthLargeIncreaseSprite;
				}
				else
				{
					m_healthDeltaIcon.sprite = m_healthSmallIncreaseSprite;
				}
			}
			m_currentHealth = m_healthPercentVariable.Value;
			m_stopShowingHealthIncreaseTime = Time.time + m_startFadeDeltaDelay;
			Color color6 = m_healthDeltaIcon.color;
			color6.a = 1f;
			m_healthDeltaIcon.color = color6;
		}
		else if (Time.time > m_stopShowingHealthIncreaseTime)
		{
			Color color7 = m_healthDeltaIcon.color;
			color7.a = Mathf.MoveTowards(color7.a, 0f, Time.deltaTime * m_fadeDeltaRate);
			m_healthDeltaIcon.color = color7;
		}
		bool flag2 = GlobalReferences.Instance.MainInventory.HasActiveArtifactEffect(GlobalReferences.Instance.ArtifactEffects.Generic.ShowPlayerHealthBar);
		m_healthSlider.gameObject.SetActive(flag2);
		if (flag2)
		{
			m_healthSlider.value = m_healthPercentVariable.Value;
			m_currentStatusLabel.text = statusFromHealthPercent.m_statusLabel.GetLocalizedString() + " " + m_healthValueVariable.Value + "\\" + m_maxHealthValueVariable.Value;
		}
	}
}
