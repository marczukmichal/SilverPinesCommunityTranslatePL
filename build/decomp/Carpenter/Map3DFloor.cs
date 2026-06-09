using UnityEngine;
using UnityEngine.Localization;

public class Map3DFloor : MonoBehaviour
{
	[SerializeField]
	private LocalizedString m_floorNameString;

	[SerializeField]
	private Material m_mapBackgroundMaterial;

	private MapArea3D[] m_cachedAreas;

	private MapAreaConnectionLine[] m_cachedConnectionLines;

	public string FloorName
	{
		get
		{
			if (m_floorNameString.IsEmpty)
			{
				return base.name;
			}
			return m_floorNameString.GetLocalizedString();
		}
	}

	public Material MapBackgroundMaterial => m_mapBackgroundMaterial;

	private MapArea3D[] Areas
	{
		get
		{
			if (m_cachedAreas == null)
			{
				m_cachedAreas = GetComponentsInChildren<MapArea3D>(includeInactive: true);
			}
			return m_cachedAreas;
		}
	}

	private MapAreaConnectionLine[] ConnectionLines
	{
		get
		{
			if (m_cachedConnectionLines == null)
			{
				m_cachedConnectionLines = GetComponentsInChildren<MapAreaConnectionLine>(includeInactive: true);
			}
			return m_cachedConnectionLines;
		}
	}

	public void SetMode(FloorMode floorMode)
	{
		MapArea3D[] areas = Areas;
		for (int i = 0; i < areas.Length; i++)
		{
			areas[i].SetFloorMode(floorMode);
		}
		MapAreaConnectionLine[] connectionLines = ConnectionLines;
		for (int i = 0; i < connectionLines.Length; i++)
		{
			connectionLines[i].SetFloorMode(floorMode);
		}
		Vector3 localPosition = base.transform.localPosition;
		if (floorMode == FloorMode.Below)
		{
			localPosition.y = -0.5f;
		}
		else
		{
			localPosition.y = 0f;
		}
		base.transform.localPosition = localPosition;
	}

	public void RefreshChildrenAsBelowFloor()
	{
		MapArea3D[] areas = Areas;
		for (int i = 0; i < areas.Length; i++)
		{
			areas[i].Refresh(null);
		}
		MapAreaConnectionLine[] connectionLines = ConnectionLines;
		for (int i = 0; i < connectionLines.Length; i++)
		{
			connectionLines[i].RefreshState();
		}
	}

	public bool IncludesArea(BaseMapAreaMetadata targetArea)
	{
		if (targetArea == null)
		{
			return false;
		}
		MapArea3D[] areas = Areas;
		foreach (MapArea3D mapArea3D in areas)
		{
			if (targetArea == mapArea3D.MapAreaMetadata)
			{
				return true;
			}
		}
		return false;
	}

	public bool ShouldFloorBeVisible()
	{
		MapArea3D[] areas = Areas;
		for (int i = 0; i < areas.Length; i++)
		{
			if (areas[i].IsKnown())
			{
				return true;
			}
		}
		return false;
	}

	public bool ContainsLore(LoreEntry lore)
	{
		return false;
	}
}
