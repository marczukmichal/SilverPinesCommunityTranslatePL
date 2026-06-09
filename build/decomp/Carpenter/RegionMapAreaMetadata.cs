using UnityEngine;

[CreateAssetMenu(menuName = "Map/Region Map Area Metadata")]
public class RegionMapAreaMetadata : BaseMapAreaMetadata
{
	[SerializeField]
	protected LevelRegionSettings m_regionSettings;

	[SerializeField]
	private MapViewMetadata m_mapViewMetadata;

	[SerializeField]
	private BaseMapAreaMetadata[] m_childAreasForReveal;

	[SerializeField]
	private Vector2 m_size = new Vector2(25f, 5f);

	[SerializeField]
	private Vector3 m_regionMapIconOffset;

	[Header("Data Tags")]
	[SerializeField]
	private bool m_hasPhone;

	[SerializeField]
	private bool m_hasShop;

	[SerializeField]
	private bool m_hasStorage;

	public MapViewMetadata MapViewMetadata => m_mapViewMetadata;

	public Vector3 RegionMapIconOffset => m_regionMapIconOffset;

	public bool HasPhone => m_hasPhone;

	public bool HasShop => m_hasShop;

	public bool HasStorage => m_hasStorage;

	public override Vector2 GetSize()
	{
		Vector2 size = m_size;
		size.x *= base.AreaTransformSettings.m_scale.x;
		size.y *= base.AreaTransformSettings.m_scale.y;
		return size;
	}

	public override bool TracksLevel(LevelMetadata levelMetadata)
	{
		return levelMetadata.Region == m_regionSettings;
	}

	public override LevelMetadata GetMainTrackedLevelMetadata()
	{
		return null;
	}

	public override bool HasPlayerVisitedArea()
	{
		BaseMapAreaMetadata[] childAreasForReveal = m_childAreasForReveal;
		for (int i = 0; i < childAreasForReveal.Length; i++)
		{
			if (childAreasForReveal[i].HasPlayerVisitedArea())
			{
				return true;
			}
		}
		return false;
	}
}
