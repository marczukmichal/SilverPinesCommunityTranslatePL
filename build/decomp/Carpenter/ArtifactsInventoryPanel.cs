using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.Localization;

public class ArtifactsInventoryPanel : MonoBehaviour
{
	[SerializeField]
	private ArtifactsGameSettings m_artifactSettings;

	[SerializeField]
	private ArtifactItemCell m_artifactCellPrefab;

	[SerializeField]
	private InventoryItemInfoPanel m_itemInfoPanel;

	[SerializeField]
	private PlayerMainInventory m_inventory;

	[SerializeField]
	private ArtifactsSlotsStatusInfo m_slotInfo;

	[SerializeField]
	private LocalizedString m_noFreeMementoSlotsString;

	[Header("Layout")]
	[SerializeField]
	private int m_columnCount;

	[SerializeField]
	private float m_spacing;

	[SerializeField]
	private Vector2 m_padding;

	[SerializeField]
	private float m_oddRowOffset;

	[Header("Audio")]
	[SerializeField]
	private AudioEvent m_equipAudioEvent;

	[SerializeField]
	private AudioEvent m_unequipAudioEvent;

	[SerializeField]
	private AudioEvent m_failedToEquipAudioEvent;

	private ArtifactItemCell m_hoveredArtifact;

	private List<ArtifactItemCell> m_artifactCells = new List<ArtifactItemCell>();

	private bool m_isAnimatingCell;

	private ItemInstance m_queuedHighlight;

	private ArtifactItemCell HoveredArtifact
	{
		get
		{
			return m_hoveredArtifact;
		}
		set
		{
			if (m_hoveredArtifact != value)
			{
				m_hoveredArtifact = value;
				if (m_hoveredArtifact != null && m_hoveredArtifact.CellState != 0)
				{
					m_itemInfoPanel.SetActiveItem(m_hoveredArtifact.ArtifactItemInstance);
				}
				else
				{
					m_itemInfoPanel.SetActiveItem((ItemInstance)null);
				}
			}
		}
	}

	public bool IsAnimating => m_isAnimatingCell;

	private void GenerateCells()
	{
		ArtifactItemDefinition[] artifacts = m_artifactSettings.Artifacts;
		foreach (ArtifactItemDefinition data in artifacts)
		{
			ArtifactItemCell artifactItemCell = UnityEngine.Object.Instantiate(m_artifactCellPrefab, base.transform);
			m_artifactCells.Add(artifactItemCell);
			artifactItemCell.SetData(data);
			artifactItemCell.OnStartHoveredArtifact = (UnityAction<ArtifactItemCell>)Delegate.Combine(artifactItemCell.OnStartHoveredArtifact, new UnityAction<ArtifactItemCell>(OnArtifactStartHovered));
			artifactItemCell.OnStopHoveredArtifact = (UnityAction<ArtifactItemCell>)Delegate.Combine(artifactItemCell.OnStopHoveredArtifact, new UnityAction<ArtifactItemCell>(OnArtifactStopHovered));
			artifactItemCell.OnArtifactClicked = (UnityAction<ArtifactItemCell>)Delegate.Combine(artifactItemCell.OnArtifactClicked, new UnityAction<ArtifactItemCell>(OnArtifactClicked));
		}
		UpdateCellsLayout();
	}

	private void UpdateCellsLayout()
	{
		int num = 0;
		int num2 = 0;
		foreach (ArtifactItemCell artifactCell in m_artifactCells)
		{
			RectTransform component = artifactCell.GetComponent<RectTransform>();
			component.anchorMin = new Vector2(0f, 1f);
			component.anchorMax = new Vector2(0f, 1f);
			Vector2 anchoredPosition = new Vector2((component.rect.size.x + m_spacing) * (float)num2, (component.rect.size.y + m_spacing) * (float)(-num));
			anchoredPosition += m_padding;
			anchoredPosition.x += component.rect.size.x * 0.5f;
			anchoredPosition.y += component.rect.size.y * -0.5f;
			if (num % 2 == 1)
			{
				anchoredPosition.x += m_oddRowOffset;
			}
			component.anchoredPosition = anchoredPosition;
			num2++;
			if (num2 >= m_columnCount)
			{
				num2 = 0;
				num++;
			}
		}
	}

