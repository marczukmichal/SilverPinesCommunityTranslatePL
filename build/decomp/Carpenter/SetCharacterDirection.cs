using HutongGames.PlayMaker;

[ActionCategory("Cutscene")]
public class SetCharacterDirection : FsmStateAction
{
	[UIHint(UIHint.Variable)]
	public FsmGameObject m_variable;

	public CharacterDirection.Facing m_facingDirection;

	public override void OnEnter()
	{
		if (m_variable.Value != null)
		{
			CharacterDirection component = m_variable.Value.GetComponent<CharacterDirection>();
			if (component != null)
			{
				component.CurrentDirection = m_facingDirection;
			}
		}
		Finish();
	}
}
