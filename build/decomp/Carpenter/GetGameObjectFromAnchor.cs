using HutongGames.PlayMaker;

[ActionCategory("Anchors")]
public class GetGameObjectFromAnchor : FsmStateAction
{
	public GameObjectAnchor m_anchor;

	[UIHint(UIHint.Variable)]
	public FsmGameObject m_variable;

	public override void OnEnter()
	{
		m_variable.Value = m_anchor.Item;
		Finish();
	}
}
