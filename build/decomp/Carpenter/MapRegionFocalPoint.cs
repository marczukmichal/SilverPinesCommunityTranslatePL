using UnityEngine;

public class MapRegionFocalPoint : MonoBehaviour
{
	[SerializeField]
	private LevelRegionSettings m_associatedRegion;

	[SerializeField]
	private float m_zoomLevel = 100f;

	public LevelRegionSettings AssociatedRegion => m_associatedRegion;

	public float ZoomLevel => m_zoomLevel;
}
