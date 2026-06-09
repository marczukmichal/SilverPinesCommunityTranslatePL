using UnityEngine;

public abstract class BaseMapObjective : MonoBehaviour
{
	public abstract void UpdateForObjectiveState(MapObjectiveState objectiveState, MapObjectiveState animate);

	private void OnEnable()
	{
		GlobalReferences.Instance.EventChannels.Map.ActiveMapFloorChanged.Register(OnFloorChanged);
		GlobalReferences.Instance.EventChannels.Map.InitialFloorMapApplied.Register(OnFloorChanged);
		RefreshFloorState();
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.Map.ActiveMapFloorChanged.Unregister(OnFloorChanged);
		GlobalReferences.Instance.EventChannels.Map.InitialFloorMapApplied.Unregister(OnFloorChanged);
	}

	protected void RefreshFloorState()
	{
		Map3D componentInParent = GetComponentInParent<Map3D>();
		if (componentInParent != null)
		{
			OnFloorChanged(componentInParent.ActiveFloor);
		}
	}

	protected virtual void OnFloorChanged(Map3DFloor floor)
	{
		MapObjectiveShowConditional[] componentsInChildren = GetComponentsInChildren<MapObjectiveShowConditional>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].ShowForFloor(floor);
		}
	}
}
