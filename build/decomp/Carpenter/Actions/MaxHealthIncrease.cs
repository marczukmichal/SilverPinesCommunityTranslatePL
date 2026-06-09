using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("Special")]
public class MaxHealthIncrease : FsmStateAction
{
	public int m_healthIncreaseAmount;

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
		base.OnEnter();
		m_characterHealth.IncreaseCharacterMaxHealth(m_healthIncreaseAmount);
		Finish();
	}
}
