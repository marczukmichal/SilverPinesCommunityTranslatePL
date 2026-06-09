using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("Utilities")]
public class StorySkipDebug : FsmStateAction
{
	[SerializeField]
	public FsmEvent m_event;

	public override void OnEnter()
	{
		if (GameDebugCommands.SKIP_STORY)
		{
			base.Fsm.Event(m_event);
		}
		Finish();
	}
}
