using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory(ActionCategory.Movement)]
public class LedgeVaultWillFall : FsmStateAction
{
	public FsmEvent m_fall = new FsmEvent("Yes");

	public FsmEvent m_stand = new FsmEvent("No");

	public float m_fallThreshold = 2f;

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
		if (CharacterLedgeGrab.LEDGE_DEBUG)
		{
			Debug.Log("LedgeVaultWillFall - fall distance: " + m_ledgeGrab.ActiveLedge.m_exitHeightOffsetDown);
		}
		if (m_ledgeGrab.ActiveLedge.m_exitHeightOffsetDown > m_fallThreshold)
		{
			base.Fsm.Event(m_fall);
		}
		else
		{
			base.Fsm.Event(m_stand);
		}
		Finish();
	}
}
