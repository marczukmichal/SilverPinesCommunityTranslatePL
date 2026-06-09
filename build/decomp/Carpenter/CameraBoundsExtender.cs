using UnityEngine;

public class CameraBoundsExtender : MonoBehaviour
{
	[SerializeField]
	public Bounds m_bounds;

	public Bounds WorldBounds
	{
		get
		{
			Bounds bounds = m_bounds;
			bounds.center += base.transform.position;
			return bounds;
		}
	}

	private void Reset()
	{
		m_bounds = new Bounds(Vector3.zero, new Vector3(1f, 2f, 1f));
	}

	public bool OverlapsCameraBounds(CameraBounds cameraBounds)
	{
		Bounds worldBounds = WorldBounds;
		Bounds worldBounds2 = cameraBounds.WorldBounds;
		worldBounds.center = new Vector3(worldBounds.center.x, worldBounds.center.y, 0f);
		worldBounds2.center = new Vector3(worldBounds2.center.x, worldBounds2.center.y, 0f);
		worldBounds.size = new Vector3(worldBounds.size.x, worldBounds.size.y, 10f);
		worldBounds2.size = new Vector3(worldBounds2.size.x, worldBounds2.size.y, 10f);
		return WorldBounds.Intersects(cameraBounds.WorldBounds);
	}

	private void OnEnable()
	{
		GlobalReferences.Instance.Sets.Camera.ActiveCameraBoundsExtenderSet.Add(this);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.Sets.Camera.ActiveCameraBoundsExtenderSet.Remove(this);
	}
}
