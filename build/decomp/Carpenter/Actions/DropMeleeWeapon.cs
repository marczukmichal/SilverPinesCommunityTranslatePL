using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("Combat")]
public class DropMeleeWeapon : FsmStateAction
{
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
			m_characterMelee.DropMeleeWeapon();
		}
		Finish();
	}
}
