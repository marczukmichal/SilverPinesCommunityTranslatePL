using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("Dialogue")]
public class WaitForDialogue : FsmStateAction
{
	[SerializeField]
	public BoolGameEventChannel m_onDialogueStateChangedEventChannel;

	[HutongGames.PlayMaker.Tooltip("Event to trigger on interact stopped")]
	public FsmEvent m_onInteractDialogueEnd;

	public override void OnEnter()
	{
		m_onDialogueStateChangedEventChannel.Register(OnDialogueStateChanged);
	}

	public override void OnExit()
	{
		m_onDialogueStateChangedEventChannel.Unregister(OnDialogueStateChanged);
	}

	private void OnDialogueStateChanged(bool newState)
	{
		if (!newState)
		{
			base.Fsm.Event(m_onInteractDialogueEnd);
		}
	}
}
