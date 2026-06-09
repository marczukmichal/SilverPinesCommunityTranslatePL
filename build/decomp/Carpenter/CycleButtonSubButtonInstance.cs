using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CycleButtonSubButtonInstance : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI m_text;

	[SerializeField]
	private Image m_background;

	[SerializeField]
	private Color m_normalBGColor;

	[SerializeField]
	private Color m_selectedBGColor;

	[SerializeField]
	private Color m_normalTextColor;

	[SerializeField]
	private Color m_selectedTextColor;

	public void Setup(string text)
	{
		m_text.text = text;
	}

	public void SetHighlighted(bool highlighted)
	{
		m_background.color = (highlighted ? m_selectedBGColor : m_normalBGColor);
		m_text.color = (highlighted ? m_selectedTextColor : m_normalTextColor);
	}
}
