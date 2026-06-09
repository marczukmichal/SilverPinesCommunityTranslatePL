using System.Collections.Generic;
using UnityEngine;

public class LocationMapAreaMetadata : BaseMapAreaMetadata
{
	[SerializeField]
	protected List<LevelMetadata> m_levelMetadatas;

	[SerializeField]
	private Vector2 m_size = new Vector2(25f, 5f);

	public override Vector2 GetSize()
	{
		Vector2 size = m_size;
		size.x *= base.AreaTransformSettings.m_scale.x;
		size.y *= base.AreaTransformSettings.m_scale.y;
		return size;
	}

	public override bool TracksLevel(LevelMetadata levelMetadata)
	{
		return m_levelMetadatas.Contains(levelMetadata);
	}

	public override LevelMetadata GetMainTrackedLevelMetadata()
	{
		if (m_levelMetadatas.Count > 0)
		{
			return m_levelMetadatas[0];
		}
		return null;
	}
}
