using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RebindingInputOverlayPopup : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI m_actionText;

	[SerializeField]
	private InputPrompt m_currentActionPrompt;

	[SerializeField]
	private Slider m_timerSlider;

	public void ShowForRebind(BindableInputActionEntry entry, InputAction action, int bindingIndex, float timeoutDuration)
	{
		base.gameObject.SetActive(value: true);
		m_actionText.text = entry.Settings.GetLocalizedControlName();
		m_currentActionPrompt.SetInputAction(action, bindingIndex);
		float num3 = (m_timerSlider.value = (m_timerSlider.maxValue = timeoutDuration));
	}

	private void Update()
	{
		m_timerSlider.value -= Time.unscaledDeltaTime;
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}
}
