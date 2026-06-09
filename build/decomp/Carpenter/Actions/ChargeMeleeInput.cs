using HutongGames.PlayMaker;
using PowerTools;
using UnityEngine;

namespace Actions;

[ActionCategory("Combat")]
public class ChargeMeleeInput : FsmStateAction
{
	public float m_earlyOutTime;

	public float m_earlyOutMaxTime;

	public FsmEvent m_earlyReleaseEvent;

	public FsmEvent m_normalAttackEvent;

	public FsmEvent m_exhaustedEvent;

	private BaseCharacterInput m_input;

	private SpriteAnim m_spriteAnim;

	private CharacterStamina m_characterStamina;

	private CharacterEquipment m_characterEquipment;

	private CharacterInventory m_characterInventory;

	private bool m_inputHeld;

	private float m_timer;

	private MeleeChargeState m_chargeStateData;

	private int m_currentChargeLevel = -1;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_input = base.Owner.GetComponent<BaseCharacterInput>();
			m_spriteAnim = base.Owner.GetComponent<SpriteAnim>();
			m_characterStamina = base.Owner.GetComponent<CharacterStamina>();
			m_characterEquipment = base.Owner.GetComponent<CharacterEquipment>();
			m_characterInventory = base.Owner.GetComponent<CharacterInventory>();
			m_chargeStateData = new MeleeChargeState();
		}
	}

	public override void OnEnter()
	{
		m_inputHeld = true;
		m_timer = 0f;
		m_currentChargeLevel = -1;
		MeleeWeaponItemInstance equippedMeleeWeaponItemInstance = m_characterEquipment.EquippedMeleeWeaponItemInstance;
		float num = 1f;
		if (m_characterInventory != null && m_characterInventory.Inventory.HasActiveArtifactEffect(GlobalReferences.Instance.ArtifactEffects.Generic.ReducedMeleeChargeTime, out float floatValue))
		{
			num -= floatValue;
		}
		for (int i = 0; i < m_chargeStateData.m_staminaRemaining.Length; i++)
		{
			float num2 = -1f;
			if (i < equippedMeleeWeaponItemInstance.WeaponDefinition.ChargeStates.Length)
			{
				num2 = equippedMeleeWeaponItemInstance.WeaponDefinition.ChargeStates[i].m_staminaChargeCost;
			}
			if (num2 > 0f)
			{
				num2 *= num;
			}
			m_chargeStateData.m_staminaRemaining[i] = num2;
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		GlobalReferences.Instance.EventChannels.Stamina.MeleeChargeState.Raise(null);
	}

	private void SetChargeLevel(int newChargeLevel)
	{
		m_currentChargeLevel = newChargeLevel;
		MeleeWeaponItemInstance equippedMeleeWeaponItemInstance = m_characterEquipment.EquippedMeleeWeaponItemInstance;
		int chargeAnimationIndex = equippedMeleeWeaponItemInstance.WeaponDefinition.ChargeStates[newChargeLevel].m_chargeAnimationIndex;
		if (chargeAnimationIndex != -1)
		{
			AnimationClip animationForChargeState = equippedMeleeWeaponItemInstance.WeaponDefinition.MoveSet.GetAnimationForChargeState(chargeAnimationIndex);
			if (animationForChargeState != null)
			{
				m_spriteAnim.Play(animationForChargeState);
			}
		}
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		float availableStamina = m_characterStamina.AvailableStamina;
		m_characterStamina.ConsumeStamina(CharacterStamina.StaminaEventType.MeleeCharge, Time.deltaTime);
		float availableStamina2 = m_characterStamina.AvailableStamina;
		float num = availableStamina - availableStamina2;
		for (int i = 0; i < m_chargeStateData.m_staminaRemaining.Length; i++)
		{
			if (!(m_chargeStateData.m_staminaRemaining[i] <= 0f))
			{
				m_chargeStateData.m_staminaRemaining[i] -= num;
				if (m_chargeStateData.m_staminaRemaining[i] <= 0f)
				{
					GlobalReferences.Instance.EventChannels.Stamina.MeleeChargeStateUpgrade.Raise();
					SetChargeLevel(i);
				}
			}
		}
		GlobalReferences.Instance.EventChannels.Stamina.MeleeChargeState.Raise(m_chargeStateData);
		if (!m_input.IsFiring)
		{
			m_inputHeld = false;
		}
		m_timer += Time.deltaTime;
		if ((m_spriteAnim.IsPlaying() || !m_characterStamina.IsExhausted) && (!(m_earlyOutTime > 0f) || !(m_timer > m_earlyOutTime) || m_inputHeld))
		{
			return;
		}
		if (m_characterStamina.IsExhausted)
		{
			if (GameUtils.IsPlayer(base.Owner))
			{
				GlobalReferences.Instance.EventChannels.Gameplay.InsufficientStamina.Raise();
			}
			base.Fsm.Event(m_exhaustedEvent);
		}
		else if (m_inputHeld || m_timer > m_earlyOutMaxTime)
		{
			if (m_currentChargeLevel != -1)
			{
				MeleeWeaponItemInstance equippedMeleeWeaponItemInstance = m_characterEquipment.EquippedMeleeWeaponItemInstance;
				base.Fsm.Event(equippedMeleeWeaponItemInstance.WeaponDefinition.ChargeStates[m_currentChargeLevel].m_attackFSMEvent);
			}
			else
			{
				base.Fsm.Event(m_normalAttackEvent);
			}
		}
		else
		{
			base.Fsm.Event(m_earlyReleaseEvent);
		}
		Finish();
	}
}
