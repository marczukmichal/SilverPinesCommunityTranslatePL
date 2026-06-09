using UnityEngine;
using UnityEngine.Events;

public class CharacterMeleeBlock : MonoBehaviour
{
	public enum TryBlockResult
	{
		None,
		Blocked,
		Parried
	}

	[Tooltip("Minimum time passed required to count as being blocking from initial start of block state")]
	[SerializeField]
	private float m_blockTimeStart;

	[Tooltip("Minimum time passed required to count as being able to parry hits from initial start of block state")]
	[SerializeField]
	private float m_parryWindowTimeStart;

	[Tooltip("Maximum time passed required to count as being able to parry hits from initial start of block state")]
	[SerializeField]
	private float m_parryWindowTimeEnd;

	[SerializeField]
	private float m_parryStaminaCostScalar = 0.25f;

	private CharacterStamina m_stamina;

	private CharacterDirection m_direction;

	private CharacterEquipment m_equipment;

	private CharacterInventory m_inventory;

	private CharacterInputPlayer m_inputPlayer;

	private bool m_isBlocking;

	public UnityEvent m_onHitWhileBlocking;

	public UnityEvent m_onSuccesfulParry;

	private float m_blockStartTime;

	public void StartBlock()
	{
		m_isBlocking = true;
		m_blockStartTime = Time.time;
	}

	public void EndBlock()
	{
		m_isBlocking = false;
	}

	private void Start()
	{
		m_stamina = GetComponent<CharacterStamina>();
		m_direction = GetComponent<CharacterDirection>();
		m_equipment = GetComponent<CharacterEquipment>();
		m_inputPlayer = GetComponent<CharacterInputPlayer>();
		m_inventory = GetComponent<CharacterInventory>();
	}

	private void Update()
	{
		if (m_isBlocking)
		{
			m_stamina.ConsumeStamina(CharacterStamina.StaminaEventType.BlockContinous, Time.deltaTime);
		}
	}

	public TryBlockResult TryBlockAttack(Vector2 attackerPosition, int healthDamage, ref float healthScalar)
	{
		if (m_isBlocking && m_direction.IsFacingPoint(attackerPosition))
		{
			if (Time.time - m_blockStartTime < m_blockTimeStart)
			{
				return TryBlockResult.None;
			}
			float num = Time.time - m_inputPlayer.ParryInputTime;
			float num2 = m_parryWindowTimeEnd;
			if (m_inventory != null && m_inventory.ArtifactInventory.HasActiveArtifactEffect(GlobalReferences.Instance.ArtifactEffects.Generic.LargerParryWindow, out var floatValue, out var integerValue))
			{
				num2 += floatValue;
			}
			bool flag = num >= m_parryWindowTimeStart && num <= num2;
			float num3 = (float)healthDamage * m_stamina.GetBlockDamageMultiplier();
			if (m_equipment.EquippedMeleeWeaponItemInstance.IsBroken())
			{
				num3 *= 2f;
			}
			if (flag)
			{
				num3 *= m_parryStaminaCostScalar;
			}
			if (m_inventory != null && m_inventory.ArtifactInventory.HasActiveArtifactEffect(GlobalReferences.Instance.ArtifactEffects.Generic.ReducedBlockStaminaCost, out var floatValue2, out integerValue))
			{
				num3 *= floatValue2;
			}
			MeleeWeapon.WeaponDurabilityLossType lossType = ((!flag) ? MeleeWeapon.WeaponDurabilityLossType.Block : MeleeWeapon.WeaponDurabilityLossType.Parry);
			if (m_equipment.ActiveWeapon.WeaponDurabilityChangeEvent(lossType))
			{
				m_equipment.EnableMeleeWeapon();
			}
			if (m_stamina.IsExhausted)
			{
				m_stamina.ConsumeStamina(num3);
				return TryBlockResult.None;
			}
			m_stamina.ConsumeStamina(num3);
			if (m_equipment.EquippedMeleeWeaponItemInstance.WeaponDefinition.BlockAudioEvent != null)
			{
				m_equipment.EquippedMeleeWeaponItemInstance.WeaponDefinition.BlockAudioEvent.Play(base.transform.position);
			}
			healthScalar = 0f;
			if (flag)
			{
				m_onSuccesfulParry.Invoke();
				return TryBlockResult.Parried;
			}
			m_onHitWhileBlocking.Invoke();
			return TryBlockResult.Blocked;
		}
		return TryBlockResult.None;
	}
}
