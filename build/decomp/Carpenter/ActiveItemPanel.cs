using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ActiveItemPanel : MonoBehaviour
{
	[SerializeField]
	private Inventory m_inventory;

	[SerializeField]
	private ActiveItemPanelInstance m_prefab;

	[SerializeField]
	private Transform m_parent;

	[SerializeField]
	private CanvasGroup m_canvasGroup;

	[Header("Animation")]
	[SerializeField]
	private float m_fadeInTime = 0.5f;

	[SerializeField]
	private float m_fadeOutTime = 0.5f;

	[Header("Positioning")]
	[SerializeField]
	private RectTransform m_rectTransform;

	[SerializeField]
	private MeleeWeaponStatusInfoPanel m_meleeStatusPanel;

	[SerializeField]
	private AimingInfoPanel m_aimingPanel;

	[SerializeField]
	private float m_normalOffset = 60f;

	[SerializeField]
	private float m_otherPanelActiveOffset = 170f;

	private List<ActiveItemPanelInstance> m_activeInstances = new List<ActiveItemPanelInstance>();

	private Coroutine m_activeCoroutine;

	private bool m_isAiming;

	private bool m_alwaysShow;

	private void OnEnable()
	{
		GlobalReferences.Instance.EventChannels.Inventory.ItemActivatedChanged.Register(OnItemActivatedChanged);
		GlobalReferences.Instance.EventChannels.LevelTransition.LevelTransitionCompleted.Register(Show);
		GlobalReferences.Instance.EventChannels.Gameplay.ShowAimingUI.Register(IsAiming);
		GlobalReferences.Instance.EventChannels.Inventory.BatteryLowItem.Register(OnBatteryEvent);
		GlobalReferences.Instance.EventChannels.Inventory.BatteryChargeLevelNotifyItem.Register(OnBatteryEvent);
		GlobalReferences.Instance.EventChannels.Inventory.BatteryDepletedItem.Register(OnBatteryEvent);
		RefreshList();
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.Inventory.ItemActivatedChanged.Unregister(OnItemActivatedChanged);
		GlobalReferences.Instance.EventChannels.LevelTransition.LevelTransitionCompleted.Unregister(Show);
		GlobalReferences.Instance.EventChannels.Gameplay.ShowAimingUI.Unregister(IsAiming);
		GlobalReferences.Instance.EventChannels.Inventory.BatteryLowItem.Unregister(OnBatteryEvent);
		GlobalReferences.Instance.EventChannels.Inventory.BatteryChargeLevelNotifyItem.Unregister(OnBatteryEvent);
		GlobalReferences.Instance.EventChannels.Inventory.BatteryDepletedItem.Unregister(OnBatteryEvent);
	}

	private void IsAiming(bool aiming)
	{
		m_isAiming = aiming;
		if (aiming)
		{
			Show();
		}
	}

	private void OnItemActivatedChanged(ItemInstance itemInstance)
	{
		RefreshList();
	}

	private void OnBatteryEvent(ItemInstance itemInstance)
	{
		Show();
	}

	private void RefreshList()
	{
		foreach (ActiveItemPanelInstance activeInstance in m_activeInstances)
		{
			Object.Destroy(activeInstance.gameObject);
		}
		m_activeInstances.Clear();
		foreach (ItemInstance item in m_inventory.ItemList)
		{
			if (item.CanBeActivated() && item.Activated)
			{
				ActiveItemPanelInstance activeItemPanelInstance = Object.Instantiate(m_prefab, m_parent);
				activeItemPanelInstance.SetItem(item);
				m_activeInstances.Add(activeItemPanelInstance);
			}
		}
		Show();
	}

	private void Show()
	{
		if (m_activeCoroutine != null)
		{
			StopCoroutine(m_activeCoroutine);
		}
		m_activeCoroutine = StartCoroutine(ShowCoroutine());
	}

	private IEnumerator ShowCoroutine()
	{
		m_canvasGroup.DOFade(1f, m_fadeInTime);
		if (m_isAiming || m_alwaysShow)
		{
			yield return new WaitUntil(() => !m_isAiming && !m_alwaysShow);
		}
		else
		{
			yield return new WaitForSeconds(5f);
		}
		m_canvasGroup.DOFade(0f, m_fadeOutTime);
		m_activeCoroutine = null;
	}

	private void Update()
	{
		bool flag = false;
		if (m_activeInstances != null && m_activeInstances.Count > 0)
		{
			foreach (ItemInstance item in m_inventory.ItemList)
			{
				if (item is PoweredItemInstance poweredItemInstance && poweredItemInstance.ChargeAmount <= GameUtils.Constants.s_lowBatteryPoint)
				{
					flag = true;
				}
			}
		}
		if (m_alwaysShow != flag)
		{
			m_alwaysShow = flag;
			if (m_alwaysShow)
			{
				Show();
			}
		}
		if (m_canvasGroup.alpha > 0f)
		{
			bool flag2 = m_aimingPanel.IsVisible || m_meleeStatusPanel.IsVisible;
			Vector2 anchoredPosition = m_rectTransform.anchoredPosition;
			anchoredPosition.y = (flag2 ? m_otherPanelActiveOffset : m_normalOffset);
			m_rectTransform.anchoredPosition = anchoredPosition;
		}
	}
}
