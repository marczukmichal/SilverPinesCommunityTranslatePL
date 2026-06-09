using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("Combat")]
public class CanGetUp : FsmStateAction
{
	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			Debug.LogError("CanGetUp is deprecated in " + base.State.Name + " for " + base.Owner.name);
		}
	}
}
