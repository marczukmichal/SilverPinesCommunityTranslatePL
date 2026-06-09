using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory(ActionCategory.Movement)]
public class CheckLedgeLevelTransition : FsmStateAction
{
	public FsmEvent m_transitionEvent;

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
		Vector3 detectedLedgePosition = m_ledgeGrab.ActiveLedge.m_detectedLedgePosition;
		foreach (LedgeGrabLevelTransition item in GlobalReferences.Instance.Sets.Generic.LedgeGrabLevelTransitionSet.Items)
		{
			if (item.IncludesLedgeGrabLocation(detectedLedgePosition))
			{
				m_ledgeGrab.SavedLedgeGrabLevelTransition = item;
				base.Fsm.Event(m_transitionEvent);
			}
		}
		Finish();
	}
}
