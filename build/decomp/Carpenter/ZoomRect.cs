using UnityEngine;
using UnityEngine.EventSystems;

public class ZoomRect : MonoBehaviour, IScrollHandler, IEventSystemHandler
{
	private Vector3 m_initialScale;

	[SerializeField]
	private float m_zoomSpeed = 0.1f;

	[SerializeField]
	private float m_maxZoom = 10f;

	private void Awake()
	{
		m_initialScale = base.transform.localScale;
	}

	public void OnScroll(PointerEventData eventData)
	{
		Vector3 vector = Vector3.one * (eventData.scrollDelta.y * m_zoomSpeed);
		Vector3 desiredScale = base.transform.localScale + vector;
		desiredScale = ClampDesiredScale(desiredScale);
		base.transform.localScale = desiredScale;
	}

	private Vector3 ClampDesiredScale(Vector3 desiredScale)
	{
		desiredScale = Vector3.Max(m_initialScale, desiredScale);
		desiredScale = Vector3.Min(m_initialScale * m_maxZoom, desiredScale);
		return desiredScale;
	}
}
