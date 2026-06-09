using HutongGames.PlayMaker;

[ActionCategory("Utilities")]
public class GetIntFromProgressionVariable : FsmStateAction
{
	public ProgressionVariableInt m_progressionVariable;

	[UIHint(UIHint.Variable)]
	public FsmInt m_variable;

	public override void OnEnter()
	{
		m_variable.Value = m_progressionVariable.Value;
		Finish();
	}
}
