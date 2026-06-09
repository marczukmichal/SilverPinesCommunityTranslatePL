using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("Combat")]
public class DisableDamageCollidersOnExit : FsmStateAction
{
	protected AnimationEventsHelper m_animEventsHelper;

	protected CharacterEquipment m_characterMelee;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_animEventsHelper = base.Owner.GetComponent<AnimationEventsHelper>();
			m_characterMelee = base.Owner.GetComponent<CharacterEquipment>();
		}
	}

	public override void OnExit()
	{
		if (m_animEventsHelper != null)
		{
			m_animEventsHelper.DisableAllDamageColliders();
		}
		if (m_characterMelee != null)
		{
			m_characterMelee.DisableWeaponDamageColliders();
		}
	}
}
