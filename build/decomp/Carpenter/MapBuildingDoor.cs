using System.Collections.Generic;
using UnityEngine;

public class MapBuildingDoor : MonoBehaviour
{
	private enum DoorState
	{
		None,
		Unknown,
		Visited,
		Active
	}

	[SerializeField]
	private Vector3 m_offset;

	[SerializeField]
	private MeshRenderer m_meshRenderer;

	[SerializeField]
	private List<LevelMetadata> m_levels;

	private DoorState m_activeDoorState;

	public Vector3 WorldPosition => base.transform.position + base.transform.TransformDirection(m_offset);

	private bool HasVisited()
	{
		return false;
	}

	private bool IsActive()
	{
		LevelMetadata item = GlobalReferences.Instance.Anchors.Generic.ActiveLevelMetadata.Item;
		foreach (LevelMetadata level in m_levels)
		{
			if (level == item)
			{
				return true;
			}
		}
		return false;
	}

	public bool TracksLevel(LevelMetadata level)
	{
		return m_levels.Contains(level);
	}

	public void Refresh()
	{
		if (IsActive())
		{
			SetDoorState(DoorState.Active);
		}
		else if (HasVisited())
		{
			SetDoorState(DoorState.Visited);
		}
		else
		{
			SetDoorState(DoorState.Unknown);
		}
	}

	private void SetDoorState(DoorState state)
	{
		if (m_activeDoorState != state)
		{
			m_activeDoorState = state;
			MapVisualsSettings.MapAreaVisuals visualsSettings = GlobalReferences.Instance.Anchors.Map.ActiveMap3DView.Item.VisualsSettings;
			List<Material> list = new List<Material>(m_meshRenderer.sharedMaterials);
			switch (state)
			{
			case DoorState.Unknown:
				list[1] = visualsSettings.DoorStandardMaterial;
				break;
			case DoorState.Visited:
				list[1] = visualsSettings.DoorVisitedMaterial;
				break;
			case DoorState.Active:
				list[1] = visualsSettings.DoorActiveMaterial;
				break;
			}
			m_meshRenderer.SetSharedMaterials(list);
		}
	}
}
