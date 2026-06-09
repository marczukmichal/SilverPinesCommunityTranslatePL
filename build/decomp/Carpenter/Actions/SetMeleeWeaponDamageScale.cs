using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("Combat")]
public class SetMeleeWeaponDamageScale : FsmStateAction
{
	public enum ArtifactBonusMode
	{
		None,
		ComboFinisher,
		ChargeAttack,
		SprintAttack
	}

	public float m_damageScale = 1f;

	public float m_staggerScale = 1f;

	private CharacterEquipment m_characterEquipment;

	private CharacterInventory m_inventory;

	public ArtifactBonusMode m_artifactBonusMode;

	public override void Awake()
	{
		base.Awake();
		if (base.Owner != null)
		{
			m_characterEquipment = base.Owner.GetComponent<CharacterEquipment>();
			m_inventory = base.Owner.GetComponent<CharacterInventory>();
		}
	}

	public override void OnEnter()
	{
		float num = m_damageScale;
		float num2 = m_staggerScale;
		float floatValue3;
		if (m_artifactBonusMode == ArtifactBonusMode.ComboFinisher)
		{
			if (m_inventory.ArtifactInventory.HasActiveArtifactEffect(GlobalReferences.Instance.ArtifactEffects.Generic.ImprovedMeleeComboFinalHit, out float floatValue))
			{
				num *= 1f + floatValue;
				num2 *= 1f + floatValue * 2.5f;
			}
		}
		else if (m_artifactBonusMode == ArtifactBonusMode.ChargeAttack)
		{
			if (m_inventory.ArtifactInventory.HasActiveArtifactEffect(GlobalReferences.Instance.ArtifactEffects.Generic.IncreasedMeleeDamageChargeAttacks, out float floatValue2))
			{
				num *= 1f + floatValue2;
				num2 *= 1f + floatValue2 * 2.5f;
			}
		}
		else if (m_artifactBonusMode == ArtifactBonusMode.SprintAttack && m_inventory.ArtifactInventory.HasActiveArtifactEffect(GlobalReferences.Instance.ArtifactEffects.Generic.ImprovedRunningMelee, out floatValue3))
		{
			num *= 1f + floatValue3;
			num2 *= 1f + floatValue3 * 2.5f;
		}
		m_characterEquipment.SetCurrentMeleeWeaponDamageScale(num, num2);
	}
}
