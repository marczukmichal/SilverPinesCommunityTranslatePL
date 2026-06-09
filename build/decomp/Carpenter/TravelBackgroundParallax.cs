using UnityEngine;

public class TravelBackgroundParallax : MonoBehaviour
{
	[SerializeField]
	private float m_speed;

	[SerializeField]
	private Vector3 m_direction;

	private void FixedUpdate()
	{
		Vector3 translation = m_direction * m_speed * Time.deltaTime;
		base.transform.Translate(translation);
	}
}
