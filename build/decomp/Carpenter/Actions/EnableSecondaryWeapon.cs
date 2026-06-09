using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("Combat")]
public class EnableSecondaryWeapon : FsmStateAction
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
		m_equipment.EnableSecondaryWeapon();
	}

	public override void OnExit()
	{
		m_equipment.DisableActiveWeapon();
	}
}
