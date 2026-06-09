using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("Combat")]
public class SetStabilityMultiplier : FsmStateAction
{
	public float m_multiplier;

	public override void OnEnter()
	{
		Debug.LogError("SetStabilityMultiplier is Deprecated");
	}
}
