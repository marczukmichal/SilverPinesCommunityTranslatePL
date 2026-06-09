using HutongGames.PlayMaker;

[ActionCategory(ActionCategory.Character)]
public class GetCharacterRecoveryPosition : FsmStateAction
{
	[UIHint(UIHint.Variable)]
	public FsmVector3 m_variable;

	private CharacterRecoveryPosition m_characterRecoveryPosition;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_characterRecoveryPosition = base.Owner.GetComponent<CharacterRecoveryPosition>();
		}
	}

	public override void OnEnter()
	{
		if (!m_variable.IsNone)
		{
			m_variable.Value = m_characterRecoveryPosition.GetRecoveryPosition();
		}
		Finish();
	}
}
