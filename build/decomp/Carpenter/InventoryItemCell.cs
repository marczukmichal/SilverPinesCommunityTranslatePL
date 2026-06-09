using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class InventoryItemCell : MonoBehaviour, IDragHandler, IEventSystemHandler
{
	public enum CombineHighlightMode
	{
		None,
		DimInvalid,
		Highlight,
		ActiveCombiner
	}

	public enum ItemCellState
	{
		None,
		Idle,
		Hovered,
		Selected,
		Using
	}

	public enum EquippedState
	{
		None,
		Ranged,
		Melee,
		Activated,
		Artifact,
		Secondary
	}

	[Header("Components")]
	[SerializeField]
	private CanvasGroup m_canvasGroup;

	[SerializeField]
	private Image m_background;

	[SerializeField]
	private Color m_normalBackgroundColor = Color.gray;

	[SerializeField]
	private Color m_emptyColor = Color.gray;

	[SerializeField]
	private Color m_combineBackgroundColor = Color.yellow;

	[SerializeField]
	private Color m_highlightBackgroundColor = Color.gray;

	[SerializeField]
	private Color m_moveBlockedBackgroundColor = Color.red;

	[SerializeField]
	private Image m_image;

	[SerializeField]
	private Color m_baseItemColor = Color.white;

	[SerializeField]
	private TextMeshProUGUI m_stackCount;

	[SerializeField]
	private Button m_button;

	[SerializeField]
	private Image m_border;

	[SerializeField]
	private Image m_selectionArrow;

	[SerializeField]
	private TextMeshProUGUI m_itemName;

	[SerializeField]
	private AmmoCounter m_ammoCounter;

	[SerializeField]
	private BatteryChargeCounter m_batteryChargeCounter;

	[SerializeField]
	private DurabilityInfoBar m_durabilityInfo;

	[SerializeField]
	private Image m_brokenDurabilityIcon;

	[SerializeField]
	private GameObject m_shortcutMarker;

	[Header("Sizing")]
	[SerializeField]
	private Vector2 m_baseSize = new Vector2(96f, 96f);

	[SerializeField]
	private float m_itemImageMargin = 4f;

	[SerializeField]
	private float m_gridGapSize = 4f;

	[Header("Border Visuals")]
	[SerializeField]
	private Color m_hoveredBorderColor;

	[SerializeField]
	private Color m_selectedBorderColor;

	[SerializeField]
	private Color m_usingBorderColor;

	[SerializeField]
	private Color m_combiningBorderColor;

	[Header("Interactable Settings")]
	[SerializeField]
	private bool m_canSelect = true;

	[Header("Equipped Icon Settings")]
	[SerializeField]
	private Image m_equippedImageBGImage;

	[SerializeField]
	private Image m_equippedImageIcon;

	[SerializeField]
	private Sprite m_rangedIconSprite;

	[SerializeField]
	private Sprite m_meleeIconSprite;

	[SerializeField]
	private Sprite m_artifactIconSprite;

	[SerializeField]
	private Sprite m_secondaryIconSprite;

	[SerializeField]
	private Sprite m_activatedIconSprite;

	[SerializeField]
	private Sprite m_deactivatedIconSprite;

	[SerializeField]
	private Color m_equippedBackgroundColor;

	[SerializeField]
	private Color m_equippedItemColor;

	[SerializeField]
	private TextMeshProUGUI m_equippedText;

	[Header("Combine")]
	[SerializeField]
	private Color m_activeCombineItemColor;

	[Header("Input")]
	[SerializeField]
	private InputState m_inputState;

	[Header("Glow")]
	[SerializeField]
	private Image m_slotGlow;

	[SerializeField]
	private Color m_equipGlowColor;

	[SerializeField]
	private Color m_activatedGlowColor;

	[SerializeField]
	private Color m_selectGlowColor;

	[SerializeField]
	private Color m_combineItemGlowColor;

	[Header("Audio")]
	[SerializeField]
	private AudioEvent m_cellSelectAudioEvent;

	[Header("Effects")]
	[SerializeField]
	private Color m_wrongItemColorFlash;

	[Header("New Slot Animation")]
	[SerializeField]
	private Image m_newSlotGlow;

	public UnityEvent<InventoryItemCell> m_onItemSelectEvent;

	public UnityEvent<InventoryItemCell> m_onDragEvent;

	private ItemInstance m_itemInstance;

	private Vector2Int m_cellPosition;

	private ProjectileWeaponItemInstance m_weaponInstance;

	private bool m_hovered;

	private bool m_selected;

	private bool m_itemBeingUsed;

	private bool m_moveBlocked;

	private CombineHighlightMode m_combineHighlightMode;

	public bool m_blockAudioEvents;

	private bool m_animating;

	private bool m_currentImageRotation;

	private ItemCellState m_cellState;

	private EquippedState m_equippedState;

	private bool m_isShortcut;

	public Vector2 Size => m_baseSize;

	public ItemInstance ItemInstance => m_itemInstance;

	public bool CanBeRotated => true;

	public Vector2Int RotatedItemSize
	{
		get
		{
			if (m_itemInstance != null)
			{
				return m_itemInstance.GetItemSize(m_itemInstance.ItemRotated);
			}
			return Vector2Int.one;
		}
	}

	public bool IsHovered => m_hovered;

	public bool MoveBlocked
	{
		get
		{
			return m_moveBlocked;
		}
		set
		{
			if (m_moveBlocked != value)
			{
				m_moveBlocked = value;
				UpdateDecoration();
			}
		}
	}

	public ItemCellState CellState
	{
		get
		{
			return m_cellState;
		}
		set
		{
			if (m_cellState != value)
			{
				m_cellState = value;
				UpdateDecoration();
			}
		}
	}

	public EquippedState ItemEquippedState
	{
		get
		{
			return m_equippedState;
		}
		set
		{
			if (value != m_equippedState)
			{
				m_equippedState = value;
				UpdateDecoration();
			}
		}
	}

	public bool IsShortcut
	{
		get
		{
			return m_isShortcut;
		}
		set
		{
			if (value != m_isShortcut)
			{
				m_isShortcut = value;
				UpdateDecoration();
			}
		}
	}

	public Vector2Int CellPosition
	{
		get
		{
			if (m_itemInstance == null)
			{
				return m_cellPosition;
			}
			return m_itemInstance.ItemGridPosition;
		}
	}

	public bool CellRotated
	{
		get
		{
			if (m_itemInstance == null)
			{
				return false;
			}
			return m_itemInstance.ItemRotated;
		}
	}

	private void OnDestroy()
	{
		if (m_weaponInstance != null)
		{
			ProjectileWeaponItemInstance weaponInstance = m_weaponInstance;
			weaponInstance.OnWeaponAmmoUpdated = (UnityAction)Delegate.Remove(weaponInstance.OnWeaponAmmoUpdated, new UnityAction(OnWeaponUpdated));
		}
	}

	private Color GetBaseItemColor()
	{
		if (m_itemInstance is ArtifactItemInstance artifactItemInstance && !artifactItemInstance.PoweredUp)
		{
			return Color.gray;
		}
		return m_baseItemColor;
	}

	private void UpdateDecoration()
	{
		if (m_canvasGroup != null)
		{
			m_canvasGroup.alpha = 1f;
		}
		switch (m_equippedState)
		{
		case EquippedState.None:
			m_image.color = GetBaseItemColor();
			m_equippedImageBGImage.gameObject.SetActive(value: false);
			m_equippedImageIcon.gameObject.SetActive(value: false);
			m_slotGlow.gameObject.SetActive(value: false);
			break;
		case EquippedState.Ranged:
			m_image.color = m_equippedItemColor;
			m_equippedImageIcon.sprite = m_rangedIconSprite;
			m_equippedImageBGImage.gameObject.SetActive(value: true);
			m_equippedImageIcon.gameObject.SetActive(value: true);
			if (m_equippedText != null)
			{
				m_equippedText.gameObject.SetActive(value: true);
			}
			m_slotGlow.gameObject.SetActive(value: true);
			m_slotGlow.color = m_equipGlowColor;
			break;
		case EquippedState.Melee:
			m_image.color = m_equippedItemColor;
			m_equippedImageIcon.sprite = m_meleeIconSprite;
			m_equippedImageBGImage.gameObject.SetActive(value: true);
			m_equippedImageIcon.gameObject.SetActive(value: true);
			if (m_equippedText != null)
			{
				m_equippedText.gameObject.SetActive(value: true);
			}
			m_slotGlow.gameObject.SetActive(value: true);
			m_slotGlow.color = m_equipGlowColor;
			break;
		case EquippedState.Activated:
			m_image.color = GetBaseItemColor();
			m_equippedImageIcon.sprite = m_activatedIconSprite;
			m_equippedImageBGImage.gameObject.SetActive(value: true);
			m_equippedImageIcon.gameObject.SetActive(value: true);
			if (m_equippedText != null)
			{
				m_equippedText.gameObject.SetActive(value: true);
				m_equippedText.text = ItemInstance.ItemDefinition.Activatable.ActivatedItemCellText;
			}
			m_slotGlow.gameObject.SetActive(value: true);
			m_slotGlow.color = m_activatedGlowColor;
			break;
		case EquippedState.Artifact:
			m_image.color = m_equippedItemColor;
			m_equippedImageIcon.sprite = m_artifactIconSprite;
			m_equippedImageBGImage.gameObject.SetActive(value: true);
			m_equippedImageIcon.gameObject.SetActive(value: true);
			if (m_equippedText != null)
			{
				m_equippedText.gameObject.SetActive(value: true);
			}
			m_slotGlow.gameObject.SetActive(value: true);
			m_slotGlow.color = m_equipGlowColor;
			break;
		case EquippedState.Secondary:
			m_image.color = m_equippedItemColor;
			m_equippedImageIcon.sprite = m_secondaryIconSprite;
			m_equippedImageBGImage.gameObject.SetActive(value: true);
			m_equippedImageIcon.gameObject.SetActive(value: true);
			if (m_equippedText != null)
			{
				m_equippedText.gameObject.SetActive(value: true);
			}
			m_slotGlow.gameObject.SetActive(value: true);
			m_slotGlow.color = m_equipGlowColor;
			break;
		}
		switch (m_cellState)
		{
		case ItemCellState.None:
		case ItemCellState.Idle:
		case ItemCellState.Hovered:
			if (m_selectionArrow != null)
			{
				m_selectionArrow.gameObject.SetActive(value: false);
			}
			if (m_slotGlow != null)
			{
				m_slotGlow.gameObject.SetActive(m_equippedState != EquippedState.None);
				Color color = m_equipGlowColor;
				if (m_itemInstance != null && m_itemInstance.Activated)
				{
					color = m_activatedGlowColor;
				}
				m_slotGlow.color = color;
			}
			break;
		case ItemCellState.Selected:
			if (m_selectionArrow != null)
			{
				m_selectionArrow.gameObject.SetActive(value: true);
			}
			if (m_slotGlow != null)
			{
				m_slotGlow.gameObject.SetActive(value: true);
				m_slotGlow.color = m_selectGlowColor;
			}
			break;
		}
		bool flag = false;
		if (m_border != null)
		{
			switch (m_cellState)
			{
			case ItemCellState.Hovered:
				m_border.gameObject.SetActive(value: true);
				m_border.color = m_hoveredBorderColor;
				flag = true;
				break;
			case ItemCellState.Selected:
				m_border.gameObject.SetActive(value: true);
				m_border.color = m_selectedBorderColor;
				flag = true;
				break;
			case ItemCellState.Using:
				m_border.gameObject.SetActive(value: true);
				m_border.color = m_usingBorderColor;
				break;
			default:
				m_border.gameObject.SetActive(value: false);
				break;
			}
		}
		m_background.color = (flag ? m_highlightBackgroundColor : m_normalBackgroundColor);
		switch (m_combineHighlightMode)
		{
		case CombineHighlightMode.DimInvalid:
		{
			Color color2 = m_image.color;
			color2 *= 0.5f;
			m_image.color = color2;
			m_canvasGroup.alpha = 0.75f;
			break;
		}
		case CombineHighlightMode.ActiveCombiner:
			m_image.color = m_activeCombineItemColor;
			break;
		case CombineHighlightMode.Highlight:
		{
			m_slotGlow.color = m_combineItemGlowColor;
			m_background.color = m_combineBackgroundColor;
			m_slotGlow.gameObject.SetActive(value: true);
			Color combiningBorderColor = m_combiningBorderColor;
			if (m_cellState != ItemCellState.Hovered)
			{
				combiningBorderColor.a *= 0.2f;
			}
			m_border.color = combiningBorderColor;
			m_border.gameObject.SetActive(value: true);
			break;
		}
		}
		if (m_moveBlocked)
		{
			m_background.color = m_moveBlockedBackgroundColor;
		}
		if (m_shortcutMarker != null)
		{
			m_shortcutMarker.SetActive(IsShortcut);
		}
		SetItemSprite();
	}

	private void OnEnable()
	{
		if (m_cellState == ItemCellState.None)
		{
			m_cellState = ItemCellState.Idle;
			if (m_selectionArrow != null)
			{
				m_selectionArrow.gameObject.SetActive(value: false);
			}
			if (m_border != null)
			{
				m_border.gameObject.SetActive(value: false);
			}
		}
	}

	private void OnDisable()
	{
		m_hovered = false;
		m_selected = false;
		CellState = ItemCellState.Idle;
		if ((bool)m_border)
		{
			DOTween.Kill(m_border);
		}
		DOTween.Kill(m_image.transform);
	}

	public void SetItem(ItemInstance itemInstance, bool modifySize = true, bool overrideRotation = false, bool forceRotationValue = false)
	{
		m_itemInstance = itemInstance;
		if (m_batteryChargeCounter != null)
		{
			m_batteryChargeCounter.gameObject.SetActive(value: false);
		}
		if (m_stackCount != null)
		{
			m_stackCount.gameObject.SetActive(value: false);
		}
		if (m_ammoCounter != null)
		{
			m_ammoCounter.gameObject.SetActive(value: false);
		}
		if (m_durabilityInfo != null)
		{
			m_durabilityInfo.gameObject.SetActive(value: false);
		}
		if (m_brokenDurabilityIcon != null)
		{
			m_brokenDurabilityIcon.gameObject.SetActive(value: false);
		}
		if (itemInstance != null)
		{
			if (m_background != null)
			{
				m_background.color = m_normalBackgroundColor;
			}
			m_image.gameObject.SetActive(value: true);
			m_image.color = GetBaseItemColor();
			SetItemSprite();
			if (m_itemName != null)
			{
				m_itemName.text = ItemInstance.ItemDefinition.ItemName;
			}
			bool flag = false;
			bool active = false;
			if (itemInstance is ProjectileWeaponItemInstance)
			{
				m_weaponInstance = itemInstance as ProjectileWeaponItemInstance;
				if (m_ammoCounter != null)
				{
					ProjectileWeaponItemInstance weaponInstance = m_weaponInstance;
					weaponInstance.OnWeaponAmmoUpdated = (UnityAction)Delegate.Combine(weaponInstance.OnWeaponAmmoUpdated, new UnityAction(OnWeaponUpdated));
					m_ammoCounter.PopulateFromWeaponInstance(m_weaponInstance);
					m_ammoCounter.gameObject.SetActive(value: true);
					if (m_stackCount != null)
					{
						m_stackCount.gameObject.SetActive(value: false);
					}
				}
				else if (m_stackCount != null)
				{
					m_stackCount.text = m_weaponInstance.AmmoCount.ToString();
					m_stackCount.gameObject.SetActive(value: true);
				}
			}
			else if (itemInstance is KeyRingItemInstance)
			{
				if (m_stackCount != null)
				{
					KeyRingItemInstance keyRingItemInstance = itemInstance as KeyRingItemInstance;
					m_stackCount.text = keyRingItemInstance.GetKeyCount().ToString();
					m_stackCount.gameObject.SetActive(value: true);
				}
			}
			else if (itemInstance is MeleeWeaponItemInstance)
			{
				MeleeWeaponItemInstance obj = itemInstance as MeleeWeaponItemInstance;
				flag = obj.WeaponDefinition.HasDurability;
				_ = obj.DurabilityProportion;
				active = obj.IsBroken();
				if (m_stackCount != null)
				{
					m_stackCount.gameObject.SetActive(value: false);
				}
			}
			else if (itemInstance is PoweredItemInstance)
			{
				PoweredItemInstance poweredInstance = itemInstance as PoweredItemInstance;
				m_batteryChargeCounter.PopulateFromPoweredItemInstance(poweredInstance);
				m_batteryChargeCounter.gameObject.SetActive(value: true);
			}
			else if (itemInstance is RefillableItemInstance)
			{
				if (m_stackCount != null)
				{
					RefillableItemInstance refillableItemInstance = itemInstance as RefillableItemInstance;
					m_stackCount.text = refillableItemInstance.Uses.ToString();
					m_stackCount.gameObject.SetActive(value: true);
				}
			}
			else if (m_stackCount != null)
			{
				m_stackCount.gameObject.SetActive(itemInstance.StackSize > 1 || itemInstance.ItemDefinition is AmmunitionItemDefinition);
				m_stackCount.text = itemInstance.StackSize.ToString();
			}
			if (m_durabilityInfo != null)
			{
				m_durabilityInfo.gameObject.SetActive(flag);
				if (flag)
				{
					MeleeWeaponItemInstance itemInstance2 = itemInstance as MeleeWeaponItemInstance;
					m_durabilityInfo.SetDurabilityInfo(itemInstance2, animateStateChange: false, isLoss: false);
				}
			}
			if (m_brokenDurabilityIcon != null)
			{
				m_brokenDurabilityIcon.gameObject.SetActive(active);
			}
			if (modifySize)
			{
				bool rotated = m_itemInstance.ItemRotated;
				if (overrideRotation)
				{
					rotated = forceRotationValue;
				}
				UpdateItemSize(m_itemInstance.ItemDefinition.ItemSize, rotated);
			}
		}
		else
		{
			if (m_background != null)
			{
				m_background.color = m_emptyColor;
			}
			m_image.gameObject.SetActive(value: false);
			if (m_itemName != null)
			{
				m_itemName.text = "";
			}
			if (modifySize)
			{
				UpdateItemSize(new Vector2Int(1, 1), rotated: false);
			}
		}
	}

	private void OnWeaponUpdated()
	{
		m_ammoCounter.PopulateFromWeaponInstance(m_weaponInstance);
	}

	private void SetItemSprite()
	{
		if (ItemInstance != null)
		{
			m_image.sprite = ItemInstance.ItemSprite;
			m_image.transform.localScale = Vector2.one * m_itemInstance.ItemDefinition.InventoryItemCellSpriteSizeScalar;
		}
	}

	private void UpdateItemSize(Vector2Int itemSize, bool rotated)
	{
		Vector2Int itemSize2 = ItemInstance.GetItemSize(itemSize, rotated);
		Vector2 sizeDelta = new Vector2(itemSize2.x, itemSize2.y);
		sizeDelta *= m_baseSize;
		sizeDelta.x += (float)(itemSize2.x - 1) * m_gridGapSize;
		sizeDelta.y += (float)(itemSize2.y - 1) * m_gridGapSize;
		GetComponent<RectTransform>().sizeDelta = sizeDelta;
		m_image.transform.rotation = Quaternion.Euler(0f, 0f, rotated ? 90f : 0f);
		SetImageSize();
	}

	private void SetImageSize()
	{
		if (ItemInstance != null)
		{
			Vector2Int itemSize = ItemInstance.ItemDefinition.ItemSize;
			Vector2 sizeDelta = new Vector2(itemSize.x, itemSize.y);
			sizeDelta *= m_baseSize;
			sizeDelta.x += (float)(itemSize.x - 1) * m_gridGapSize;
			sizeDelta.y += (float)(itemSize.y - 1) * m_gridGapSize;
			sizeDelta.x -= m_itemImageMargin * 2f;
			sizeDelta.y -= m_itemImageMargin * 2f;
			m_image.rectTransform.sizeDelta = sizeDelta;
		}
	}

	public void SetCellPosition(Vector2Int cellPosition, bool rotated)
	{
		m_cellPosition = cellPosition;
		if (m_itemInstance != null)
		{
			m_itemInstance.ItemGridPosition = cellPosition;
			m_itemInstance.ItemRotated = rotated;
			UpdateItemSize(m_itemInstance.ItemDefinition.ItemSize, rotated);
		}
	}

	public void OnPressed()
	{
		if (m_canSelect)
		{
			m_onItemSelectEvent.Invoke(this);
		}
	}

	private void UpdateSelectionState()
	{
		ItemCellState cellState = ItemCellState.Idle;
		if (m_itemBeingUsed)
		{
			cellState = ItemCellState.Using;
		}
		else if (m_selected)
		{
			cellState = ItemCellState.Selected;
		}
		else if (m_hovered)
		{
			cellState = ItemCellState.Hovered;
		}
		CellState = cellState;
	}

	public void SetSelected(bool selected)
	{
		if (m_selected != selected)
		{
			m_selected = selected;
			UpdateSelectionState();
		}
	}

	public void SetItemBeingUsed(bool itemUsed)
	{
		if (m_itemBeingUsed != itemUsed)
		{
			m_itemBeingUsed = itemUsed;
			UpdateSelectionState();
		}
	}

	public List<Vector2Int> GetCellOccupiedPositions()
	{
		if (m_itemInstance != null)
		{
			return m_itemInstance.GetOccupiedPositions();
		}
		return new List<Vector2Int> { m_cellPosition };
	}

	public void SetHovered(bool hovered)
	{
		if (m_canSelect)
		{
			m_hovered = hovered;
			UpdateSelectionState();
		}
	}

	public void ShowCombineOptionGlow(CombineHighlightMode combineHilightMode)
	{
		m_combineHighlightMode = combineHilightMode;
		UpdateDecoration();
	}

	public void AnimateApplyItemWrongItem()
	{
		if (!m_animating)
		{
			StartCoroutine(AnimateApplyItemWrongItemCoroutine());
		}
	}

	private IEnumerator AnimateApplyItemWrongItemCoroutine()
	{
		m_animating = true;
		m_image.transform.DOPunchScale(new Vector3(0.25f, 0.25f, 0.25f), 0.25f).SetUpdate(isIndependentUpdate: true);
		yield return m_image.DOColor(m_wrongItemColorFlash, 0.25f).SetLoops(4, LoopType.Yoyo).SetUpdate(isIndependentUpdate: true)
			.WaitForCompletion();
		m_image.color = GetBaseItemColor();
		m_image.transform.localScale = Vector2.one * m_itemInstance.ItemDefinition.InventoryItemCellSpriteSizeScalar;
		m_animating = false;
	}

	public void AnimateApplyItemCorrectItem()
	{
		if (!m_animating)
		{
			base.gameObject.SetActive(value: true);
			StartCoroutine(AnimateApplyItemCorrectItemCoroutine());
		}
	}

	private IEnumerator AnimateApplyItemCorrectItemCoroutine()
	{
		m_animating = true;
		yield return m_image.transform.DOPunchScale(new Vector3(-0.25f, -0.25f, -0.25f), 0.25f).SetUpdate(isIndependentUpdate: true).WaitForCompletion();
		m_image.transform.localScale = Vector2.one * m_itemInstance.ItemDefinition.InventoryItemCellSpriteSizeScalar;
		m_animating = false;
	}

	public bool CheckForOverlap(ItemInstance itemInstance, Vector2Int itemPosition, bool rotated)
	{
		List<Vector2Int> occupiedPositions = itemInstance.GetOccupiedPositions(itemPosition, rotated);
		foreach (Vector2Int cellOccupiedPosition in GetCellOccupiedPositions())
		{
			if (occupiedPositions.Contains(cellOccupiedPosition))
			{
				return true;
			}
		}
		return false;
	}

	public bool CheckForOverlapWithPosition(Vector2Int position)
	{
		return GetCellOccupiedPositions().Contains(position);
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			m_onDragEvent.Invoke(this);
		}
	}

	private void SetImageRotation(bool rotated)
	{
		if (m_currentImageRotation != rotated)
		{
			m_currentImageRotation = rotated;
			SetImageSize();
		}
	}
}
