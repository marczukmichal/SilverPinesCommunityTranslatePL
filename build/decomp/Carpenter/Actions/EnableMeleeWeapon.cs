using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("Combat")]
public class EnableMeleeWeapon : FsmStateAction
{
	protected CharacterEquipment m_equipment;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_equipment = base.Owner.GetComponent<CharacterEquipment>();
		}
	}

	public override void OnEnter()
	{
		m_equipment.EnableMeleeWeapon();
	}

	public override void OnExit()
	{
		m_equipment.DisableActiveWeapon();
	}
}
