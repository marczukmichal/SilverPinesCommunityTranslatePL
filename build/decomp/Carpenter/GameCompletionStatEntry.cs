using TMPro;
using UnityEngine;
using UnityEngine.Localization;

public class GameCompletionStatEntry : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI m_label;

	[SerializeField]
	private TextMeshProUGUI m_value;

	public void Set(LocalizedString label, string value)
	{
		m_label.text = label.GetLocalizedString();
		m_value.text = value;
	}

	public void SetValueColor(Color color)
	{
		m_value.color = color;
	}
}
