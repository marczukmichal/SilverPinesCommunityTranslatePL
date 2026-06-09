using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.UI;

public class NotesList : MonoBehaviour
{
	[SerializeField]
	private LoreInventory m_loreInventory;

	[SerializeField]
	private GameObject m_notesItemPrefab;

	[SerializeField]
	private GameObject m_notesCategoryHeaderPrefab;

	[SerializeField]
	private Transform m_notesItemButtonListTransform;

	[SerializeField]
	private LocalizedString m_categoryMiscString;

	[Header("Mask Fade")]
	[SerializeField]
	private RectMask2D m_rectMask;

	[SerializeField]
	private ScrollRect m_scrollRect;

	[SerializeField]
	private float m_itemOffsetAmountForMaskFade = 150f;

	[SerializeField]
	private float m_maskFadeOffset = 600f;

	[Header("Inputs")]
	[SerializeField]
	private MenuInputPrompts.AvaialbleInput[] m_availableInputsNormal;

	[SerializeField]
	private MenuInputPrompts.AvaialbleInput[] m_availableInputsExpandCategory;

	[SerializeField]
	private MenuInputPrompts.AvaialbleInput[] m_availableInputsCloseCategory;

	private List<LevelRegionSettings> m_availableCategories;

	private HashSet<LevelRegionSettings> m_expandedCategories;

	private List<NotesItemButton> m_buttons;

	private List<NotesListCategoryButton> m_categoryButtons;

	private LoreEntry m_activeLoreEntry;

	private LoreEntry m_loreEntry;

	public UnityAction<LoreEntry> OnRequestShowLore;

	public UnityAction<LoreEntry> OnRequestLocateLoreOnMap;

	public LoreEntry HighlightedLoreEntry => m_loreEntry;

	private LoreEntry RequestedLoreJumpEntry => m_loreInventory.FocusedLoreEntry;

	public MenuInputPrompts.AvaialbleInput[] AvailableInputs
	{
		get
		{
			if (EventSystem.current != null)
			{
				GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
				if (currentSelectedGameObject != null)
				{
					NotesListCategoryButton component = currentSelectedGameObject.GetComponent<NotesListCategoryButton>();
					if (component != null)
					{
						if (m_expandedCategories.Contains(component.AssociatedRegion))
						{
							return m_availableInputsCloseCategory;
						}
						return m_availableInputsExpandCategory;
					}
				}
			}
			return m_availableInputsNormal;
		}
	}

	private void OnEnable()
	{
		GameInputManager.GameInputActions.UI.SecondaryTabRight.performed += JumpToNextCategory;
		GameInputManager.GameInputActions.UI.SecondaryTabLeft.performed += JumpToPreviousCategory;
		Populate();
	}

	private void OnDisable()
	{
		if (GameInputManager.GameInputActions != null)
		{
			GameInputManager.GameInputActions.UI.SecondaryTabRight.performed -= JumpToNextCategory;
			GameInputManager.GameInputActions.UI.SecondaryTabLeft.performed -= JumpToPreviousCategory;
		}
	}

	public void Populate()
	{
		PopulateCategories();
		PopulateLoreList();
	}

	public void Clear()
	{
		foreach (Transform item in m_notesItemButtonListTransform)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
	}

	private void PopulateCategories()
	{
		m_availableCategories = new List<LevelRegionSettings>();
		m_expandedCategories = new HashSet<LevelRegionSettings>();
		foreach (CollectedLoreInstance item in m_loreInventory.CollectedLore)
		{
			LoreEntry loreEntry = item.LoreEntry;
			if (!m_availableCategories.Contains(loreEntry.AssociatedRegion))
			{
				m_availableCategories.Add(loreEntry.AssociatedRegion);
			}
		}
		if (m_availableCategories.Contains(null))
		{
			m_availableCategories.Remove(null);
			m_availableCategories.Add(null);
		}
	}

	private NotesItemButton GetButtonForLoreEntry(LoreEntry loreEntry)
	{
		foreach (NotesItemButton button in m_buttons)
		{
			if (button.LoreEntry == loreEntry)
			{
				return button;
			}
		}
		return null;
	}

