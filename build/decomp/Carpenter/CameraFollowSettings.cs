using UnityEngine;

[CreateAssetMenu(menuName = "Settings/Camera Follow")]
public class CameraFollowSettings : ScriptableObject
{
	[SerializeField]
	private float m_horizontalOffsetScalar;

	[SerializeField]
	private float m_downOffsetScalar;

	[SerializeField]
	private float m_upOffsetScalar;

	[SerializeField]
	private float m_zoomScale = 1f;

	[SerializeField]
	private Vector2 m_trackingPositionOffset;

	public Vector2 PositionOffset;

	[SerializeField]
	private bool m_offsetIgnoresDirection;

	public float HorizontalOffsetScalar => m_horizontalOffsetScalar;

	public float DownOffsetScalar => m_downOffsetScalar;

	public float UpOffsetScalar => m_upOffsetScalar;

	public float ZoomScale => m_zoomScale;

	public bool OffsetIgnoreDirection => m_offsetIgnoresDirection;
}
