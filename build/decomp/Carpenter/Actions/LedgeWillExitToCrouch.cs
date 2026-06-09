using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory(ActionCategory.Movement)]
public class LedgeWillExitToCrouch : FsmStateAction
{
	[SerializeField]
	public FsmEvent m_crouch = new FsmEvent("Yes");

	[SerializeField]
	public FsmEvent m_stand = new FsmEvent("No");

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
		if (m_ledgeGrab.ActiveLedge.m_exitHeightOffsetUp < 1.8f)
		{
			base.Fsm.Event(m_crouch);
		}
		else
		{
			base.Fsm.Event(m_stand);
		}
		Finish();
	}
}
