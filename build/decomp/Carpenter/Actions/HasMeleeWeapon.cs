using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("Combat")]
public class HasMeleeWeapon : FsmStateAction
{
	[Tooltip("Event to trigger if has a melee weapon equipped")]
	public FsmEvent m_hasMelee;

	[Tooltip("Event to trigger if has no melee weapon equipped")]
	public FsmEvent m_hasNoMelee;

	private CharacterEquipment m_characterMelee;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_characterMelee = base.Owner.GetComponent<CharacterEquipment>();
		}
	}

	public override void OnEnter()
	{
		if (m_characterMelee != null && m_characterMelee.HasMeleeWeapon)
		{
			base.Fsm.Event(m_hasMelee);
		}
		else
		{
			base.Fsm.Event(m_hasNoMelee);
		}
		Finish();
	}
}
