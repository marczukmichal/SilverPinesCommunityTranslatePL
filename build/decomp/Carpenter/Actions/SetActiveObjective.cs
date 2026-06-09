using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("Objective")]
public class SetActiveObjective : FsmStateAction
{
	[SerializeField]
	public MapObjectiveState m_objective;

	public override void OnEnter()
	{
		GlobalReferences.Instance.ActiveObjective.SetObjective(m_objective);
		Finish();
	}
}
