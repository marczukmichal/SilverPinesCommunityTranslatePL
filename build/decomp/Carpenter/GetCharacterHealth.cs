using HutongGames.PlayMaker;

[ActionCategory(ActionCategory.Character)]
public class GetCharacterHealth : FsmStateAction
{
	public FsmOwnerDefault m_gameObject;

	[UIHint(UIHint.Variable)]
	public FsmInt m_variable;

	[UIHint(UIHint.Variable)]
	public FsmFloat m_percentage;

	public bool m_everyFrame;

	public override void OnEnter()
	{
		UpdateValues();
		if (!m_everyFrame)
		{
			Finish();
		}
	}

	public override void OnUpdate()
	{
		if (m_everyFrame)
		{
			UpdateValues();
		}
	}

	private void UpdateValues()
	{
		CharacterHealth component = base.Fsm.GetOwnerDefaultTarget(m_gameObject).GetComponent<CharacterHealth>();
		if (!m_percentage.IsNone)
		{
			m_percentage.Value = component.HealthPercentage;
		}
		if (!m_variable.IsNone)
		{
			m_variable.Value = component.Health;
		}
	}
}
