using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class InventoryActionButton : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
	[SerializeField]
	private TextMeshProUGUI m_text;

	[SerializeField]
	private GameObject m_underline;

	[SerializeField]
	private Image m_backgroundImage;

	[SerializeField]
	private Image m_iconImage;

	[SerializeField]
	private Color m_normalColor;

	[SerializeField]
	private Color m_highlightedColor;

	[SerializeField]
	private Color m_activeModeColor;

	[SerializeField]
	private Color m_disabledNormalTextColor;

	[SerializeField]
	private Color m_disabledHighlightTextColor;

	[SerializeField]
	private Color m_normalBackgroundColor;

	[SerializeField]
	private Color m_selectedBackgroundColor;

	[SerializeField]
	private Color m_disabledBackgroundColor;

	[SerializeField]
	private LocalizeStringEvent m_localizeString;

	public UnityAction m_onClickEvent;

	private HoldButton m_holdButton;

	private bool m_selected;

	private bool m_active;

	private bool m_buttonEnabled = true;

	public bool IsButtonActive => m_buttonEnabled;

	private void Start()
	{
		Button component = GetComponent<Button>();
		if (component != null)
		{
			component.onClick.AddListener(OnButtonPressed);
		}
		m_holdButton = GetComponent<HoldButton>();
		if (m_holdButton != null)
		{
			HoldButton holdButton = m_holdButton;
			holdButton.m_onClick = (UnityAction)Delegate.Combine(holdButton.m_onClick, new UnityAction(OnButtonPressed));
			m_holdButton.HoldButtonEnabled = m_buttonEnabled;
		}
		UpdateVisuals();
	}

	public void ClearState()
	{
		m_selected = false;
		UpdateVisuals();
	}

	public void ForceSelected()
	{
		m_selected = true;
		UpdateVisuals();
	}

	private void OnButtonPressed()
	{
		if (IsButtonActive)
		{
			m_onClickEvent?.Invoke();
		}
	}

	private void UpdateVisuals()
	{
		Color color = (m_buttonEnabled ? m_normalColor : m_disabledNormalTextColor);
		if (m_active)
		{
			color = m_activeModeColor;
		}
		else if (m_selected)
		{
			color = (m_buttonEnabled ? m_highlightedColor : m_disabledHighlightTextColor);
		}
		if ((bool)m_underline)
		{
			m_underline.SetActive(m_active);
		}
		Color color4 = (m_iconImage.color = (m_text.color = color));
		if (m_buttonEnabled)
		{
			m_backgroundImage.color = (m_selected ? m_selectedBackgroundColor : m_normalBackgroundColor);
		}
		else
		{
			m_backgroundImage.color = m_disabledBackgroundColor;
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		m_selected = true;
		UpdateVisuals();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		m_selected = false;
		UpdateVisuals();
	}

	public void OnSelect(BaseEventData eventData)
	{
		m_selected = true;
		UpdateVisuals();
	}

	public void OnDeselect(BaseEventData eventData)
	{
		m_selected = false;
		UpdateVisuals();
	}

	public void OnActionActive(bool active)
	{
		if (m_active != active)
		{
			m_active = active;
			UpdateVisuals();
		}
	}

	public void SetButtonEnabled(bool enabled)
	{
		if (m_buttonEnabled != enabled)
		{
			m_buttonEnabled = enabled;
			if (m_holdButton != null)
			{
				m_holdButton.HoldButtonEnabled = enabled;
			}
			UpdateVisuals();
		}
	}

	public void SetString(LocalizedString stringReference)
	{
		m_localizeString.StringReference = stringReference;
	}

	public void SetIconSprite(Sprite icon)
	{
		if (icon != null)
		{
			m_iconImage.sprite = icon;
			m_iconImage.enabled = true;
		}
		else
		{
			m_iconImage.enabled = false;
		}
	}
}