	private void PopulateLoreList()
	{
		Clear();
		m_activeLoreEntry = null;
		m_buttons = new List<NotesItemButton>();
		m_categoryButtons = new List<NotesListCategoryButton>();
		foreach (LevelRegionSettings availableCategory in m_availableCategories)
		{
			bool flag = true;
			foreach (CollectedLoreInstance item in m_loreInventory.CollectedLore)
			{
				LoreEntry loreEntry = item.LoreEntry;
				if (!(loreEntry.AssociatedRegion != availableCategory))
				{
					if (flag)
					{
						NotesListCategoryButton component = UnityEngine.Object.Instantiate(m_notesCategoryHeaderPrefab, m_notesItemButtonListTransform).GetComponent<NotesListCategoryButton>();
						component.SetRegion(localizedString: (!(availableCategory != null)) ? m_categoryMiscString : availableCategory.DisplayNameLocalizedString, region: loreEntry.AssociatedRegion);
						component.OnButtonClicked = (UnityAction<NotesListCategoryButton>)Delegate.Combine(component.OnButtonClicked, new UnityAction<NotesListCategoryButton>(OnCategoryButtonClicked));
						component.OnButtonSelected = (UnityAction<NotesListCategoryButton>)Delegate.Combine(component.OnButtonSelected, new UnityAction<NotesListCategoryButton>(OnCategoryButtonSelected));
						m_categoryButtons.Add(component);
						flag = false;
					}
					NotesItemButton component2 = UnityEngine.Object.Instantiate(m_notesItemPrefab, m_notesItemButtonListTransform).GetComponent<NotesItemButton>();
					component2.Register(OnItemSelected, OnItemHovered, loreEntry);
					m_buttons.Add(component2);
				}
			}
		}
		StartCoroutine(DelayedRefresh());
		UpdateMask();
	}

	private IEnumerator DelayedRefresh()
	{
		yield return new WaitForEndOfFrame();
		LayoutRebuilder.ForceRebuildLayoutImmediate(m_scrollRect.content);
		LayoutRebuilder.ForceRebuildLayoutImmediate(m_scrollRect.GetComponent<RectTransform>());
		Canvas.ForceUpdateCanvases();
		if (RequestedLoreJumpEntry != null)
		{
			foreach (NotesItemButton button in m_buttons)
			{
				if (button.LoreEntry == RequestedLoreJumpEntry)
				{
					FocusOnItemInList(button);
					break;
				}
			}
		}
		else
		{
			m_scrollRect.verticalNormalizedPosition = 1f;
		}
		SetSelectedGameObject();
		UpdateMask();
	}

	public void SetSelectedGameObject()
	{
		if (m_buttons == null || m_buttons.Count <= 0)
		{
			return;
		}
		if (RequestedLoreJumpEntry != null)
		{
			foreach (NotesItemButton button in m_buttons)
			{
				if (button.LoreEntry == RequestedLoreJumpEntry)
				{
					EventSystem.current.SetSelectedGameObject(button.gameObject);
					OnItemSelected(button);
					break;
				}
			}
			return;
		}
		EventSystem.current.SetSelectedGameObject(m_buttons[0].gameObject);
	}

	private void OnLocateItemOnMapPerformed(InputAction.CallbackContext input)
	{
		if (m_activeLoreEntry != null)
		{
			OnRequestLocateLoreOnMap?.Invoke(m_activeLoreEntry);
		}
	}

	private void OnItemSelected(NotesItemButton loreButton)
	{
		m_loreInventory.FocusedLoreEntry = loreButton.LoreEntry;
		if (!m_expandedCategories.Contains(loreButton.LoreEntry.AssociatedRegion))
		{
			ExpandCategory(loreButton.LoreEntry.AssociatedRegion);
		}
		HighlightLoreEntry(loreButton.LoreEntry);
		ShowLoreEntry(loreButton.LoreEntry);
		if (GlobalReferences.Instance.InputState.InputMode == InputState.Mode.Gamepad)
		{
			FocusOnItemInList(loreButton);
		}
	}

