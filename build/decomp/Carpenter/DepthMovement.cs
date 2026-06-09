using UnityEngine;

public class DepthMovement : MonoBehaviour
{
	[SerializeField]
	private Vector3 m_position1;

	[SerializeField]
	private Vector3 m_position2;

	private Vector3 m_leftPos;

	private Vector3 m_rightPos;

	private void Awake()
	{
		Vector3 vector = base.transform.TransformPoint(m_position1);
		Vector3 vector2 = base.transform.TransformPoint(m_position2);
		m_leftPos = ((vector.x < vector2.x) ? vector : vector2);
		m_rightPos = ((vector.x < vector2.x) ? vector2 : vector);
	}

	public float GetZDepth(Vector3 position)
	{
		float t = Mathf.Clamp01(position.x.Remap(m_leftPos.x, m_rightPos.x, 0f, 1f));
		return Mathf.Lerp(m_leftPos.z, m_rightPos.z, t);
	}
}
