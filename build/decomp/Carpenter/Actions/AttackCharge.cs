using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("Combat")]
public class AttackCharge : FsmStateAction
{
	public CharacterEquipment m_equipment;

	public CharacterStamina m_stamina;

	[HutongGames.PlayMaker.Tooltip("The starting damage scalar when starting the charge")]
	public float m_startScale;

	[HutongGames.PlayMaker.Tooltip("The end damage scalar once full charge time has expired")]
	public float m_endScale;

	[HutongGames.PlayMaker.Tooltip("How long it takes to get a full charge")]
	public float m_fullChargeTime;

	[HutongGames.PlayMaker.Tooltip("Event to trigger on stamina depleted")]
	public FsmEvent m_onStaminaDepleted;

	private float m_timer;

	public float m_staminaReductionRate;

	public override void Awake()
	{
		base.Awake();
		if (base.Owner != null)
		{
			m_equipment = base.Owner.GetComponent<CharacterEquipment>();
			m_stamina = base.Owner.GetComponent<CharacterStamina>();
		}
	}

	public override void OnEnter()
	{
		m_equipment.SetCurrentMeleeWeaponDamageScale(m_startScale, 1f);
		m_timer = 0f;
	}

	public override void OnUpdate()
	{
		m_timer += Time.deltaTime;
		m_equipment.SetCurrentMeleeWeaponDamageScale(Mathf.Lerp(m_startScale, m_endScale, m_timer / m_fullChargeTime), 1f);
		m_stamina.ConsumeStamina(m_staminaReductionRate * Time.deltaTime);
		if (m_stamina.AvailableStamina <= 0f)
		{
			base.Fsm.Event(m_onStaminaDepleted);
		}
	}
}
