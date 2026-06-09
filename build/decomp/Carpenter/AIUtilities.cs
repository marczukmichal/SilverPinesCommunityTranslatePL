using UnityEngine;

public static class AIUtilities
{
	public static bool IsInFrontOfWall(CapsuleCollider2D collider, Vector2 forwardDir, int layerMask, float checkDistance = 1f)
	{
		RaycastHit2D[] array = Physics2D.CapsuleCastAll(collider.bounds.center, collider.size, collider.direction, 0f, forwardDir, checkDistance, layerMask);
		for (int i = 0; i < array.Length; i++)
		{
			RaycastHit2D raycastHit2D = array[i];
			if (!(raycastHit2D.collider.GetComponent<PlatformEffector2D>() != null) && Vector2.Dot(raycastHit2D.normal, forwardDir) < -0.5f)
			{
				return true;
			}
		}
		return false;
	}
}
