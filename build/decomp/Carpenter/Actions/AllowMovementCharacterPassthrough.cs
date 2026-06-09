using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory(ActionCategory.Movement)]
public class AllowMovementCharacterPassthrough : FsmStateAction
{
	public FsmOwnerDefault m_gameObject;

	public bool m_resetOnExit = true;

	public override void OnEnter()
	{
		base.Fsm.GetOwnerDefaultTarget(m_gameObject).GetComponent<CharacterMovement>().AllowCharacterPassthrough = true;
		Finish();
	}

	public override void OnExit()
	{
		GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(m_gameObject);
		if (m_resetOnExit)
		{
			ownerDefaultTarget.GetComponent<CharacterMovement>().AllowCharacterPassthrough = false;
		}
	}
}
