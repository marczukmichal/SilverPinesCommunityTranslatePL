using UnityEngine;

public class MapGenericObjective : BaseMapObjective
{
	[SerializeField]
	private MapObjectiveState m_objectiveState;

	private Map3D m_map3d;

	public override void UpdateForObjectiveState(MapObjectiveState objectiveState, MapObjectiveState animate)
	{
		if (objectiveState.HasFlag(m_objectiveState))
		{
			ShowStatic();
		}
		else
		{
			Hide();
		}
	}

	public void ShowStatic()
	{
		base.gameObject.SetActive(value: true);
		RefreshFloorState();
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}
}
