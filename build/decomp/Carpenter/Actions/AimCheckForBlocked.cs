using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("Combat")]
public class AimCheckForBlocked : FsmStateAction
{
	[RequiredField]
	[HutongGames.PlayMaker.Tooltip("Character collider to use for collision check, should be the main movement collider")]
	public CapsuleCollider2D m_movementCollider;

	private CharacterDirection m_direction;

	private LegacyCharacterMovement m_movement;

	private CharacterInventory m_inventory;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_direction = base.Owner.GetComponent<CharacterDirection>();
			m_movement = base.Owner.GetComponent<LegacyCharacterMovement>();
			m_inventory = base.Owner.GetComponent<CharacterInventory>();
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
		Check();
		Finish();
	}

	private void Check()
	{
		ProjectileWeaponItemInstance equippedRangedItem = m_inventory.Inventory.EquippedRangedItem;
		if (equippedRangedItem != null)
		{
			int layerMask = ((!(m_movement != null)) ? GameLayers.EnvironmentMask : m_movement.EnvironmentLayerMask);
			float wallBlockedDistance = equippedRangedItem.WeaponSettings.WallBlockedDistance;
			if (AIUtilities.IsInFrontOfWall(m_movementCollider, m_direction.GetForwardVector(), layerMask, wallBlockedDistance))
			{
				base.Fsm.Event("Aim/BlockedGun");
			}
		}
	}
}
