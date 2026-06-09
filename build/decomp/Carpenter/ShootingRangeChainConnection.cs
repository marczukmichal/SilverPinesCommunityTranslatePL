using UnityEngine;

public class ShootingRangeChainConnection : MonoBehaviour
{
	[SerializeField]
	private WireRenderer m_wireRenderer;

	private ShootingRangeController m_controller;

	private void Awake()
	{
		m_controller = Object.FindAnyObjectByType<ShootingRangeController>();
	}

	private void LateUpdate()
	{
		if (m_controller != null && m_controller.GameActive && m_controller.WeaponChainTransform != null)
		{
			m_wireRenderer.m_anchorPoints[0] = Vector3.zero;
			m_wireRenderer.m_anchorPoints[1] = base.transform.InverseTransformPoint(m_controller.WeaponChainTransform.position);
			m_wireRenderer.GenerateLine();
		}
	}
}