	private void OnArtifactStartHovered(ArtifactItemCell cell)
	{
		if (cell.CellState != 0)
		{
			HoveredArtifact = cell;
		}
	}

	private void OnArtifactStopHovered(ArtifactItemCell cell)
	{
		if (HoveredArtifact == cell)
		{
			HoveredArtifact = null;
		}
	}

	private void OnArtifactClicked(ArtifactItemCell cell)
	{
		AudioEvent audioEvent = null;
		if (cell.CellState != 0)
		{
			if (m_inventory.CanEquipArtifact(cell.ArtifactItemInstance))
			{
				audioEvent = ((!m_inventory.IsArtifactEquipped(cell.ArtifactItemInstance)) ? ((cell.ArtifactItemInstance.ItemDefinition.Audio.OnEquip != null) ? cell.ArtifactItemInstance.ItemDefinition.Audio.OnEquip : m_equipAudioEvent) : ((cell.ArtifactItemInstance.ItemDefinition.Audio.OnUnEquip != null) ? cell.ArtifactItemInstance.ItemDefinition.Audio.OnUnEquip : m_unequipAudioEvent));
				m_inventory.EquipArtifactItem(cell.ArtifactItemInstance);
				RefreshData();
			}
			else
			{
				GlobalReferences.Instance.EventChannels.Hints.DynamicHint.Raise(DynamicHintEvent.NoArtifactSlots);
				audioEvent = m_failedToEquipAudioEvent;
				GlobalReferences.Instance.EventChannels.InGameMenu.MenuInfoMessage.Raise(new MenuInfoMessageData
				{
					m_stringReference = m_noFreeMementoSlotsString
				});
			}
			if (audioEvent != null)
			{
				AudioEvent.Play2D(audioEvent);
			}
		}
	}

	private void RefreshData()
	{
		PlayerMainInventory mainInventory = GlobalReferences.Instance.MainInventory;
		foreach (ArtifactItemCell artifactCell in m_artifactCells)
		{
			ArtifactItemCell.State state = ArtifactItemCell.State.Unknown;
			ArtifactItemInstance artifactOfType = mainInventory.GetArtifactOfType(artifactCell.ItemDefinition);
			if (artifactOfType != null)
			{
				state = ((!mainInventory.IsArtifactEquipped(artifactOfType)) ? ArtifactItemCell.State.Inactive : ArtifactItemCell.State.Active);
			}
			artifactCell.SetState(state);
			artifactCell.ArtifactItemInstance = artifactOfType;
			artifactCell.SetInteractable(interactable: true);
		}
		m_slotInfo.SetSlotsStatus(mainInventory.MaxEquippedArtifactsCount, mainInventory.GetEquippedArtifactCount());
		if (m_queuedHighlight != null)
		{
			HighlightArtifact(m_queuedHighlight);
		}
	}

	private void Awake()
	{
		GenerateCells();
	}

	private void OnEnable()
	{
		RefreshData();
	}

	public void HighlightArtifact(ItemInstance itemInstance)
	{
		if (m_artifactCells == null || m_artifactCells.Count == 0)
		{
			m_queuedHighlight = itemInstance;
			return;
		}
		m_queuedHighlight = null;
		foreach (ArtifactItemCell artifactCell in m_artifactCells)
		{
			artifactCell.SetInteractable(interactable: false);
		}
		foreach (ArtifactItemCell artifactCell2 in m_artifactCells)
		{
			if (artifactCell2.ArtifactItemInstance == itemInstance)
			{
				StartCoroutine(DoHighlightAnimationFlow(artifactCell2));
				break;
			}
		}
	}

	private IEnumerator DoHighlightAnimationFlow(ArtifactItemCell highlightCell)
	{
		yield return new WaitForEndOfFrame();
		foreach (ArtifactItemCell artifactCell in m_artifactCells)
		{
			artifactCell.SetInteractable(interactable: false);
		}
		m_isAnimatingCell = true;
		EventSystem.current.SetSelectedGameObject(highlightCell.gameObject);
		HoveredArtifact = highlightCell;
		yield return highlightCell.DOAnimation();
		m_isAnimatingCell = false;
		foreach (ArtifactItemCell artifactCell2 in m_artifactCells)
		{
			artifactCell2.SetInteractable(interactable: true);
		}
	}
}
