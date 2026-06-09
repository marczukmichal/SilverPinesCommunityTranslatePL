using UnityEngine;

public class UITimeParallax : MonoBehaviour
{
	[SerializeField]
	private Vector2 m_parallaxAmount = new Vector2(5f, 10f);

	[SerializeField]
	private Vector2 m_parallaxSpeed = new Vector2(0.25f, 0.4f);

	private float m_xSin;

	private float m_ySin;

	private void Update()
	{
		Vector2 vector = new Vector2(Mathf.Sin(m_xSin) * m_parallaxAmount.x, Mathf.Sin(m_ySin) * m_parallaxAmount.y);
		m_xSin += Time.unscaledDeltaTime * m_parallaxSpeed.x;
		m_ySin += Time.unscaledDeltaTime * m_parallaxSpeed.y;
		base.transform.localPosition = vector;
	}
}
