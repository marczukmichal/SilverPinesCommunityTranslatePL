using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.UI)]
public class FadeScreenAction : FsmStateAction
{
	public FadeType m_fadeType;

	public override void OnEnter()
	{
		GlobalReferences.Instance.EventChannels.Generic.ScreenFadeOfType.Raise(m_fadeType);
		Finish();
	}
}
