using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public class StatusInfoPopup : MonoBehaviour
{
	[Header("UI")]
	[SerializeField]
	private Image m_leftImage;

	[SerializeField]
	private Image m_rightImage;

	[SerializeField]
	private RectTransform m_rectTransform;

	[SerializeField]
	private CanvasGroup m_canvasGroup;

	[SerializeField]
	private TextMeshProUGUI m_label;

	[Header("Animation")]
	[SerializeField]
	private float m_animFadeInTime = 0.1f;

	[SerializeField]
	private float m_animFadeOutTime = 0.25f;

	[SerializeField]
	private float m_holdTimeShort = 1f;

	[SerializeField]
	private float m_holdTimeLong = 4f;

	[SerializeField]
	private float m_shakeTime = 0.6f;

	[SerializeField]
	private float m_shakeStrength = 30f;

	[Header("Weapon")]
	[SerializeField]
	private Sprite m_weaponBreakIcon;

	[SerializeField]
	private LocalizedString m_weaponBreakText;

	[SerializeField]
	private LocalizedString m_weaponDurabilityLowText;

	[SerializeField]
	private LocalizedString m_weaponRepairedText;

	[SerializeField]
	private Sprite m_meleeWeaponEquippedIcon;

	[SerializeField]
	private Sprite m_rangedWeaponEquippedIcon;

	[SerializeField]
	private Sprite m_secondaryWeaponEquippedIcon;

	[SerializeField]
	private LocalizedString m_weaponEquippedText;

	[Header("Coffee")]
	[SerializeField]
	private Sprite m_coffeeRightIcon;

	[SerializeField]
	private Sprite m_coffeeLeftIcon;

	[SerializeField]
	private Color m_healthIconColor;

	[SerializeField]
	private LocalizedString m_coffeeConsumedText;

	[SerializeField]
	private AudioEvent m_coffeeConsumedAudioEvent;

	[Header("Battery")]
	[SerializeField]
	private Sprite m_batteryLowChargeIcon;

	[SerializeField]
	private Sprite m_batteryDepletedIcon;

	[SerializeField]
	private LocalizedString m_batteryLowText;

	[SerializeField]
	private LocalizedString m_batteryDepletedText;

	private Coroutine m_activeCoroutine;

	private void OnEnable()
	{
		m_rectTransform.gameObject.SetActive(value: false);
		GlobalReferences.Instance.EventChannels.Gameplay.CoffeeConsumed.Register(OnCoffeeConsumed);
		GlobalReferences.Instance.EventChannels.Inventory.ItemShortcutUsed.Register(OnItemShortcutUsed);
		GlobalReferences.Instance.EventChannels.Inventory.BatteryLowItem.Register(OnBatteryChargeLow);
		GlobalReferences.Instance.EventChannels.Inventory.BatteryDepletedItem.Register(OnBatteryDepleted);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.Gameplay.CoffeeConsumed.Unregister(OnCoffeeConsumed);
		GlobalReferences.Instance.EventChannels.Inventory.ItemShortcutUsed.Unregister(OnItemShortcutUsed);
		GlobalReferences.Instance.EventChannels.Inventory.BatteryLowItem.Unregister(OnBatteryChargeLow);
		GlobalReferences.Instance.EventChannels.Inventory.BatteryDepletedItem.Unregister(OnBatteryDepleted);
	}

	private void ShowStatusPopup(Sprite leftImage, Color leftColor, Sprite rightImage, Color rightColor, string text, float holdTime)
	{
		m_leftImage.sprite = leftImage;
		m_leftImage.color = leftColor;
		m_rightImage.sprite = rightImage;
		m_rightImage.color = rightColor;
		m_label.text = text;
		StopAnimation();
		m_activeCoroutine = StartCoroutine(AnimateOut(shake: false, holdTime));
	}

	private void OnCoffeeConsumed()
	{
		ShowStatusPopup(m_coffeeLeftIcon, Color.white, m_coffeeRightIcon, m_healthIconColor, m_coffeeConsumedText.GetLocalizedString(), m_holdTimeLong);
		if ((bool)m_coffeeConsumedAudioEvent)
		{
			m_coffeeConsumedAudioEvent.Play2D();
		}
	}

	private void OnMeleeWeaponBroken(ItemInstance weapon)
	{
		ShowStatusPopup(m_weaponBreakIcon, Color.red, weapon.ItemSprite, Color.white, m_weaponBreakText.GetLocalizedString(), m_holdTimeShort);
	}

	private void OnMeleeWeaponDurabilityLow(ItemInstance weapon)
	{
		ShowStatusPopup(m_weaponBreakIcon, Color.yellow, weapon.ItemSprite, Color.white, m_weaponDurabilityLowText.GetLocalizedString(), m_holdTimeShort);
	}

	private void OnMeleeWeaponDurabilityRepaired(ItemInstance weapon)
	{
		ShowStatusPopup(m_weaponBreakIcon, Color.green, weapon.ItemSprite, Color.white, m_weaponRepairedText.GetLocalizedString(), m_holdTimeShort);
	}

	private void OnBatteryChargeLow(ItemInstance item)
	{
		ShowStatusPopup(m_batteryLowChargeIcon, Color.yellow, item.ItemSprite, Color.white, m_batteryLowText.GetLocalizedString(), m_holdTimeShort);
	}

	private void OnBatteryDepleted(ItemInstance item)
	{
		ShowStatusPopup(m_batteryDepletedIcon, Color.red, item.ItemSprite, Color.white, m_batteryDepletedText.GetLocalizedString(), m_holdTimeShort);
	}

	private void OnMeleeWeaponEquipped(ItemInstance weapon)
	{
		if (weapon != null)
		{
			ShowStatusPopup(m_meleeWeaponEquippedIcon, Color.white, weapon.ItemSprite, Color.white, m_weaponEquippedText.GetLocalizedString(), m_holdTimeShort);
		}
	}

	private void OnItemShortcutUsed(ItemInstance item)
	{
		if (item != null)
		{
			Sprite leftImage = null;
			if (item.ItemDefinition is ProjectileWeaponItemDefinition)
			{
				leftImage = m_rangedWeaponEquippedIcon;
			}
			else if (item.ItemDefinition is MeleeWeaponItemDefinition)
			{
				leftImage = m_meleeWeaponEquippedIcon;
			}
			else if (item.ItemDefinition is SecondaryWeaponItemDefinition)
			{
				leftImage = m_secondaryWeaponEquippedIcon;
			}
			ShowStatusPopup(leftImage, Color.white, item.ItemSprite, Color.white, m_weaponEquippedText.GetLocalizedString(), m_holdTimeShort);
		}
	}

	private void StopAnimation()
	{
		if (m_activeCoroutine != null)
		{
			DOTween.Kill(m_rectTransform);
			StopCoroutine(m_activeCoroutine);
		}
	}

	private IEnumerator AnimateOut(bool shake, float holdTime)
	{
		m_rectTransform.gameObject.SetActive(value: true);
		m_canvasGroup.alpha = 0f;
		yield return m_canvasGroup.DOFade(1f, m_animFadeInTime).WaitForCompletion();
		if (shake)
		{
			m_rectTransform.DOShakeAnchorPos(m_shakeTime, m_shakeStrength);
		}
		yield return new WaitForSeconds(holdTime);
		yield return m_canvasGroup.DOFade(0f, m_animFadeOutTime).WaitForCompletion();
		m_rectTransform.gameObject.SetActive(value: false);
	}
}
