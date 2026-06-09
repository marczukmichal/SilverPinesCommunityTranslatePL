using System;
using UnityEngine;

public class PathMapAreaMetadata : BaseMapAreaMetadata
{
	[Serializable]
	public struct InLevelData
	{
		[SerializeField]
		private bool m_isSubMap;

		[SerializeField]
		private Vector2 m_mapBoundsSize;

		[SerializeField]
		private Vector2 m_mapBoundsOffset;

		[SerializeField]
		private int m_priority;

		[SerializeField]
		private float[] m_referencePoints;

		public bool IsSubMap
		{
			get
			{
				return m_isSubMap;
			}
			set
			{
				m_isSubMap = value;
			}
		}

		public Vector2 MapBoundsSize
		{
			get
			{
				return m_mapBoundsSize;
			}
			set
			{
				m_mapBoundsSize = value;
			}
		}

		public Vector2 MapBoundsOffset
		{
			get
			{
				return m_mapBoundsOffset;
			}
			set
			{
				m_mapBoundsOffset = value;
			}
		}

		public int Priority
		{
			get
			{
				return m_priority;
			}
			set
			{
				m_priority = value;
			}
		}

		public Rect MapBoundsRect
		{
			get
			{
				Vector2 mapBoundsOffset = m_mapBoundsOffset;
				mapBoundsOffset.x -= m_mapBoundsSize.x * 0.5f;
				mapBoundsOffset.y -= m_mapBoundsSize.y * 0.5f;
				return new Rect(mapBoundsOffset, m_mapBoundsSize);
			}
			set
			{
				Rect rect = value;
				m_mapBoundsOffset = rect.center;
				m_mapBoundsSize = rect.size;
			}
		}

		public float[] ReferencesPoints => m_referencePoints;

		public bool EncapsulatesPosition(Vector2 position, float margin = 0.5f)
		{
			Rect mapBoundsRect = MapBoundsRect;
			mapBoundsRect.x -= margin * 0.5f;
			mapBoundsRect.y -= margin * 0.5f;
			mapBoundsRect.width += margin;
			mapBoundsRect.height += margin;
			if (mapBoundsRect.Contains(position))
			{
				return true;
			}
			return false;
		}
	}

	[SerializeField]
	public LevelMetadata m_levelMetadata;

	[SerializeField]
	public InLevelData m_inLevelData;

	protected override void UpdateDynamicData(MapDynamicData.MapAreaDynamicData dynamicData, GameObject player)
	{
		base.UpdateDynamicData(dynamicData, player);
		Vector3 position = player.transform.position;
		position.x -= 2.5f;
		Vector3 position2 = player.transform.position;
		position2.x += 2.5f;
		float b = Mathf.Clamp01(position.x.Remap(m_inLevelData.MapBoundsRect.xMin, m_inLevelData.MapBoundsRect.xMax, 0f, 1f));
		float b2 = Mathf.Clamp01(position2.x.Remap(m_inLevelData.MapBoundsRect.xMin, m_inLevelData.MapBoundsRect.xMax, 0f, 1f));
		float minXNormalizePosition = dynamicData.m_minXNormalizePosition;
		float maxXNormalizePosition = dynamicData.m_maxXNormalizePosition;
		dynamicData.m_minXNormalizePosition = Mathf.Min(dynamicData.m_minXNormalizePosition, b);
		dynamicData.m_maxXNormalizePosition = Mathf.Max(dynamicData.m_maxXNormalizePosition, b2);
		if (!Mathf.Approximately(minXNormalizePosition, dynamicData.m_minXNormalizePosition) || !Mathf.Approximately(maxXNormalizePosition, dynamicData.m_maxXNormalizePosition))
		{
			dynamicData.m_isDirty = true;
		}
	}

	public override Vector2 GetSize()
	{
		return new Vector2(m_inLevelData.MapBoundsSize.x * base.AreaTransformSettings.m_scale.x, 2.5f * base.AreaTransformSettings.m_scale.y);
	}

	public override bool TracksLevel(LevelMetadata levelMetadata)
	{
		return m_levelMetadata == levelMetadata;
	}

	public override bool IsPlayerInArea()
	{
		if (m_inLevelData.IsSubMap)
		{
			if (base.IsPlayerInArea())
			{
				GameObject item = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item;
				if (item != null)
				{
					return m_inLevelData.EncapsulatesPosition(item.transform.position, 0f);
				}
			}
			return false;
		}
		return base.IsPlayerInArea();
	}

	public override float GetDistanceFromPlayer()
	{
		if (m_inLevelData.IsSubMap)
		{
			GameObject item = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item;
			if (item != null)
			{
				Rect mapBoundsRect = m_inLevelData.MapBoundsRect;
				Vector2 a = item.transform.position;
				return Vector2.Distance(b: new Vector2(Mathf.Clamp(a.x, mapBoundsRect.xMin, mapBoundsRect.xMax), Mathf.Clamp(a.y, mapBoundsRect.yMin, mapBoundsRect.yMax)), a: a);
			}
			return float.MaxValue;
		}
		return base.GetDistanceFromPlayer();
	}

	public override LevelMetadata GetMainTrackedLevelMetadata()
	{
		return m_levelMetadata;
	}
}
