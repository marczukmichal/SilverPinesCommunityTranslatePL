using UnityEngine;
using UnityEngine.Serialization;

public class FieldOfViewArea : MonoBehaviour
{
	[SerializeField]
	private Mesh m_mesh;

	[Header("Match Camera Bounds")]
	[SerializeField]
	private CameraBounds m_targetCameraBound;

	[SerializeField]
	private Vector2 m_margin;

	[FormerlySerializedAs("m_zDepth")]
	[SerializeField]
	private float m_zDepthStart = 0.75f;

	[SerializeField]
	private float m_zDepthEnd = 0.75f;

	private bool m_isVisible;

	private float m_obscuringAlpha;

	private RaycastHit2D[] m_raycastHits = new RaycastHit2D[8];

	public Mesh Mesh => m_mesh;

	public Matrix4x4 Matrix => base.transform.localToWorldMatrix;

	public bool IsVisible => m_isVisible;

	public float ObscuringAlpha => m_obscuringAlpha;

	public bool ShouldRender(Camera camera)
	{
		if (camera == null)
		{
			return false;
		}
		if (m_mesh == null)
		{
			return false;
		}
		if (m_obscuringAlpha <= 0f)
		{
			return false;
		}
		Bounds bounds = new Bounds(base.transform.position, base.transform.TransformVector(m_mesh.bounds.size));
		return GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(camera), bounds);
	}

	private void Start()
	{
		if (m_mesh == null)
		{
			Debug.LogError("Field of view area is missing a mesh! " + base.gameObject.name, this);
		}
		else
		{
			ApplyTargetCameraBound();
		}
	}

	private void ApplyTargetCameraBound()
	{
		if ((bool)m_targetCameraBound)
		{
			base.transform.SetParent(m_targetCameraBound.transform);
			float z = (m_zDepthStart + m_zDepthEnd) * 0.5f;
			base.transform.localPosition = new Vector3(0f, 0f, z);
			Vector3 size = m_targetCameraBound.ViewBounds.size;
			size.x += m_margin.x;
			size.y += m_margin.y;
			size.z = Mathf.Abs(m_zDepthStart - m_zDepthEnd);
			base.transform.localScale = size;
		}
	}

	private void OnValidate()
	{
		if (base.gameObject.scene.IsValid())
		{
			ApplyTargetCameraBound();
		}
	}

	public void UpdateVisibilityFromViewer(FieldOfViewViewer viewer)
	{
		Bounds bounds = m_mesh.bounds;
		bounds.center = base.transform.TransformPoint(m_mesh.bounds.center);
		bounds.size = base.transform.TransformVector(m_mesh.bounds.size);
		Vector3 position = viewer.transform.position;
		position.z = bounds.center.z;
		bounds.size = new Vector3(bounds.size.x, bounds.size.y, 100f);
		if (bounds.Contains(position))
		{
			m_isVisible = true;
			return;
		}
		Vector2 end = bounds.ClosestPoint(viewer.transform.position);
		Vector2 start = viewer.transform.position;
		ContactFilter2D contactFilter = default(ContactFilter2D);
		contactFilter.SetLayerMask(GameLayers.EnvironmentMask);
		int num = Physics2D.Linecast(start, end, contactFilter, m_raycastHits);
		for (int i = 0; i < num; i++)
		{
			RaycastHit2D raycastHit2D = m_raycastHits[i];
			NewSideDoor componentInParent = raycastHit2D.collider.GetComponentInParent<NewSideDoor>();
			if (componentInParent != null && componentInParent.DoorAnimationActive)
			{
				num--;
			}
		}
		m_isVisible = num == 0;
	}

	public void ForceUpdateAlpha()
	{
		m_obscuringAlpha = (m_isVisible ? 0f : 1f);
	}

	private void Update()
	{
		float num = (m_isVisible ? 0f : 1f);
		float num2 = ((num > m_obscuringAlpha) ? 3f : 1f);
		m_obscuringAlpha = Mathf.MoveTowards(m_obscuringAlpha, num, Time.deltaTime * num2);
	}

	private void OnEnable()
	{
		GlobalReferences.Instance.Sets.FieldOfView.FieldOfViewAreaSet.Add(this);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.Sets.FieldOfView.FieldOfViewAreaSet.Remove(this);
	}

	private void OnDrawGizmos()
	{
		if (m_mesh != null)
		{
			Vector3 center = base.transform.TransformPoint(m_mesh.bounds.center);
			Gizmos.color = Color.white;
			Gizmos.DrawWireCube(center, base.transform.TransformVector(m_mesh.bounds.size));
		}
		Gizmos.color = Color.white;
	}

	private void OnDrawGizmosSelected()
	{
		if (m_mesh != null)
		{
			Vector3 center = base.transform.TransformPoint(m_mesh.bounds.center);
			Gizmos.color = Color.white;
			Gizmos.DrawWireCube(center, Vector3.Scale(m_mesh.bounds.size, base.transform.lossyScale));
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireMesh(m_mesh, 0, base.transform.position, base.transform.rotation, base.transform.lossyScale);
		}
		Gizmos.color = Color.white;
	}

	public bool ShouldEffectObject(ObjectVisibility hider)
	{
		return ShouldEffectBounds(hider.Bounds);
	}

	public Bounds GetWorldBounds(float depth = 1f)
	{
		Bounds bounds = m_mesh.bounds;
		bounds.center = base.transform.TransformPoint(bounds.center);
		bounds.size = Vector3.Scale(bounds.size, base.transform.lossyScale);
		bounds.size = new Vector3(bounds.size.x, bounds.size.y, depth);
		return bounds;
	}

	public bool ShouldEffectBounds(Bounds bounds)
	{
		return GetWorldBounds(1000f).Intersects(bounds);
	}

	public static bool IsBoundsVisible(Bounds bounds)
	{
		foreach (FieldOfViewArea item in GlobalReferences.Instance.Sets.FieldOfView.FieldOfViewAreaSet)
		{
			if (!item.IsVisible && item.ShouldEffectBounds(bounds))
			{
				return false;
			}
		}
		return true;
	}
}
