using UnityEngine;
using UnityEngine.EventSystems;

public class UIHighlightAudioTrigger : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, ISelectHandler
{
	public AudioEvent m_highlightSound;

	private float m_enabledTime;

	private void OnEnable()
	{
		m_enabledTime = Time.unscaledTime;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		PlaySound();
	}

	public void OnSelect(BaseEventData eventData)
	{
		PlaySound();
	}

	private void PlaySound()
	{
		if (base.isActiveAndEnabled && Time.unscaledTime - m_enabledTime > 0.1f)
		{
			m_highlightSound.Play2D();
		}
	}
}
