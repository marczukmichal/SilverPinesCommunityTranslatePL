using UnityEngine;

public class SlidePlatform : MonoBehaviour
{
	[SerializeField]
	private float m_slideAngle;

	public Vector2 GetSlideDirection()
	{
		Vector2 right = Vector2.right;
		right = right.Rotate(m_slideAngle);
		return base.transform.TransformDirection(right);
	}
}
