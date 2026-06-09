using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.UI)]
public class ShowAimingUI : FsmStateAction
{
	public override void OnEnter()
	{
		GlobalReferences.Instance.EventChannels.Gameplay.ShowAimingUI.Raise(value: true);
		Finish();
	}

	public override void OnExit()
	{
		base.OnExit();
		GlobalReferences.Instance.EventChannels.Gameplay.ShowAimingUI.Raise(value: false);
	}
}
