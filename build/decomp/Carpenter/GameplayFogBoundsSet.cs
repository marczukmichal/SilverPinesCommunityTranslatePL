using UnityEngine;

[CreateAssetMenu(menuName = "Sets/Gameplay Fog Bounds")]
public class GameplayFogBoundsSet : RuntimeSet<GameplayFogBounds>
{
	public bool IsInFog(Bounds bounds)
	{
		bool flag = false;
		bool flag2 = false;
		foreach (GameplayFogBounds item in m_items)
		{
			if (item.Bounds.Intersects(bounds))
			{
				if (item.Type == GameplayFogBounds.FogType.Fog)
				{
					flag = true;
				}
				else if (item.Type == GameplayFogBounds.FogType.Void)
				{
					flag2 = true;
				}
			}
		}
		if (flag)
		{
			return !flag2;
		}
		return false;
	}
}
