using UnityEngine;
using UnityEngine.EventSystems;

public class WindowDragBar : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler
{
	[SerializeField]
	private RectTransform m_rectTransform;

	private RectTransform m_parentRect;

	private ComputerWindow m_window;

	private ComputerDesktop m_computerDesktop;

	private Vector2 m_parentBounds;

	private Vector2 m_startAnchorPos;

	private Canvas m_canvas;

	private float m_requiredMargin = 40f;

	private Vector2 m_startPosition;

	private Vector2 m_startDragOffset;

	private Vector2 m_totalDrag;

	private void Awake()
	{
		m_parentRect = m_rectTransform.parent.GetComponent<RectTransform>();
		m_parentBounds = new Vector2(m_parentRect.rect.width, m_parentRect.rect.height);
		m_window = GetComponentInParent<ComputerWindow>();
		m_computerDesktop = GetComponentInParent<ComputerDesktop>();
		m_canvas = GetComponentInParent<Canvas>();
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		m_startAnchorPos = m_rectTransform.anchoredPosition;
		m_window.Focus();
		m_window.StartDrag();
		m_totalDrag = Vector2.zero;
	}

	public void OnDrag(PointerEventData eventData)
	{
		RectTransformUtility.ScreenPointToLocalPointInRectangle(m_computerDesktop.WindowsParent, Vector2.zero, m_computerDesktop.UICamera, out var localPoint);
		RectTransformUtility.ScreenPointToLocalPointInRectangle(m_computerDesktop.WindowsParent, eventData.delta, m_computerDesktop.UICamera, out var localPoint2);
		m_totalDrag += localPoint2 - localPoint;
		m_rectTransform.anchoredPosition = ClampToBounds(m_startAnchorPos + m_totalDrag);
	}

	private Vector2 ClampToBounds(Vector2 pos)
	{
		pos.x = Mathf.Clamp(pos.x, 0f - (m_rectTransform.rect.width - m_requiredMargin), m_parentBounds.x - m_requiredMargin);
		pos.y = Mathf.Clamp(pos.y, 0f - m_parentBounds.y + m_requiredMargin, 0f);
		return pos;
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		m_window.EndDrag();
	}
}
