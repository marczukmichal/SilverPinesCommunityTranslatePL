using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.Character)]
public class Revive : FsmStateAction
{
	private CharacterNecromancy m_necromancy;

	private CharacterHealth m_health;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_necromancy = base.Owner.GetComponent<CharacterNecromancy>();
			m_health = base.Owner.GetComponent<CharacterHealth>();
		}
	}

	public override void OnEnter()
	{
		if (m_necromancy != null)
		{
			m_necromancy.OnRevive();
		}
		if (m_health != null)
		{
			m_health.ResetHealth();
		}
		Finish();
	}
}
