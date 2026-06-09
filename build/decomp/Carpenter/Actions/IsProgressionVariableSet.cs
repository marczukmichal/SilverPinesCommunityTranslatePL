using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("Progression")]
public class IsProgressionVariableSet : FsmStateAction
{
	[SerializeField]
	public BoolVariable m_progressionVariable;

	[HutongGames.PlayMaker.Tooltip("Event to trigger if the variable is set")]
	public FsmEvent m_set;

	[HutongGames.PlayMaker.Tooltip("Event to trigger if the variable is not set")]
	public FsmEvent m_notSet;

	public override void OnEnter()
	{
		if (m_progressionVariable.Value)
		{
			base.Fsm.Event(m_set);
		}
		else
		{
			base.Fsm.Event(m_notSet);
		}
		Finish();
	}
}
