using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.Movement)]
public class DisableMovementGravity : FsmStateAction
{
	public FsmOwnerDefault m_gameObject;

	public override void OnEnter()
	{
		Finish();
	}

	public override void OnExit()
	{
	}
}
