using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("Utilities")]
public class SetGameSummaryEnding : FsmStateAction
{
	[SerializeField]
	public GameEnding m_ending;

	public override void OnEnter()
	{
		GameCompletionSummary.SetGameEnding(m_ending);
		Finish();
	}
}
