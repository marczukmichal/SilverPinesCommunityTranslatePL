using System;
using UnityEngine;

public class AICollisionTag : MonoBehaviour
{
	[Flags]
	public enum AICollisionTagFlag
	{
		None = 0,
		DetectionBarrier = 1
	}

	[SerializeField]
	private AICollisionTagFlag m_flags;

	public AICollisionTagFlag Flags => m_flags;

	public bool IsFlagSet(AICollisionTagFlag flag)
	{
		return m_flags.HasFlag(flag);
	}
}
