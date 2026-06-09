using HutongGames.PlayMaker;
using UnityEngine.SceneManagement;

namespace Actions;

[ActionCategory("Utilities")]
public class ReturnToMainmenu : FsmStateAction
{
	public override void OnEnter()
	{
		GlobalReferences.Instance.Anchors.Generic.ActiveLevelMetadata.Set(null);
		GlobalReferences.Instance.GameState.ClearStateFlag(GameState.GameStateFlag.GameActive);
		SceneManager.LoadScene("Assets/Scenes/Utility/Startup.unity");
		Finish();
	}
}
