using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.UI)]
public class WorldMapScript : FsmStateAction
{
	public string m_mapScript;

	public override void OnEnter()
	{
		GlobalReferences.Instance.Variables.Generic.MapScriptTrigger.Value = m_mapScript;
		GlobalReferences.Instance.EventChannels.InGameMenu.ShowMapMenuTab.Raise();
		Finish();
	}
}
