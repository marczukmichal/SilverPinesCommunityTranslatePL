using HutongGames.PlayMaker;

[ActionCategory("Cutscene")]
public class ForcePlayerInteract : FsmStateAction
{
	public BaseInteractable m_interactable;

	public override void OnEnter()
	{
		GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item.GetComponent<CharacterInteractor>().DoForceInteract(m_interactable);
		Finish();
	}
}
