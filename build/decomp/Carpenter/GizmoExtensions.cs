using UnityEngine;

public static class GizmoExtensions
{
	public static void DrawArrow(Vector3 position, Vector3 direction, float length = 1f, float flangeWidth = 1f, float flangeDepth = 1f)
	{
		direction.Normalize();
		Vector3 vector = position + direction * length;
		Vector3 vector2 = Vector3.Cross(Quaternion.Euler(0f, 0f, 90f) * direction, direction);
		Gizmos.DrawLine(position, vector);
		Gizmos.DrawLine(vector, vector + vector2 * flangeWidth - direction * flangeDepth);
		Gizmos.DrawLine(vector, vector - vector2 * flangeWidth - direction * flangeDepth);
	}

	public static void DrawToFromArrow(Vector3 positionStart, Vector3 positionEnd, float flangeWidth = 1f, float flangeDepth = 1f)
	{
		Vector3 vector = positionEnd - positionStart;
		vector.Normalize();
		Vector3 vector2 = Vector3.Cross(Quaternion.Euler(0f, 0f, 90f) * vector, vector);
		Gizmos.DrawLine(positionStart, positionEnd);
		Gizmos.DrawLine(positionEnd, positionEnd + vector2 * flangeWidth - vector * flangeDepth);
		Gizmos.DrawLine(positionEnd, positionEnd - vector2 * flangeWidth - vector * flangeDepth);
	}
}
