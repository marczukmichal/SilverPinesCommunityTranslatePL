using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class NotesListCategoryButton : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
	[SerializeField]
	private LocalizeStringEvent m_localize;

	[SerializeField]
	private Image m_dropdownArrow;

	[SerializeField]
	private Sprite m_collapsedArrowSprite;

	[SerializeField]
	private Sprite m_expandedArrowSprite;

	[SerializeField]
	private TextMeshProUGUI m_text;

	[SerializeField]
	private Color m_normalTextColor;

	[SerializeField]
	private Color m_selectedTextColor;

	[SerializeField]
	private Image m_background;

	[SerializeField]
	private Color m_normalBGColor;

	[SerializeField]
	private Color m_selectedBGColor;

	[SerializeField]
	private AudioEvent m_clickedAudioEvent;

	private LevelRegionSettings m_associatedRegion;

	public UnityAction<NotesListCategoryButton> OnButtonClicked;

	public UnityAction<NotesListCategoryButton> OnButtonSelected;

	private bool m_isExpanded;

	private bool m_isHovered;

	private bool m_isSelected;

	public LevelRegionSettings AssociatedRegion => m_associatedRegion;

	public void SetRegion(LevelRegionSettings region, LocalizedString localizedString)
	{
		m_associatedRegion = region;
		m_localize.StringReference = localizedString;
	}

	public void SetIsExpanded(bool expanded)
	{
		m_isExpanded = expanded;
		RefreshVisuals();
	}

	private void RefreshVisuals()
	{
		m_dropdownArrow.sprite = (m_isExpanded ? m_expandedArrowSprite : m_collapsedArrowSprite);
		bool flag = m_isSelected || m_isHovered;
		Color color3 = (m_dropdownArrow.color = (m_text.color = (flag ? m_selectedTextColor : m_normalTextColor)));
		m_background.color = (flag ? m_selectedBGColor : m_normalBGColor);
	}

	public void ClickCategoryButton()
	{
		if (m_clickedAudioEvent != null)
		{
			m_clickedAudioEvent.Play2D();
		}
		OnButtonClicked?.Invoke(this);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		m_isHovered = true;
		RefreshVisuals();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		m_isHovered = false;
		RefreshVisuals();
	}

	public void OnSelect(BaseEventData eventData)
	{
		m_isSelected = true;
		OnButtonSelected?.Invoke(this);
		RefreshVisuals();
	}

	public void OnDeselect(BaseEventData eventData)
	{
		m_isSelected = false;
		RefreshVisuals();
	}
}
