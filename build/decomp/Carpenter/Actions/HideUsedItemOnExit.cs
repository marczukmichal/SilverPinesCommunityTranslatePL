using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("Inventory")]
public class HideUsedItemOnExit : FsmStateAction
{
	private CharacterEquipment m_equipment;

	public override void Awake()
	{
		base.Awake();
		if (base.Owner != null)
		{
			m_equipment = base.Owner.GetComponent<CharacterEquipment>();
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		m_equipment.HideUsingItem();
	}
}
