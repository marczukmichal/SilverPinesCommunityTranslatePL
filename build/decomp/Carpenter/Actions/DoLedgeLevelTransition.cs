using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory(ActionCategory.Movement)]
public class DoLedgeLevelTransition : FsmStateAction
{
	private CharacterLedgeGrab m_ledgeGrab;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_ledgeGrab = base.Owner.GetComponent<CharacterLedgeGrab>();
		}
	}

	public override void OnEnter()
	{
		if (m_ledgeGrab.SavedLedgeGrabLevelTransition != null)
		{
			m_ledgeGrab.SavedLedgeGrabLevelTransition.Trigger();
		}
		else
		{
			Debug.LogError("DoLedgeLevelTransition called but there was no ledge grab level transition saved!");
		}
		Finish();
	}
}
