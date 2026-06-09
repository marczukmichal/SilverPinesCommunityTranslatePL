using UnityEngine;

public class WorldMapBounds : MonoBehaviour
{
	[SerializeField]
	private Bounds m_bounds;

	[SerializeField]
	private float m_maxZoom = 100f;

	public Bounds WorldBounds => new Bounds(base.transform.TransformPoint(m_bounds.center), Vector3.Scale(m_bounds.size, base.transform.lossyScale));

	public float MaxZoom => m_maxZoom;

	private void OnDrawGizmos()
	{
		Bounds worldBounds = WorldBounds;
		Gizmos.DrawWireCube(worldBounds.center, worldBounds.size);
	}
}
