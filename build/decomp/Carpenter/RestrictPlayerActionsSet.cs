using UnityEngine;

[CreateAssetMenu(menuName = "Sets/Restrict Player Actions")]
public class RestrictPlayerActionsSet : RuntimeSet<RestrictPlayerActions>
{
	public RestrictPlayerActions.Mode GetMode()
	{
		foreach (RestrictPlayerActions item in m_items)
		{
			if (item.ActiveMode != 0)
			{
				return item.ActiveMode;
			}
		}
		return RestrictPlayerActions.Mode.None;
	}
}