	private void ExpandCategory(LevelRegionSettings region)
	{
		if (!m_expandedCategories.Contains(region))
		{
			m_expandedCategories.Add(region);
			RefreshEntryVisibility();
		}
	}

	private void CollapseCategory(LevelRegionSettings region)
	{
		if (m_expandedCategories.Contains(region))
		{
			m_expandedCategories.Remove(region);
			RefreshEntryVisibility();
		}
	}

	private void OnCategoryButtonClicked(NotesListCategoryButton button)
	{
		LevelRegionSettings associatedRegion = button.AssociatedRegion;
		if (!m_expandedCategories.Contains(associatedRegion))
		{
			ExpandCategory(associatedRegion);
		}
		else
		{
			CollapseCategory(associatedRegion);
		}
	}

	private void OnCategoryButtonSelected(NotesListCategoryButton button)
	{
		if (GlobalReferences.Instance.InputState.InputMode == InputState.Mode.Gamepad)
		{
			FocusOnItem(m_scrollRect, button.GetComponent<RectTransform>());
		}
	}

	private void RefreshEntryVisibility()
	{
		foreach (NotesItemButton button in m_buttons)
		{
			button.gameObject.SetActive(m_expandedCategories.Contains(button.LoreEntry.AssociatedRegion));
		}
		foreach (NotesListCategoryButton categoryButton in m_categoryButtons)
		{
			categoryButton.SetIsExpanded(m_expandedCategories.Contains(categoryButton.AssociatedRegion));
		}
	}

	private void OnItemHovered(NotesItemButton loreButton)
	{
		HighlightLoreEntry(loreButton.LoreEntry);
	}

	private void UpdateLoreSelectionHighlight()
	{
		foreach (NotesItemButton button in m_buttons)
		{
			button.SetSelected(button.LoreEntry == m_activeLoreEntry);
		}
	}

	public bool CanExamineSelected()
	{
		if (m_activeLoreEntry != null && m_activeLoreEntry.PageCount > 0)
		{
			return true;
		}
		return false;
	}

	private string GetCategoryText(LevelRegionSettings region)
	{
		if (region == null)
		{
			return m_categoryMiscString.GetLocalizedString();
		}
		return region.DisplayName;
	}

	private void SelectLoreCategory(LevelRegionSettings region)
	{
	}

	private Vector2 CalculateFocusedScrollPosition(ScrollRect scrollView, Vector2 focusPoint)
	{
		Vector2 size = scrollView.content.rect.size;
		Vector2 size2 = ((RectTransform)scrollView.content.parent).rect.size;
		Vector2 scale = scrollView.content.localScale;
		size.Scale(scale);
		focusPoint.Scale(scale);
		Vector2 normalizedPosition = scrollView.normalizedPosition;
		if (scrollView.horizontal && size.x > size2.x)
		{
			normalizedPosition.x = Mathf.Clamp01((focusPoint.x - size2.x * 0.5f) / (size.x - size2.x));
		}
		if (scrollView.vertical && size.y > size2.y)
		{
			normalizedPosition.y = Mathf.Clamp01((focusPoint.y - size2.y * 0.5f) / (size.y - size2.y));
		}
		return normalizedPosition;
	}

	private Vector2 CalculateFocusedScrollPosition(ScrollRect scrollView, RectTransform item)
	{
		Vector2 vector = scrollView.content.InverseTransformPoint(item.transform.TransformPoint(item.rect.center));
		Vector2 size = scrollView.content.rect.size;
		size.Scale(scrollView.content.pivot);
		return CalculateFocusedScrollPosition(scrollView, vector + size);
	}

	public void FocusOnItem(ScrollRect scrollView, RectTransform item)
	{
		scrollView.normalizedPosition = CalculateFocusedScrollPosition(scrollView, item);
	}

	private void FocusOnItemInList(NotesItemButton button)
	{
		FocusOnItem(m_scrollRect, button.GetComponent<RectTransform>());
	}

