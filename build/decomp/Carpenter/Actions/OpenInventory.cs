using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.Character)]
public class OpenInventory : FsmStateAction
{
	public override void OnEnter()
	{
		GlobalReferences.Instance.EventChannels.InGameMenu.ShowInventoryMenuTab.Raise();
	}
}
