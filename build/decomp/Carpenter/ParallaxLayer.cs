using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
	[SerializeField]
	private float m_multiplier;

	private Transform m_cameraTransform;

	private Vector3 m_startCameraPos;

	private Vector3 m_startPos;

	private void Start()
	{
		m_cameraTransform = Camera.main.transform;
		m_startCameraPos = m_cameraTransform.position;
		m_startPos = base.transform.position;
	}

	private void LateUpdate()
	{
		Vector3 startPos = m_startPos;
		startPos += m_multiplier * (m_cameraTransform.position - m_startCameraPos);
		base.transform.position = startPos;
	}
}
