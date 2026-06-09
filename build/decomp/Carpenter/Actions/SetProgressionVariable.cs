using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("Progression")]
public class SetProgressionVariable : FsmStateAction
{
	[SerializeField]
	public BoolVariable m_progressionVariable;

	public bool m_value = true;

	public override void OnEnter()
	{
		if (m_progressionVariable != null)
		{
			m_progressionVariable.Value = m_value;
		}
		Finish();
	}
}
