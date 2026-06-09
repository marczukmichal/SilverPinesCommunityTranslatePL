using UnityEngine;

[CreateAssetMenu(menuName = "Sets/Visibility Light Sources")]
public class VisibilityLightSourceSet : RuntimeSet<VisibilityLightSource>
{
	public bool IsLit(Bounds bounds)
	{
		foreach (VisibilityLightSource item in m_items)
		{
			if (item.BoundIsLit(bounds))
			{
				return true;
			}
		}
		return false;
	}
}
