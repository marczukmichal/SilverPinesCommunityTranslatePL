using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("Inventory")]
public class WeaponPumpAction : FsmStateAction
{
	private CharacterInventory m_inventory;

	private CharacterAiming m_aiming;

	private float m_targetTime;

	private float m_timer;

	private float m_timeScalar = 1f;

	public override void Awake()
	{
		base.Awake();
		if (base.Owner != null)
		{
			m_inventory = base.Owner.GetComponent<CharacterInventory>();
			m_aiming = base.Owner.GetComponent<CharacterAiming>();
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_timer = 0f;
		m_targetTime = m_inventory.Inventory.EquippedRangedItem.WeaponSettings.ActionFSMTime;
		m_timeScalar = 1f;
		if (m_inventory != null && m_inventory.ArtifactInventory.HasActiveArtifactEffect(GlobalReferences.Instance.ArtifactEffects.Generic.RateOfFire, out float floatValue))
		{
			m_timeScalar *= 1f + floatValue;
		}
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		m_timer += Time.deltaTime * m_timeScalar;
		if (m_timer > m_targetTime)
		{
			m_aiming.PerformPumpFromAnimation(ejectShellCasing: true);
			Finish();
		}
	}
}
