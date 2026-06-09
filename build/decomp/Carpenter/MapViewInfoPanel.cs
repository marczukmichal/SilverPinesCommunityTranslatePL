using TMPro;
using UnityEngine;

public class MapViewInfoPanel : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI m_mapViewTextName;

	[SerializeField]
	private TextMeshProUGUI m_floorNameText;

	public void SetActiveMapView(MapViewMetadata mapView)
	{
		if (mapView == null)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		base.gameObject.SetActive(value: true);
		m_mapViewTextName.text = mapView.AreaName;
	}

	public void SetActiveFloor(Map3DFloor floor)
	{
		if (floor != null)
		{
			m_floorNameText.text = floor.FloorName;
		}
		m_floorNameText.gameObject.SetActive(floor != null);
	}
}
