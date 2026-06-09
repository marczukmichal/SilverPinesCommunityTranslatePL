using UnityEngine;

public static class ProjectileMath
{
	public static Vector2 CalculateJumpVelocity(float horizontalDistance, float heightOffset, float gravity, float linearDrag)
	{
		float num = Mathf.Sqrt(2f * heightOffset / gravity);
		float y = gravity * num * (1f + linearDrag * num);
		return new Vector2(horizontalDistance / (num * 2f) / (1f - linearDrag * num), y);
	}
}
