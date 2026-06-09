using UnityEngine;

public class MapAreaConnectionLineEndPoint : MonoBehaviour, IMapRaycastTooltipElement
{
	[SerializeField]
	private MapAreaConnectionLine m_parent;

	[SerializeField]
	private MapArea3D m_associatedArea;

	[SerializeField]
	private SpriteRenderer m_spriteRenderer;

	public SpriteRenderer SpriteRenderer => m_spriteRenderer;

	public bool TooltipActive
	{
		get
		{
			if (m_parent.TooltipActive)
			{
				if (m_associatedArea != null && !m_associatedArea.ShouldShowInFogOfWar())
				{
					return false;
				}
				return true;
			}
			return false;
		}
	}

	public int TooltipPriority => m_parent.TooltipPriority;

	public string TooltipString => m_parent.TooltipString;

	public Vector3 TooltipWorldPosition => base.transform.position;
}
