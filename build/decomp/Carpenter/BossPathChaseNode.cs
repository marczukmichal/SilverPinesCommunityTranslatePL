using UnityEngine;
using UnityEngine.Events;

public class BossPathChaseNode : MonoBehaviour
{
	public enum ChaseNodeType
	{
		Normal,
		Climb,
		StartFightStage1,
		StartFightStage2
	}

	[SerializeField]
	private ChaseNodeType m_type;

	[SerializeField]
	private Vector2 m_climbOffset;

	[SerializeField]
	private UnityEvent m_onNodeReached;

	public ChaseNodeType Type => m_type;

	public Vector3 Position => base.transform.position;

	public void ReachedNodeTrigger()
	{
		m_onNodeReached.Invoke();
	}

	public Vector2 GetClimbPosition()
	{
		return base.transform.TransformPoint(m_climbOffset);
	}

	private void OnDrawGizmos()
	{
		if (m_type == ChaseNodeType.Climb)
		{
			Gizmos.color = Color.magenta;
			GizmoExtensions.DrawToFromArrow(base.transform.position, GetClimbPosition());
		}
	}
}
