using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory(ActionCategory.Input)]
public class InputWaitForAnyPlayerInput : FsmStateAction
{
	[HutongGames.PlayMaker.Tooltip("Where to send the event")]
	public FsmEventTarget m_eventTarget;

	[RequiredField]
	[HutongGames.PlayMaker.Tooltip("Event to send when any Key or Mouse Button is pressed.")]
	public FsmEvent m_sendEvent;

	public override void Reset()
	{
		m_eventTarget = null;
		m_sendEvent = null;
	}

	public override void OnUpdate()
	{
		if (GameInputManager.GameInputActions != null && (GameInputManager.GameInputActions.Player.Move.ReadValue<Vector2>().magnitude > 0.5f || GameInputManager.GameInputActions.Player.Interact.WasPerformedThisFrame() || GameInputManager.GameInputActions.Player.Fire.WasPerformedThisFrame() || GameInputManager.GameInputActions.Player.Jump.WasPerformedThisFrame() || GameInputManager.GameInputActions.Player.Dodge.WasPerformedThisFrame() || GameInputManager.GameInputActions.Player.AimMode.WasPerformedThisFrame() || GameInputManager.GameInputActions.Player.Crouch.WasPerformedThisFrame() || GameInputManager.GameInputActions.Player.Reload.WasPerformedThisFrame()))
		{
			base.Fsm.Event(m_eventTarget, m_sendEvent);
		}
	}
}
