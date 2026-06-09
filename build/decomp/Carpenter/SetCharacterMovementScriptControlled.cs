using HutongGames.PlayMaker;
using UnityEngine;

[ActionCategory(ActionCategory.Character)]
public class SetCharacterMovementScriptControlled : FsmStateAction
{
	public FsmOwnerDefault m_gameObject;

	public bool m_enable = true;

	public bool m_sendFSMEvent = true;

	public override void OnEnter()
	{
		if (m_enable)
		{
			EnableScriptControlled();
		}
		else
		{
			DisableScriptControlled();
		}
		Finish();
	}

	private void EnableScriptControlled()
	{
		GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(m_gameObject);
		ownerDefaultTarget.GetComponent<CharacterMovement>().enabled = false;
		if (m_sendFSMEvent)
		{
			ownerDefaultTarget.GetComponent<PlayMakerFSM>().SendEvent("ScriptControlled/Enable");
		}
	}

	private void DisableScriptControlled()
	{
		GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(m_gameObject);
		ownerDefaultTarget.GetComponent<CharacterMovement>().enabled = true;
		if (m_sendFSMEvent)
		{
			ownerDefaultTarget.GetComponent<PlayMakerFSM>().SendEvent("Reset");
		}
	}
}
