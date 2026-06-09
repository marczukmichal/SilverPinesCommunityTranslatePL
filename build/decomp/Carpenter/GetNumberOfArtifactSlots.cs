using HutongGames.PlayMaker;

[ActionCategory("Inventory")]
public class GetNumberOfArtifactSlots : FsmStateAction
{
	[UIHint(UIHint.Variable)]
	public FsmInt m_variable;

	public override void OnEnter()
	{
		m_variable.Value = GlobalReferences.Instance.MainInventory.MaxEquippedArtifactsCount;
		Finish();
	}
}
