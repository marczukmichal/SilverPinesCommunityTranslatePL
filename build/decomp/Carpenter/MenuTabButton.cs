using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MenuTabButton : MonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
{
	[SerializeField]
	private Image m_underline;

	[FormerlySerializedAs("m_text")]
	[SerializeField]
	private Graphic m_graphic;

	[SerializeField]
	private Color m_activeColor;

	[SerializeField]
	private Color m_inactiveColor;

	[Header("Background Anim")]
	[SerializeField]
	private Image m_background;

	[SerializeField]
	private Color m_activeBackgroundColor;

	[SerializeField]
	private Color m_inactiveBackgroundColor;

	[Header("Scale Options")]
	[SerializeField]
	private bool m_scaleOnSelected;

	[SerializeField]
	private float m_selectedScale;

	[SerializeField]
	private float m_animTime = 0.5f;

	[SerializeField]
	private bool m_setActiveOnSelect;

	[SerializeField]
	private GameObject m_tabGameObject;

	private bool m_active;

	public GameObject TabGameObject => m_tabGameObject;

	public bool Active
	{
		get
		{
			return m_active;
		}
		set
		{
			if (m_active != value)
			{
				m_active = value;
				if ((bool)m_underline)
				{
					m_underline.DOFade(value ? 1f : 0f, m_animTime);
				}
				if ((bool)m_background)
				{
					m_background.DOColor(value ? m_activeBackgroundColor : m_inactiveBackgroundColor, m_animTime);
				}
				if (m_scaleOnSelected)
				{
					base.transform.localScale = Vector3.one * (value ? m_selectedScale : 1f);
				}
				m_graphic.DOColor(value ? m_activeColor : m_inactiveColor, m_animTime);
			}
		}
	}

	private void OnEnable()
	{
		if (m_underline != null)
		{
			Color color = m_underline.color;
			color.a = (m_active ? 1f : 0f);
			m_underline.color = color;
		}
		if (m_background != null)
		{
			m_background.color = (m_active ? m_activeBackgroundColor : m_inactiveBackgroundColor);
		}
		if (m_scaleOnSelected)
		{
			base.transform.localScale = Vector3.one * (m_active ? m_selectedScale : 1f);
		}
		m_graphic.color = (m_active ? m_activeColor : m_inactiveColor);
	}

	public void ResetState()
	{
		m_active = false;
		if (m_underline != null)
		{
			Color color = m_underline.color;
			color.a = 0f;
			m_underline.color = color;
		}
		if ((bool)m_background)
		{
			m_background.color = m_inactiveBackgroundColor;
		}
		m_graphic.color = m_inactiveColor;
	}

	public void OnSelect(BaseEventData eventData)
	{
		if (m_setActiveOnSelect)
		{
			Active = true;
		}
	}

	public void OnDeselect(BaseEventData eventData)
	{
		if (m_setActiveOnSelect)
		{
			Active = false;
		}
	}

	public void SetActive(bool active)
	{
		Active = active;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (m_setActiveOnSelect)
		{
			Active = true;
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (m_setActiveOnSelect)
		{
			Active = false;
		}
	}
}
