using System;
using UnityEngine;

[Serializable]
public class MapUserMarkerData
{
	[SerializeField]
	private Vector3 m_mapWorldPosition;

	[SerializeField]
	private int m_mapId;

	[SerializeField]
	private int m_floorIndex;

	[SerializeField]
	private int m_iconSpriteIndex;

	private Texture2D m_texture;

	public Vector3 MapWorldPosition => m_mapWorldPosition;

	public int MapID => m_mapId;

	public int FloorIndex => m_floorIndex;

	public int IconSpriteIndex
	{
		get
		{
			return m_iconSpriteIndex;
		}
		set
		{
			m_iconSpriteIndex = value;
		}
	}

	public MapUserMarkerData(Vector3 mapWorldPosition, int mapId, int floorIndex)
	{
		m_mapWorldPosition = mapWorldPosition;
		m_iconSpriteIndex = 0;
		m_mapId = mapId;
		m_floorIndex = floorIndex;
	}
}