	public void HighlightLoreEntry(LoreEntry loreEntry)
	{
		m_loreEntry = loreEntry;
		m_activeLoreEntry = loreEntry;
		UpdateLoreSelectionHighlight();
	}

	private void ShowLoreEntry(LoreEntry entry)
	{
		OnRequestShowLore?.Invoke(entry);
	}

	public float BottomMaskVisibility()
	{
		RectTransform content = m_scrollRect.content;
		RectTransform viewport = m_scrollRect.viewport;
		float num = content.rect.height - content.anchoredPosition.y;
		float num2 = viewport.rect.height - num;
		if (num2 >= 0f)
		{
			return 0f;
		}
		return Mathf.Clamp01(Mathf.InverseLerp(0f, 0f - m_itemOffsetAmountForMaskFade, num2));
	}

	public float TopMaskVisibility()
	{
		RectTransform content = m_scrollRect.content;
		if (content.anchoredPosition.y <= 0f)
		{
			return 0f;
		}
		return Mathf.Clamp01(Mathf.InverseLerp(0f, m_itemOffsetAmountForMaskFade, content.anchoredPosition.y));
	}

	private void UpdateMask()
	{
		float num = 1f - TopMaskVisibility();
		float num2 = 1f - BottomMaskVisibility();
		Vector4 padding = new Vector4(0f, num2 * (0f - m_maskFadeOffset), 0f, num * (0f - m_maskFadeOffset));
		m_rectMask.padding = padding;
		m_rectMask.softness = new Vector2Int(0, (int)m_maskFadeOffset);
	}

	private void Update()
	{
		UpdateMask();
	}

	public void JumpToLoreEntry(LoreEntry entry)
	{
		m_loreInventory.FocusedLoreEntry = entry;
	}

	private void JumpToPreviousCategory(InputAction.CallbackContext context)
	{
		OnCategoryJump(forward: false);
	}

	private void JumpToNextCategory(InputAction.CallbackContext context)
	{
		OnCategoryJump(forward: true);
	}

	private void OnCategoryJump(bool forward)
	{
		NotesListCategoryButton notesListCategoryButton = null;
		LevelRegionSettings levelRegionSettings = null;
		GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
		if (currentSelectedGameObject != null)
		{
			NotesListCategoryButton component = currentSelectedGameObject.GetComponent<NotesListCategoryButton>();
			if (component != null)
			{
				levelRegionSettings = component.AssociatedRegion;
			}
		}
		if (levelRegionSettings == null && m_activeLoreEntry != null)
		{
			levelRegionSettings = m_activeLoreEntry.AssociatedRegion;
			if (!forward)
			{
				foreach (NotesListCategoryButton categoryButton in m_categoryButtons)
				{
					if (categoryButton.AssociatedRegion == levelRegionSettings)
					{
						notesListCategoryButton = categoryButton;
						break;
					}
				}
			}
		}
		if (notesListCategoryButton == null)
		{
			if (forward)
			{
				bool flag = false;
				foreach (NotesListCategoryButton categoryButton2 in m_categoryButtons)
				{
					if (categoryButton2.AssociatedRegion == levelRegionSettings)
					{
						flag = true;
					}
					else if (flag)
					{
						notesListCategoryButton = categoryButton2;
						break;
					}
				}
			}
			else
			{
				NotesListCategoryButton notesListCategoryButton2 = null;
				LevelRegionSettings levelRegionSettings2 = null;
				foreach (NotesListCategoryButton categoryButton3 in m_categoryButtons)
				{
					if (categoryButton3.AssociatedRegion == levelRegionSettings)
					{
						notesListCategoryButton = notesListCategoryButton2;
						break;
					}
					if (levelRegionSettings2 != categoryButton3.AssociatedRegion)
					{
						levelRegionSettings2 = categoryButton3.AssociatedRegion;
						notesListCategoryButton2 = categoryButton3;
					}
				}
			}
		}
		if (notesListCategoryButton != null)
		{
			EventSystem.current.SetSelectedGameObject(notesListCategoryButton.gameObject);
		}
	}
}
