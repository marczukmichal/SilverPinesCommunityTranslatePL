using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class Map3DIcon : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
{
	[Header("Map 3D Icon Settings")]
	[SerializeField]
	protected float m_hoveredScale = 1.5f;

	[SerializeField]
	protected float m_normalScale = 1f;

	[SerializeField]
	private CanvasGroup m_canvasGroup;

	[SerializeField]
	private Transform m_scalingTransform;

	public UnityAction<Map3DIcon> OnClicked;

	public UnityAction<Map3DIcon> OnStartedHovered;

	public UnityAction<Map3DIcon> OnStoppedHovered;

	private Vector3 m_mapWorldPosition;

	private float m_zoomAlpha = 1f;

	private bool m_isHovered;

	private bool m_isSelected;

	private bool m_isMuted;

	private RectTransform m_rectTransform;

	[SerializeField]
	private RectTransform m_focusPoint;

	private MapIconGroup m_parentGroup;

	private bool m_clickActive;

	private bool m_clickBlocked;

	public virtual bool CanBeGrouped => true;

	public virtual bool ShouldScale => false;

	public virtual bool HasMenu => false;

	public virtual bool UseZoomLevelAlpha => true;

	public virtual Vector3 InfoPanelOffset => Vector3.zero;

	public float ZoomAlpha
	{
		get
		{
			return m_zoomAlpha;
		}
		set
		{
			if (UseZoomLevelAlpha)
			{
				m_zoomAlpha = value;
				UpdateCanvasGroupAlpha();
			}
		}
	}

	public Vector3 MapWorldPosition
	{
		get
		{
			return m_mapWorldPosition;
		}
		set
		{
			m_mapWorldPosition = value;
		}
	}

	public bool IsHovered
	{
		get
		{
			return m_isHovered;
		}
		set
		{
			if (m_isHovered != value)
			{
				m_isHovered = value;
				if (value)
				{
					OnStartedHovered?.Invoke(this);
				}
				else
				{
					OnStoppedHovered?.Invoke(this);
				}
				UpdateDecoration();
			}
		}
	}

	public bool IsSelected
	{
		get
		{
			return m_isSelected;
		}
		set
		{
			if (m_isSelected != value)
			{
				m_isSelected = value;
				UpdateDecoration();
			}
		}
	}

	public bool IsMuted
	{
		get
		{
			return m_isMuted;
		}
		set
		{
			if (m_isMuted != value)
			{
				m_isMuted = value;
				UpdateDecoration();
			}
		}
	}

	public RectTransform RectTransform
	{
		get
		{
			if (m_rectTransform == null)
			{
				m_rectTransform = GetComponent<RectTransform>();
			}
			return m_rectTransform;
		}
	}

	public Vector2 FocusPoint
	{
		get
		{
			if ((bool)m_focusPoint)
			{
				return m_focusPoint.transform.position;
			}
			Vector2 size = RectTransform.rect.size;
			Vector2 vector = new Vector2((0.5f - RectTransform.pivot.x) * size.x, (0.5f - RectTransform.pivot.y) * size.y);
			return base.transform.position + RectTransform.TransformVector(vector);
		}
	}

	public MapIconGroup ParentGroup => m_parentGroup;

	public void SetGroup(MapIconGroup group)
	{
		m_parentGroup = group;
		IsHovered = false;
	}

	private void Awake()
	{
		base.transform.localScale = Vector3.one * m_normalScale;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (!m_isHovered)
		{
			SendToFront();
			IsHovered = true;
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (m_isHovered)
		{
			IsHovered = false;
		}
	}

	private void UpdateCanvasGroupAlpha()
	{
		if (m_canvasGroup != null)
		{
			float num = (IsHovered ? 1f : m_zoomAlpha);
			m_canvasGroup.alpha = num * (IsMuted ? 0.2f : 1f);
			CanvasGroup canvasGroup = m_canvasGroup;
			bool blocksRaycasts = (m_canvasGroup.interactable = !IsMuted && m_canvasGroup.alpha > 0f);
			canvasGroup.blocksRaycasts = blocksRaycasts;
		}
	}

	protected virtual void UpdateDecoration()
	{
		bool flag = m_isHovered || m_isSelected;
		Transform obj = (m_scalingTransform ? m_scalingTransform : base.transform);
		DOTween.Kill(obj);
		obj.DOScale(flag ? m_hoveredScale : m_normalScale, 5f).SetUpdate(isIndependentUpdate: true).SetSpeedBased(isSpeedBased: true);
		if (m_canvasGroup != null)
		{
			UpdateCanvasGroupAlpha();
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
	}

	public void SendToFront()
	{
		base.transform.SetAsLastSibling();
	}

	public virtual string GetHoverName()
	{
		return base.gameObject.name;
	}

	public void BlockClick()
	{
		m_clickActive = false;
		m_clickBlocked = true;
		StartCoroutine(ClickBlocked());
	}

	private IEnumerator ClickBlocked()
	{
		yield return new WaitForSecondsRealtime(0.1f);
		m_clickBlocked = false;
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (m_clickActive && eventData.button == PointerEventData.InputButton.Left)
		{
			m_clickActive = false;
			OnClicked?.Invoke(this);
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (!m_clickBlocked && eventData.button == PointerEventData.InputButton.Left)
		{
			m_clickActive = true;
		}
	}
}
