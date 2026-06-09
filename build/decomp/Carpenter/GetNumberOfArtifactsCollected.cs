using HutongGames.PlayMaker;

[ActionCategory("Inventory")]
public class GetNumberOfArtifactsCollected : FsmStateAction
{
	[UIHint(UIHint.Variable)]
	public FsmInt m_variable;

	public override void OnEnter()
	{
		m_variable.Value = GlobalReferences.Instance.MainInventory.ArtifactItems.Count;
		Finish();
	}
}
