using UnityEngine;

public class GamepadCursorMagnetismTarget : MonoBehaviour
{
	[SerializeField]
	private RectTransform m_targetPosition;

	[SerializeField]
	private bool m_disableMagnetism;

	public Vector3 Position => m_targetPosition.transform.position;

	public bool DisableMagnetism => m_disableMagnetism;

	public void SetMagnetismDisabled(bool disabled)
	{
		m_disableMagnetism = disabled;
	}
}
