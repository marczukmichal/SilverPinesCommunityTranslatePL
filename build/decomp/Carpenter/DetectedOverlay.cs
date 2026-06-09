using UnityEngine;
using UnityEngine.UI;

public class DetectedOverlay : MonoBehaviour
{
	[SerializeField]
	private AISensesSet m_aiSensesSet;

	[SerializeField]
	private Image m_image;

	[SerializeField]
	private Color m_detectedColor;

	[SerializeField]
	private float m_fadeRate = 4f;

	private void Update()
	{
		Color b;
		if (m_aiSensesSet.GetHighestTargetState() == AISenses.TargetState.Detected)
		{
			b = m_detectedColor;
		}
		else
		{
			b = Color.black;
			b.a = 0f;
		}
		Color color = Color.Lerp(m_image.color, b, Time.deltaTime * m_fadeRate);
		m_image.color = color;
	}
}
