using System.Collections.Generic;
using UnityEngine;

public static class RaycastUtils
{
	public static RaycastHit2D[] SharedRaycastArray;

	public static List<RaycastHit2D> SharedRaycastList;

	public static ContactFilter2D ContactFilterDefault;

	public static ContactFilter2D ContactFilterDamageable;

	public static ContactFilter2D ContactFilterMelee;

	static RaycastUtils()
	{
		SharedRaycastArray = new RaycastHit2D[32];
		SharedRaycastList = new List<RaycastHit2D>();
		ContactFilterDefault = default(ContactFilter2D);
		ContactFilterDamageable = default(ContactFilter2D);
		ContactFilterMelee = default(ContactFilter2D);
		ContactFilterDamageable.SetLayerMask(GameLayers.DamageablesMask);
		ContactFilterMelee.SetLayerMask(GameLayers.MeleeMask);
	}
}
