using System.Collections.Generic;
using UnityEngine;

public class Stairs : MonoBehaviour
{
	public enum PassableMode
	{
		Passable,
		AlwaysUseStairs,
		UseOnNeutral
	}

	[Header("Top Entrance")]
	[SerializeField]
	private Vector2 m_topStairsPosition;

	[SerializeField]
	private PassableMode m_topStairsPassable;

	[Header("Bottom Entrance")]
	[SerializeField]
	private Vector2 m_bottomStairsPosition;

	[SerializeField]
	private PassableMode m_bottomsStairsPassable;

	[Tooltip("If this is set, then hitting this collider will also attempt to put a character into stairs mode")]
	[Header("Settings")]
	[SerializeField]
	private Collider2D m_collider;

	[SerializeField]
	private float m_zDepthOffset;

	[SerializeField]
	private SurfaceSettings m_surface;

	[Header("Ignore Colliders")]
	[SerializeField]
	private GameObject[] m_collisionObjectsToIgnoreOnStairs;

	[Header("Deprecated")]
	[SerializeField]
	public Bounds m_topBounds;

	[SerializeField]
	public Bounds m_bottomBounds;

	private List<Collider2D> m_ignoreColliders;

	public PassableMode TopStairsPassableMode => m_topStairsPassable;

	public PassableMode BottomStairsPassableMode => m_bottomsStairsPassable;

	public Collider2D StairsCollider => m_collider;

	public float ZDepthOffset => m_zDepthOffset;

	public SurfaceSettings Surface => m_surface;

	public Bounds TopBounds => GetBoundsForStairsPosition(TopStairsWorldPosition);

	public Bounds BottomBounds => GetBoundsForStairsPosition(BottomStairsWorldPosition);

	public Vector3 TopStairsWorldPosition
	{
		get
		{
			Vector3 position = new Vector3(m_topStairsPosition.x, m_topStairsPosition.y, m_zDepthOffset);
			return base.transform.TransformPoint(position);
		}
	}

	public Vector3 BottomStairsWorldPosition
	{
		get
		{
			Vector3 position = new Vector3(m_bottomStairsPosition.x, m_bottomStairsPosition.y, m_zDepthOffset);
			return base.transform.TransformPoint(position);
		}
	}

	public Vector2 DownDirection => (BottomStairsWorldPosition - TopStairsWorldPosition).normalized;

	public Vector2 UpDirection => (TopStairsWorldPosition - BottomStairsWorldPosition).normalized;

	private void UpdateIgnoreColliders()
	{
		bool flag = m_ignoreColliders == null;
		if (m_ignoreColliders != null)
		{
			foreach (Collider2D ignoreCollider in m_ignoreColliders)
			{
				if (ignoreCollider == null)
				{
					flag = true;
				}
			}
		}
		if (!flag)
		{
			return;
		}
		m_ignoreColliders = new List<Collider2D>();
		GameObject[] collisionObjectsToIgnoreOnStairs = m_collisionObjectsToIgnoreOnStairs;
		foreach (GameObject gameObject in collisionObjectsToIgnoreOnStairs)
		{
			if (!(gameObject == null))
			{
				Collider2D[] componentsInChildren = gameObject.GetComponentsInChildren<Collider2D>(includeInactive: true);
				foreach (Collider2D item in componentsInChildren)
				{
					m_ignoreColliders.Add(item);
				}
			}
		}
	}

	public bool ShouldIgnoreCollider(Collider2D collider)
	{
		UpdateIgnoreColliders();
		if (m_ignoreColliders != null)
		{
			return m_ignoreColliders.Contains(collider);
		}
		return false;
	}

	private void Awake()
	{
		Collider2D component = GetComponent<Collider2D>();
		if (component != null)
		{
			Object.Destroy(component);
		}
		EdgeCollider2D edgeCollider2D = base.gameObject.AddComponent<EdgeCollider2D>();
		List<Vector2> points = new List<Vector2> { m_bottomStairsPosition, m_topStairsPosition };
		edgeCollider2D.SetPoints(points);
		base.gameObject.layer = GameLayers.StairsLayer;
		PlatformEffector2D platformEffector2D = base.gameObject.AddComponent<PlatformEffector2D>();
		if (platformEffector2D != null)
		{
			Vector2 vector = m_bottomStairsPosition - m_topStairsPosition;
			float num2 = (platformEffector2D.rotationalOffset = Mathf.Atan2(vector.y, vector.x) * 57.29578f);
		}
		edgeCollider2D.usedByEffector = true;
	}

	private void OnEnable()
	{
		GlobalReferences.Instance.Sets.Generic.StairsSet.Add(this);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.Sets.Generic.StairsSet.Remove(this);
	}

	private Bounds GetBoundsForStairsPosition(Vector3 stairsPosition)
	{
		Bounds result = default(Bounds);
		Vector3 center = stairsPosition;
		center.y += 1f;
		result.center = center;
		result.size = new Vector3(0.1f, 2f, 10f);
		return result;
	}

	public static bool DoesPositionOverlapWithStairs(Vector2 position)
	{
		foreach (Stairs item in GlobalReferences.Instance.Sets.Generic.StairsSet)
		{
			Bounds topBounds = item.TopBounds;
			Bounds bottomBounds = item.BottomBounds;
			Vector3 center = topBounds.center;
			center.z = 0f;
			topBounds.center = center;
			center = bottomBounds.center;
			center.z = 0f;
			bottomBounds.center = center;
			Vector3 size = topBounds.size;
			size.z = 100f;
			topBounds.size = size;
			size = bottomBounds.size;
			size.z = 100f;
			bottomBounds.size = size;
			if (topBounds.Contains(position))
			{
				return true;
			}
			if (bottomBounds.Contains(position))
			{
				return true;
			}
		}
		return false;
	}

	public Vector3 GetSnapPosition(Vector3 position)
	{
		float value = Mathf.InverseLerp(BottomStairsWorldPosition.x, TopStairsWorldPosition.x, position.x);
		value = Mathf.Clamp01(value);
		return Vector3.Lerp(BottomStairsWorldPosition, TopStairsWorldPosition, value);
	}
}
