using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class StaminaPanel : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup m_canvasGroup;

	[SerializeField]
	private RectTransform m_sizeTransform;

	[SerializeField]
	private float m_sizePerStamina = 3f;

	[SerializeField]
	private RectTransform m_containerTransform;

	[SerializeField]
	private RectTransform m_mainBarTransform;

	[SerializeField]
	private RectTransform m_changeFillBarTransformLeft;

	[SerializeField]
	private RectTransform m_changeFillBarTransformRight;

	[SerializeField]
	private RectTransform m_reserveFillTransform;

	[SerializeField]
	private float m_fadeTime;

	[SerializeField]
	private float m_showAlpha = 0.5f;

	[SerializeField]
	private Image m_background;

	[SerializeField]
	private Color m_backgroundColorNormal;

	[SerializeField]
	private Color m_backgroundColorExhausted;

	[SerializeField]
	private Color m_backgroundColorExhaustedFlash;

	[SerializeField]
	private Image m_mainBar;

	[SerializeField]
	private Color m_barColorNormal;

	[SerializeField]
	private Color m_barColorExhausted;

	[SerializeField]
	private Color m_barColorExhaustedFlash;

	[SerializeField]
	private float m_percentageToShow = 0.99f;

	[SerializeField]
	private float m_percentageToHide = 0.99f;

	[SerializeField]
	private float m_delayedConsumptionAnimationSpeed = 1f;

	[SerializeField]
	private VoidGameEventChannel m_insuffiencentStaminaEventChannel;

	[Header("Charge")]
	[SerializeField]
	private RectTransform[] m_chargeBars;

	[SerializeField]
	private GameObject m_chargeFlash;

	private float m_delayedStaminaAmount;

	private float m_currentStaminaAmount;

	private float m_currentReservePercent;

	private float m_targetReservePercent;

	private float m_exhaustPoint;

	private bool m_isShowing;

	private bool m_flashing;

	private float m_maxStamina;

	private Coroutine m_flashChargeCoroutine;

	private void OnEnable()
	{
		m_insuffiencentStaminaEventChannel.Register(InsufficientStamina);
		GlobalReferences.Instance.EventChannels.Stamina.StaminaInfo.Register(StaminaUpdated);
		GlobalReferences.Instance.EventChannels.Stamina.MeleeChargeState.Register(MeleeChargeStateUpdated);
		GlobalReferences.Instance.EventChannels.Stamina.MeleeChargeStateUpgrade.Register(MeleeChargeUpgrade);
	}

	private void OnDisable()
	{
		m_insuffiencentStaminaEventChannel.Unregister(InsufficientStamina);
		GlobalReferences.Instance.EventChannels.Stamina.StaminaInfo.Unregister(StaminaUpdated);
		GlobalReferences.Instance.EventChannels.Stamina.MeleeChargeState.Unregister(MeleeChargeStateUpdated);
		GlobalReferences.Instance.EventChannels.Stamina.MeleeChargeStateUpgrade.Unregister(MeleeChargeUpgrade);
	}

	private void StaminaUpdated(StaminaInfoData eventData)
	{
		m_maxStamina = eventData.m_totalMaxStamina;
		m_sizeTransform.sizeDelta = new Vector2(m_sizePerStamina * eventData.m_totalMaxStamina, 12f);
		bool flag = eventData.m_forceShow || ((!m_isShowing) ? (eventData.StaminaPercentage < m_percentageToShow) : (eventData.StaminaPercentage < m_percentageToHide));
		if (flag != m_isShowing)
		{
			m_isShowing = flag;
			m_canvasGroup.DOKill();
			m_canvasGroup.DOFade(flag ? m_showAlpha : 0f, m_fadeTime);
		}
		m_mainBarTransform.anchorMin = new Vector2(Mathf.Lerp(0.5f, 0f, eventData.StaminaPercentage), 0f);
		m_mainBarTransform.anchorMax = new Vector2(Mathf.Lerp(0.5f, 1f, eventData.StaminaPercentage), 1f);
		m_targetReservePercent = eventData.m_staminaReserve / eventData.m_totalMaxStamina;
		if (m_targetReservePercent < m_currentReservePercent)
		{
			m_currentReservePercent = m_targetReservePercent;
		}
		m_currentStaminaAmount = eventData.StaminaPercentage;
		if (!m_flashing)
		{
			m_background.color = (eventData.m_isExhausted ? m_backgroundColorExhausted : m_backgroundColorNormal);
			m_mainBar.color = (eventData.m_isExhausted ? m_barColorExhausted : m_barColorNormal);
		}
	}

	private void Start()
	{
		m_currentReservePercent = 0f;
		m_isShowing = false;
		m_canvasGroup.alpha = 0f;
		m_chargeFlash.gameObject.SetActive(value: false);
		for (int i = 0; i < m_chargeBars.Length; i++)
		{
			m_chargeBars[i].gameObject.SetActive(value: false);
		}
	}

	private void InsufficientStamina()
	{
		StopAllCoroutines();
		StartCoroutine(InsufficientStaminaCoroutine());
	}

	private IEnumerator InsufficientStaminaCoroutine()
	{
		m_background.color = m_backgroundColorExhaustedFlash;
		m_mainBar.color = m_barColorExhaustedFlash;
		m_flashing = true;
		yield return new WaitForSeconds(0.05f);
		m_flashing = false;
		m_containerTransform.anchoredPosition = Vector2.zero;
		m_containerTransform.DOKill();
		yield return m_containerTransform.DOShakeAnchorPos(0.75f, 5f).WaitForCompletion();
		m_containerTransform.anchoredPosition = Vector2.zero;
	}

	private void LateUpdate()
	{
		if (m_delayedStaminaAmount > m_currentStaminaAmount)
		{
			m_delayedStaminaAmount = Mathf.MoveTowards(m_delayedStaminaAmount, m_currentStaminaAmount, Time.deltaTime * m_delayedConsumptionAnimationSpeed);
		}
		else
		{
			m_delayedStaminaAmount = m_currentStaminaAmount;
		}
		m_changeFillBarTransformLeft.anchorMin = new Vector2(Mathf.Lerp(0.5f, 0f, m_delayedStaminaAmount), 0f);
		m_changeFillBarTransformLeft.anchorMax = new Vector2(Mathf.Lerp(0.5f, 0f, m_currentStaminaAmount), 1f);
		m_changeFillBarTransformRight.anchorMin = new Vector2(Mathf.Lerp(0.5f, 1f, m_currentStaminaAmount), 0f);
		m_changeFillBarTransformRight.anchorMax = new Vector2(Mathf.Lerp(0.5f, 1f, m_delayedStaminaAmount), 1f);
		m_currentReservePercent = Mathf.MoveTowards(m_currentReservePercent, m_targetReservePercent, Time.deltaTime);
		m_reserveFillTransform.gameObject.SetActive(m_currentReservePercent > 0f);
		if (m_currentReservePercent > 0f)
		{
			m_reserveFillTransform.anchorMin = new Vector2(Mathf.Lerp(0.5f, 0f, m_currentReservePercent / m_currentStaminaAmount), 0f);
			m_reserveFillTransform.anchorMax = new Vector2(Mathf.Lerp(0.5f, 1f, m_currentReservePercent / m_currentStaminaAmount), 1f);
		}
	}

	private void MeleeChargeStateUpdated(MeleeChargeState meleeChargeState)
	{
		if (meleeChargeState == null)
		{
			RectTransform[] chargeBars = m_chargeBars;
			for (int i = 0; i < chargeBars.Length; i++)
			{
				chargeBars[i].gameObject.SetActive(value: false);
			}
			m_chargeFlash.gameObject.SetActive(value: false);
			if (m_flashChargeCoroutine != null)
			{
				StopCoroutine(m_flashChargeCoroutine);
				m_flashChargeCoroutine = null;
			}
			return;
		}
		for (int j = 0; j < m_chargeBars.Length; j++)
		{
			bool flag = meleeChargeState.m_staminaRemaining[j] > 0f;
			if (flag)
			{
				float currentStaminaAmount = m_currentStaminaAmount;
				currentStaminaAmount -= meleeChargeState.m_staminaRemaining[j] / m_maxStamina;
				if (currentStaminaAmount > 0f)
				{
					m_chargeBars[j].anchorMin = new Vector2(Mathf.Lerp(0.5f, 0f, currentStaminaAmount), 0f);
					m_chargeBars[j].anchorMax = new Vector2(Mathf.Lerp(0.5f, 1f, currentStaminaAmount), 1f);
				}
				else
				{
					flag = false;
				}
			}
			m_chargeBars[j].gameObject.SetActive(flag);
		}
	}

	private void MeleeChargeUpgrade()
	{
		m_flashChargeCoroutine = StartCoroutine(ChargeUpgradeEffectCorutine());
	}

	private IEnumerator ChargeUpgradeEffectCorutine()
	{
		m_chargeFlash.gameObject.SetActive(value: true);
		yield return new WaitForSeconds(0.05f);
		m_chargeFlash.gameObject.SetActive(value: false);
	}
}
