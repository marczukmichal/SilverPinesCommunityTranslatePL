using TMPro;
using UnityEngine;

public class MapRegionAreaIcon : Map3DIcon
{
	[Header("UI")]
	[SerializeField]
	private TextMeshProUGUI m_regionNameText;

	[SerializeField]
	private GameObject m_phoneIcon;

	[SerializeField]
	private GameObject m_storageIcon;

	[SerializeField]
	private GameObject m_shopIcon;

	[SerializeField]
	private GameObject m_loreIcon;

	[SerializeField]
	private TextMeshProUGUI m_loreCountText;

	[SerializeField]
	private GameObject m_photosIcon;

	[SerializeField]
	private TextMeshProUGUI m_photosCountText;

	private RegionMapAreaMetadata m_region;

	public override bool CanBeGrouped => false;

	public MapViewMetadata MapViewMetadata => m_region.MapViewMetadata;

	public override Vector3 InfoPanelOffset => new Vector3(0f, -32f, 0f);

	public void SetRegionData(RegionMapAreaMetadata region)
	{
		m_region = region;
		m_regionNameText.text = region.MapViewMetadata.AreaName;
		UserMapData userMapData = GlobalReferences.Instance.UserMapData;
		LoreInventory loreInventory = GlobalReferences.Instance.LoreInventory;
		bool hasPhone = region.HasPhone;
		bool hasStorage = region.HasStorage;
		bool hasShop = region.HasShop;
		int mapViewMetadataLoreCount = loreInventory.GetMapViewMetadataLoreCount(m_region.MapViewMetadata);
		int mapViewMetadataPhotoCount = userMapData.GetMapViewMetadataPhotoCount(m_region.MapViewMetadata);
		m_phoneIcon.SetActive(hasPhone);
		m_storageIcon.SetActive(hasStorage);
		m_shopIcon.SetActive(hasShop);
		m_loreIcon.SetActive(mapViewMetadataLoreCount > 0);
		if (m_loreCountText != null)
		{
			m_loreCountText.text = mapViewMetadataLoreCount.ToString();
		}
		m_photosIcon.SetActive(mapViewMetadataPhotoCount > 0);
		if (m_photosCountText != null)
		{
			m_photosCountText.text = mapViewMetadataPhotoCount.ToString();
		}
	}

	protected override void UpdateDecoration()
	{
		base.UpdateDecoration();
	}
}
