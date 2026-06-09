using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("Special")]
public class ShowQuickMap : FsmStateAction
{
	public override void OnEnter()
	{
		base.OnEnter();
		GlobalReferences.Instance.EventChannels.Map.ShowQuickMap.Raise(value: true);
		Finish();
	}

	public override void OnExit()
	{
		base.OnExit();
		GlobalReferences.Instance.EventChannels.Map.ShowQuickMap.Raise(value: false);
	}
}
