using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.UI)]
public class ShowPauseHint : FsmStateAction
{
	public PauseHintInfo m_pauseHint;

	public override void OnEnter()
	{
		GlobalReferences.Instance.EventChannels.Hints.ShowPauseHintInfo.Raise(m_pauseHint);
		Finish();
	}
}
