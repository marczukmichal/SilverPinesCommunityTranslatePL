using HutongGames.PlayMaker;
using UnityEngine.AddressableAssets;

namespace Actions;

[ActionCategory("Utilities")]
public class SwitchToDemoCallToActionScene : FsmStateAction
{
	public override void OnEnter()
	{
		GlobalReferences.Instance.Anchors.Generic.ActiveLevelMetadata.Set(null);
		GlobalReferences.Instance.GameState.ClearStateFlag(GameState.GameStateFlag.GameActive);
		Addressables.LoadSceneAsync("DemoCallToAction");
		Finish();
	}
}
