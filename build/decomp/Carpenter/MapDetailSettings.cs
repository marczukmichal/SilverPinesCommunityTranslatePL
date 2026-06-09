using System;
using UnityEngine;

[Serializable]
public class MapDetailSettings
{
	[SerializeField]
	private MapDetailType m_type;

	[SerializeField]
	private MapDetailPositioning m_positionAnchor;

	[SerializeField]
	private float m_positionOffset;

	public MapDetailType MapDetailType => m_type;

	public MapDetailPositioning PositionAnchor => m_positionAnchor;

	public float PositionOffset => m_positionOffset;
}
