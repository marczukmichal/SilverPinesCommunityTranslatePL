using System.Collections.Generic;
using UnityEngine;

public class LevelMapSubArea : MonoBehaviour
{
	public enum MapAreaType
	{
		Path,
		Location
	}

	[SerializeField]
	private MapAreaType m_mapAreaType;

	[SerializeField]
	private Bounds m_bounds;

	[SerializeField]
	private BaseMapAreaMetadata m_mapAreaMetadata;

	[SerializeField]
	private LevelMapAreaSeparator m_leftEdgeSeperator;

	[SerializeField]
	private LevelMapAreaSeparator m_rightEdgeSeperator;

	[SerializeField]
	private List<LevelMapAreaStairs> m_stairsSeperators;

	[SerializeField]
	private int m_mapFloor;

	public Bounds Bounds
	{
		get
		{
			return m_bounds;
		}
		set
		{
			m_bounds = value;
		}
	}

	public Vector2 MapSize => base.transform.TransformVector(m_bounds.size);

	public Vector2 MapCenter => base.transform.TransformPoint(m_bounds.center);

	public Rect Rect
	{
		get
		{
			Rect result = default(Rect);
			result.size = m_bounds.size;
			result.center = base.transform.TransformPoint(m_bounds.center);
			return result;
		}
	}

	public LevelMapAreaSeparator LeftEdgeSeperator => m_leftEdgeSeperator;

	public LevelMapAreaSeparator RightEdgeSeperator => m_rightEdgeSeperator;

	public List<LevelMapAreaStairs> Stairs => m_stairsSeperators;

	public int MapFloor => m_mapFloor;
}
