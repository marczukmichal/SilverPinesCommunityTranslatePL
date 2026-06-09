using System.Collections.Generic;
using UnityEngine;

public abstract class BaseCollisionGenerator : MonoBehaviour
{
	[SerializeField]
	[HideInInspector]
	protected List<Vector2> m_validGroundPositions = new List<Vector2>();

	public List<Vector2> ValidGroundPositions => m_validGroundPositions;
}
