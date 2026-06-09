using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("Special")]
public class FindDigSpot : FsmStateAction
{
	public FsmEvent m_found;

	public FsmEvent m_notFound;

	[UIHint(UIHint.Variable)]
	public FsmGameObject m_foundDigSpot;

	public override void OnEnter()
	{
		base.OnEnter();
		DigSpot digSpot = DigSpot.FindDigSpot(base.Owner.transform.position);
		if (m_foundDigSpot != null)
		{
			GameObject value = null;
			if (digSpot != null)
			{
				value = digSpot.gameObject;
			}
			m_foundDigSpot.Value = value;
		}
		if (digSpot != null)
		{
			base.Fsm.Event(m_found);
		}
		else
		{
			base.Fsm.Event(m_notFound);
		}
		Finish();
	}
}
