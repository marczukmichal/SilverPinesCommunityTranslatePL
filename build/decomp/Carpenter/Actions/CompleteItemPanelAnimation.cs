using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.UI)]
public class CompleteItemPanelAnimation : FsmStateAction
{
	public override void OnEnter()
	{
		base.Owner.GetComponentInParent<InventoryItemPickupPanel>().DoneAnimation();
		Finish();
	}
}
