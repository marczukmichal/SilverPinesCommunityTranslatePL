using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.UI)]
public class CloseInGameMenu : FsmStateAction
{
	public override void OnEnter()
	{
		GlobalReferences.Instance.GameMenuState.ClearInMenu(GameMenuState.GameMenu.InGameMenu);
		Finish();
	}
}
