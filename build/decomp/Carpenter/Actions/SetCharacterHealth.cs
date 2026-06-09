using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.Character)]
public class SetCharacterHealth : FsmStateAction
{
	public FsmInt m_healthAmount;

	protected CharacterHealth m_characterHealth;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_characterHealth = base.Owner.GetComponent<CharacterHealth>();
		}
	}

	public override void OnEnter()
	{
		m_characterHealth.Health = m_healthAmount.Value;
	}
}
