using UnityEngine;

public interface IMapRaycastTooltipElement
{
	bool TooltipActive { get; }

	int TooltipPriority { get; }

	string TooltipString { get; }

	Vector3 TooltipWorldPosition { get; }
}
