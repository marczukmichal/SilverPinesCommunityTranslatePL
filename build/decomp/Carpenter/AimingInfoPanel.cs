using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AimingInfoPanel : MonoBehaviour
{
	[Header("Animation Settings")]
	[SerializeField]
	private float m_hideDelay = 1f;

	[SerializeField]
	private float m_showAnimationTime = 0.25f;

	[SerializeField]
	private float m_hideAnimationTime = 1f;

	[SerializeField]
	private float m_visibleCanvasGroupAlpha = 0.5f;

	[Header("UI")]
	[SerializeField]
	private AmmoCounter m_counter;

	[SerializeField]
	private GameObject m_container;

	[SerializeField]
	private CanvasGroup m_canvasGroup;

	[SerializeField]
	private GameObject m_projectileWeaponInfoContainer;

	[SerializeField]
	private GameObject m_secondaryWeaponInfoContainer;

	[SerializeField]
	private Image m_secondarySprite;

	[SerializeField]
	private TextMeshProUGUI m_secondaryCountValue;

	[SerializeField]
	private CanvasGroup m_reloadButtonPrompt;

	[SerializeField]
	private WeaponSwapInfoPanel m_swapWeaponInfo;

	[SerializeField]
	private float m_swapWeaponShowTime;

	[SerializeField]
	public UnityEvent m_onShowing;

	private float m_hideTimer;

	private bool m_isAiming;

	private bool m_isShowing;

	public static UnityAction OnShowingAimInfoPanel;

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
			if (m_isShowing == value)
			{
				return;
			}
			m_isShowing = value;
			if (value)
			{
				m_onShowing.Invoke();
			}
			DOTween.Kill(m_canvasGroup);
			if (value)
			{
				PopulateFromPlayerCharacter();
				m_canvasGroup.DOFade(m_visibleCanvasGroupAlpha, m_showAnimationTime);
				m_container.SetActive(value: true);
			}
			else
			{
				m_canvasGroup.DOFade(0f, m_hideAnimationTime).OnComplete(delegate
				{
					m_container.SetActive(value: false);
				});
			}
		}
	}

	private void OnEnable()
	{
		m_hideTimer = 0f;
		m_container.SetActive(value: false);
		m_canvasGroup.alpha = 0f;
		GlobalReferences.Instance.EventChannels.Gameplay.ShowAimingUI.Register(OnAimStateChanged);
		GlobalReferences.Instance.EventChannels.Generic.WeaponInfo.Register(WeaponInfoUpdated);
		GlobalReferences.Instance.EventChannels.Inventory.WeaponCycled.Register(OnWeaponCycled);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.Gameplay.ShowAimingUI.Unregister(OnAimStateChanged);
		GlobalReferences.Instance.EventChannels.Generic.WeaponInfo.Unregister(WeaponInfoUpdated);
		GlobalReferences.Instance.EventChannels.Inventory.WeaponCycled.Unregister(OnWeaponCycled);
	}

	private void OnWeaponCycled()
	{
		m_hideTimer = Mathf.Min(m_hideTimer, 0f - m_swapWeaponShowTime);
		IsShowing = true;
		PopulateFromPlayerCharacter();
	}

	private void PopulateFromPlayerCharacter()
	{
		GameObject item = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item;
		if (!(item != null))
		{
			return;
		}
		CharacterInventory component = item.GetComponent<CharacterInventory>();
		if (component != null)
		{
			int secondaryCount = 0;
			SecondaryWeaponItemInstance equippedSecondaryItem = component.Inventory.EquippedSecondaryItem;
			if (equippedSecondaryItem != null)
			{
				secondaryCount = component.Inventory.GetItemCount(equippedSecondaryItem.ItemDefinition);
			}
			bool canReload = component.Inventory.CanReloadEquippedItem();
			Populate(component.Inventory.GetEquippedRangedItemIncludingQueued(), canReload, equippedSecondaryItem, secondaryCount);
			m_swapWeaponInfo.Populate(component.Inventory);
		}
	}

	private void OnAimStateChanged(bool isAiming)
	{
		m_isAiming = isAiming;
		if (isAiming)
		{
			IsShowing = true;
		}
	}

	private void WeaponInfoUpdated(WeaponInfoData eventData)
	{
		PopulateFromPlayerCharacter();
	}

	private void Populate(ProjectileWeaponItemInstance projectileWeapon, bool canReload, SecondaryWeaponItemInstance secondaryWeapon, int secondaryCount)
	{
		if (projectileWeapon != null)
		{
			m_projectileWeaponInfoContainer.SetActive(value: true);
			m_counter.PopulateFromWeaponInstance(projectileWeapon);
			m_reloadButtonPrompt.alpha = ((canReload && projectileWeapon.AmmoCount == 0) ? 0.5f : 0f);
		}
		else
		{
			m_projectileWeaponInfoContainer.SetActive(value: false);
		}
		bool flag = secondaryWeapon != null && secondaryCount > 0;
		m_secondaryWeaponInfoContainer.SetActive(flag);
		if (flag)
		{
			m_secondarySprite.sprite = secondaryWeapon.ItemDefinition.InventorySprite;
			m_secondaryCountValue.text = "x" + secondaryCount;
		}
	}

	private void Update()
	{
		if (!IsShowing)
		{
			return;
		}
		if (m_isAiming)
		{
			m_hideTimer = 0f;
			return;
		}
		m_hideTimer += Time.deltaTime;
		if (m_hideTimer > m_hideDelay)
		{
			IsShowing = false;
		}
	}

	public void InstantHide()
	{
		m_isShowing = false;
		DOTween.Kill(m_canvasGroup);
		m_canvasGroup.alpha = 0f;
		m_container.SetActive(value: false);
	}
}
