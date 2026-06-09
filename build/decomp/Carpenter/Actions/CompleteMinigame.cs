using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("Minigames")]
public class CompleteMinigame : FsmStateAction
{
	[SerializeField]
	public MinigameScene m_minigameScene;

	public override void OnEnter()
	{
		m_minigameScene.SetMinigameCompleted();
		Finish();
	}
}
