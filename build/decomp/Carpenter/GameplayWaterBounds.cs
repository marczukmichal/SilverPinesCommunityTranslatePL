using UnityEngine;

public class GameplayWaterBounds : MonoBehaviour
{
	[SerializeField]
	private Bounds m_bounds;

	[SerializeField]
	private float m_swimmingZDepth;

	private bool m_isPlayerInWater;

	private static float m_deepWaterDepth = 0.6f;

	public float GetZDepth()
	{
		return m_swimmingZDepth;
	}

	private void OnEnable()
	{
		GlobalReferences.Instance.Sets.Generic.GameplayWaterSet.Add(this);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.Sets.Generic.GameplayWaterSet.Remove(this);
		if (m_isPlayerInWater)
		{
			GameObject item = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item;
			if (item != null)
			{
				ToggleDeepWater(item, deepWaterEnabled: false);
			}
		}
	}

	private void Update()
	{
		GameObject item = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item;
		bool flag = false;
		if (item != null && IsPositionSubmerged(item.transform.position, m_deepWaterDepth))
		{
			flag = true;
		}
		if (flag != m_isPlayerInWater)
		{
			m_isPlayerInWater = flag;
			ToggleDeepWater(item, m_isPlayerInWater);
		}
	}

	private void ToggleDeepWater(GameObject character, bool deepWaterEnabled)
	{
		StatusEffectReceiver component = character.GetComponent<StatusEffectReceiver>();
		if (component != null)
		{
			component.ApplyStatusEffect(GlobalReferences.Instance.StatusEffects.Generic.DeepWater, deepWaterEnabled ? 1 : (-1));
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.blue;
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.DrawWireCube(m_bounds.center, m_bounds.size);
		Vector3 min = m_bounds.min;
		Vector3 max = m_bounds.max;
		min.y = (max.y = base.transform.position.y);
		min.z = (max.z = m_swimmingZDepth);
		Gizmos.color = Color.magenta;
		Gizmos.matrix = Matrix4x4.identity;
		Gizmos.DrawLine(min, max);
		Gizmos.color = Color.white;
	}

	public bool IsPositionSubmerged(Vector2 position, float submergedDepth)
	{
		Vector3 center = m_bounds.center;
		center.z = 0f;
		m_bounds.center = center;
		Vector2 vector = position;
		vector.y += submergedDepth;
		vector = base.transform.InverseTransformPoint(vector);
		if (m_bounds.Contains(vector))
		{
			return true;
		}
		return false;
	}

	public float GetWaterSurfaceHeight()
	{
		Vector3 max = m_bounds.max;
		return base.transform.TransformPoint(max).y;
	}

	public float GetDistanceFromPoint(Vector2 position)
	{
		Bounds bounds = m_bounds;
		Vector3 center = base.transform.TransformPoint(bounds.center);
		Vector3 vector = Vector3.Scale(bounds.extents, base.transform.lossyScale);
		center.z = 0f;
		bounds = new Bounds(center, vector * 2f);
		if (bounds.Contains(position))
		{
			return -1f;
		}
		return Mathf.Sqrt(bounds.SqrDistance(position));
	}

	public static GameplayWaterBounds GetActiveWaterBound(Vector2 position, float submergedDepth)
	{
		foreach (GameplayWaterBounds item in GlobalReferences.Instance.Sets.Generic.GameplayWaterSet)
		{
			if (item.IsPositionSubmerged(position, submergedDepth))
			{
				return item;
			}
		}
		return null;
	}

	public static bool IsNearWater(Vector2 position, float allowedDistance)
	{
		foreach (GameplayWaterBounds item in GlobalReferences.Instance.Sets.Generic.GameplayWaterSet)
		{
			if (item.GetDistanceFromPoint(position) < allowedDistance)
			{
				return true;
			}
		}
		return false;
	}
}
