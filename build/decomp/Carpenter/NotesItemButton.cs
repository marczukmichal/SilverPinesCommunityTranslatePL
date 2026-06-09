using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class NotesItemButton : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, ISelectHandler
{
	[Header("Text")]
	[SerializeField]
	private TextMeshProUGUI m_text;

	[SerializeField]
	private Color m_textNormalColor;

	[SerializeField]
	private Color m_textSelectedColor;

	[Header("Border")]
	[SerializeField]
	private Image m_borderSprite;

	[SerializeField]
	private Color m_borderNormalColor;

	[SerializeField]
	private Color m_borderSelectedColor;

	[Header("Icon")]
	[SerializeField]
	private Image m_noteIcon;

	[SerializeField]
	private Color m_iconNormalColor;

	[SerializeField]
	private Color m_iconSeleectedColor;

	private UnityAction<NotesItemButton> m_onSelected;

	private UnityAction<NotesItemButton> m_onHovered;

	private LoreEntry m_loreEntry;

	public LoreEntry LoreEntry => m_loreEntry;

	private void Awake()
	{
		SetSelected(selected: false);
	}

	public void Register(UnityAction<NotesItemButton> onButtonSelected, UnityAction<NotesItemButton> onButtonHovered, LoreEntry loreEntry)
	{
		m_loreEntry = loreEntry;
		m_onSelected = (UnityAction<NotesItemButton>)Delegate.Combine(m_onSelected, onButtonSelected);
		m_onHovered = (UnityAction<NotesItemButton>)Delegate.Combine(m_onHovered, onButtonHovered);
		m_noteIcon.sprite = loreEntry.LeadImage;
		m_text.text = loreEntry.Title;
	}

	public void OnPressed()
	{
		m_onSelected?.Invoke(this);
	}

	public void SetSelected(bool selected)
	{
		m_text.color = (selected ? m_textSelectedColor : m_textNormalColor);
		m_borderSprite.color = (selected ? m_borderSelectedColor : m_borderNormalColor);
		m_noteIcon.color = (selected ? m_iconSeleectedColor : m_iconNormalColor);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		m_onHovered?.Invoke(this);
	}

	public void OnSelect(BaseEventData eventData)
	{
		m_onHovered?.Invoke(this);
		m_onSelected?.Invoke(this);
	}
}
