using TMPro;
using UnityEngine;

public class ClockPanel : MonoBehaviour
{
	[SerializeField]
	private TimeOfDay m_timeOfDay;

	[SerializeField]
	private TextMeshProUGUI m_text;

	private void Update()
	{
		m_text.text = m_timeOfDay.TimeString();
	}
}
