using UnityEngine;

public class SurfaceType : MonoBehaviour
{
	[SerializeField]
	private SurfaceSettings m_surfaceSettings;

	[SerializeField]
	private float m_surfaceHeightOffset;

	private float m_surfaceHeight;

	public float SurfaceHeight => m_surfaceHeight + m_surfaceHeightOffset;

	public SurfaceSettings Surface
	{
		get
		{
			if (m_surfaceSettings != null)
			{
				return m_surfaceSettings;
			}
			return GlobalReferences.Instance.DefaultSurfaceSettings;
		}
		set
		{
			m_surfaceSettings = value;
		}
	}

	private void Start()
	{
		m_surfaceHeight = base.transform.position.y;
		Collider2D component = GetComponent<Collider2D>();
		if (component != null)
		{
			m_surfaceHeight = component.bounds.max.y;
		}
	}
}
