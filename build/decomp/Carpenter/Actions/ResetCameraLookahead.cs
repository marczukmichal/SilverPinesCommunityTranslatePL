using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.Camera)]
public class ResetCameraLookahead : FsmStateAction
{
	public override void OnEnter()
	{
		GlobalReferences.Instance.EventChannels.Camera.ResetCameraLookahead.Raise();
		Finish();
	}
}
