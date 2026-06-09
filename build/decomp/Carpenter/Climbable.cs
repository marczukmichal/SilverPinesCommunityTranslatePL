using UnityEngine;

public class Climbable : MonoBehaviour
{
	public enum ClimbableMode
	{
		Normal,
		ClimbThrough
	}

	[SerializeField]
	private ClimbableMode m_climbableMode;

	public ClimbableMode Mode => m_climbableMode;
}
