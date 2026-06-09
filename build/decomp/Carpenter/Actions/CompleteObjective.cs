using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("Objective")]
public class CompleteObjective : FsmStateAction
{
	[SerializeField]
	public MapObjectiveState m_objective;

	public override void OnEnter()
	{
		GlobalReferences.Instance.ActiveObjective.ClearObjective(m_objective);
		Finish();
	}
}
