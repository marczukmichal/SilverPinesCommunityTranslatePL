using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MapFloorsListInfo : MonoBehaviour
{
	[SerializeField]
	private MapFloorsListEntry m_floorIconPrefab;

	[SerializeField]
	private Transform m_floorIconsParent;

	private List<MapFloorsListEntry> m_floors;

	public UnityAction<Map3DFloor> m_onRequestFloorEvent;

	private Map3DFloor m_cachedActiveFloor;

	public void SetFloorsData(List<Map3DFloor> floors, BaseMapAreaMetadata currentPlayerArea)
	{
		Clear();
		m_floors = new List<MapFloorsListEntry>();
		if (floors != null)
		{
			foreach (Map3DFloor floor in floors)
			{
				if (floor.ShouldFloorBeVisible())
				{
					MapFloorsListEntry mapFloorsListEntry = Object.Instantiate(m_floorIconPrefab, m_floorIconsParent);
					mapFloorsListEntry.Setup(floor);
					m_floors.Add(mapFloorsListEntry);
				}
			}
		}
		base.gameObject.SetActive(m_floors.Count > 1);
		SetActiveFloor(m_cachedActiveFloor, currentPlayerArea);
	}

	public void SetActiveFloor(Map3DFloor floor, BaseMapAreaMetadata currentPlayerArea)
	{
		m_cachedActiveFloor = floor;
		if (m_floors == null)
		{
			return;
		}
		foreach (MapFloorsListEntry floor2 in m_floors)
		{
			if (floor2.Floor == floor)
			{
				floor2.SetState(MapFloorsListEntry.FloorIconState.Active);
			}
			else
			{
				floor2.SetState(floor2.Floor.IncludesArea(currentPlayerArea) ? MapFloorsListEntry.FloorIconState.InactiveWithPlayer : MapFloorsListEntry.FloorIconState.Inactive);
			}
		}
	}

	private void Clear()
	{
		foreach (Transform item in m_floorIconsParent)
		{
			Object.Destroy(item.gameObject);
		}
		m_floors = null;
	}
}
