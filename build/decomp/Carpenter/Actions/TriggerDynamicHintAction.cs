using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.UI)]
public class TriggerDynamicHintAction : FsmStateAction
{
	public FsmEnum m_hint;

	public override void OnEnter()
	{
		GlobalReferences.Instance.EventChannels.Hints.DynamicHint.Raise((DynamicHintEvent)(object)m_hint.Value);
		Finish();
	}
}
