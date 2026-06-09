using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MeleeWeaponStatusInfoPanel : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup m_canvasGroup;

	[SerializeField]
	private Image m_meleeItemIcon;

	[SerializeField]
	private GameObject m_brokenItemInfo;

	[SerializeField]
	private DurabilityInfoBar m_durabilityBar;

	[SerializeField]
	private UnityEvent m_onShowing;

	private bool m_isWeaponBroken;

	public UnityAction<bool> OnShowingChanged;

	private bool m_isShowing;

	private static WaitForSeconds m_fadeWaitTime = new WaitForSeconds(4f);

	public bool IsVisible
	{
		get
		{
			if (m_canvasGroup.isActiveAndEnabled)
			{
				return m_canvasGroup.alpha > 0f;
			}
			return false;
		}
	}

	public bool IsShowing
	{
		get
		{
			return m_isShowing;
		}
		set
		{
			if (m_isShowing != value)
			{
				m_isShowing = value;
				OnShowingChanged?.Invoke(value);
			}
		}
	}

	private void Start()
	{
		m_canvasGroup.alpha = 0f;
	}

	private void OnEnable()
	{
		GlobalReferences.Instance.EventChannels.Generic.MeleeWeaponDurabilityChanged.Register(OnMeleeWeaponStatusChanged);
		GlobalReferences.Instance.EventChannels.Gameplay.ShowAimingUI.Register(AimPanelEvent);
		GlobalReferences.Instance.EventChannels.Inventory.EquippedMeleeItemChanged.Register(ChangedMeleeWeapon);
		GlobalReferences.Instance.EventChannels.Generic.MeleeWeaponRefresh.Register(RefreshEvent);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.Generic.MeleeWeaponDurabilityChanged.Unregister(OnMeleeWeaponStatusChanged);
		GlobalReferences.Instance.EventChannels.Gameplay.ShowAimingUI.Unregister(AimPanelEvent);
		GlobalReferences.Instance.EventChannels.Inventory.EquippedMeleeItemChanged.Unregister(ChangedMeleeWeapon);
		GlobalReferences.Instance.EventChannels.Generic.MeleeWeaponRefresh.Unregister(RefreshEvent);
	}

	private void AimPanelEvent(bool showing)
	{
		if (showing)
		{
			InstantHide();
		}
	}

	public void InstantHide()
	{
		StopAllCoroutines();
		DOTween.Kill(m_canvasGroup);
		m_canvasGroup.alpha = 0f;
	}

	private void OnMeleeWeaponStatusChanged(MeleeWeaponDurabilityChangedData eventData)
	{
		UpdateForWeapon(eventData.m_meleeWeapon, eventData.m_startingDurabilityProportion, eventData.m_endingDurabilityProportion, animate: true);
	}

	private void UpdateForWeapon(MeleeWeaponItemInstance instance, float startingDurability, float endingDurability, bool animate)
	{
		bool flag = endingDurability < startingDurability;
		m_durabilityBar.SetDurabilityInfo(instance, animate, flag);
		if (flag)
		{
			m_durabilityBar.AnimateTakeDamage(instance, startingDurability, endingDurability);
		}
		m_meleeItemIcon.sprite = instance.ItemDefinition.InventorySprite;
		m_brokenItemInfo.SetActive(instance.IsBroken());
		m_isWeaponBroken = instance.IsBroken();
		m_onShowing.Invoke();
		StopAllCoroutines();
		StartCoroutine(ShowFlow());
	}

	private void RefreshEvent(MeleeWeaponDurabilityChangedData eventData)
	{
		if (eventData.m_meleeWeapon == null || !eventData.m_meleeWeapon.IsBroken())
		{
			InstantHide();
		}
		else
		{
			UpdateForWeapon(eventData.m_meleeWeapon, eventData.m_meleeWeapon.DurabilityProportion, eventData.m_meleeWeapon.DurabilityProportion, animate: false);
		}
	}

	private void ChangedMeleeWeapon(ItemEquippedEventData itemChangedEvent)
	{
		if (itemChangedEvent.m_itemInstance != null)
		{
			MeleeWeaponItemInstance meleeWeaponItemInstance = itemChangedEvent.m_itemInstance as MeleeWeaponItemInstance;
			UpdateForWeapon(itemChangedEvent.m_itemInstance as MeleeWeaponItemInstance, meleeWeaponItemInstance.DurabilityProportion, meleeWeaponItemInstance.DurabilityProportion, animate: false);
		}
	}

	private IEnumerator ShowFlow()
	{
		IsShowing = true;
		yield return m_canvasGroup.DOFade(1f, 1f).SetSpeedBased(isSpeedBased: true).WaitForCompletion();
		yield return m_fadeWaitTime;
		yield return new WaitUntil(() => !m_isWeaponBroken);
		yield return m_canvasGroup.DOFade(0f, 1f).SetSpeedBased(isSpeedBased: true).WaitForCompletion();
		IsShowing = false;
	}
}
