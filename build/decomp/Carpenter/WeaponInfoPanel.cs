using System.Collections;
using DG.Tweening;
using UnityEngine;

public class WeaponInfoPanel : MonoBehaviour
{
	[Header("Event Channels")]
	[SerializeField]
	private WeaponInfoGameEventChannel m_weaponInfoEventChannel;

	[SerializeField]
	private ActiveUseStateGameEventChannel m_weaponReloadInfoEventChannel;

	[Header("UI")]
	[SerializeField]
	private AmmoCounter m_counter;

	[SerializeField]
	private RectTransform m_rectTransform;

	[SerializeField]
	private CanvasGroup m_reloadBarCanvasGroup;

	[SerializeField]
	private CanvasGroup m_panelCanvasGroup;

	[Header("Animation")]
	[SerializeField]
	private float m_animTime = 0.25f;

	[SerializeField]
	private float m_holdTime = 4f;

	private float m_timer;

	private bool m_isShowing;

	private bool m_isReloading;

	private void OnEnable()
	{
		m_rectTransform.gameObject.SetActive(value: false);
		m_weaponInfoEventChannel.Register(WeaponInfoUpdated);
		m_weaponReloadInfoEventChannel.Register(ReloadInfoEvent);
	}

	private void OnDisable()
	{
		m_weaponInfoEventChannel.Unregister(WeaponInfoUpdated);
		m_weaponReloadInfoEventChannel.Unregister(ReloadInfoEvent);
	}

	private void ReloadInfoEvent(ActiveUseStateInfoData eventData)
	{
		if (m_isReloading = eventData.m_isActive && eventData.m_progress <= 1f)
		{
			m_timer = m_holdTime;
			if (!m_isShowing)
			{
				StartCoroutine(ShowWeaponStatus());
			}
		}
	}

	private void WeaponInfoUpdated(WeaponInfoData eventData)
	{
		if (eventData.m_weaponInstance != null)
		{
			m_counter.PopulateFromWeaponInstance(eventData.m_weaponInstance);
		}
		if (eventData.m_updateType == WeaponInfoData.UpdateType.Reload)
		{
			m_timer = m_holdTime;
			if (!m_isShowing)
			{
				StartCoroutine(ShowWeaponStatus());
			}
		}
	}

	private IEnumerator ShowWeaponStatus()
	{
		m_isShowing = true;
		m_rectTransform.gameObject.SetActive(value: true);
		m_panelCanvasGroup.alpha = 0f;
		yield return m_panelCanvasGroup.DOFade(1f, m_animTime).WaitForCompletion();
		while (m_timer > 0f)
		{
			m_timer -= Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		if (m_isReloading)
		{
			yield return new WaitForEndOfFrame();
		}
		yield return m_panelCanvasGroup.DOFade(0f, m_animTime).WaitForCompletion();
		m_rectTransform.gameObject.SetActive(value: false);
		m_isShowing = false;
	}
}
