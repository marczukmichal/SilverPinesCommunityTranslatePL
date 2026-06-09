using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class CancelHandler : MonoBehaviour, ICancelHandler, IEventSystemHandler
{
	[SerializeField]
	private UnityEvent m_cancelEvent;

	public void OnCancel(BaseEventData eventData)
	{
		m_cancelEvent.Invoke();
	}
}
