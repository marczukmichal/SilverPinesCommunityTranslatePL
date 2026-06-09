using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("Combat")]
public class SetAimStateAnimationVariable : FsmStateAction
{
	public enum AimAnimOverrideType
	{
		None,
		AimStart,
		AimStartArm,
		AimEnd,
		AimEndArm,
		AimBlocked
	}

	[UIHint(UIHint.Variable)]
	public FsmObject m_animationVariable;

	public AimAnimOverrideType m_aimAnimType;

	public AnimationClip m_rangedClip;

	public AnimationClip m_throwingClip;

	public bool m_getPreviousAimState;

	private CharacterInventory m_inventory;

	private CharacterAiming m_aiming;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_inventory = base.Owner.GetComponent<CharacterInventory>();
			m_aiming = base.Owner.GetComponent<CharacterAiming>();
		}
	}

	public override void OnEnter()
	{
		AnimationClip animationClip = m_rangedClip;
		if (m_getPreviousAimState)
		{
			if (m_aiming.ActiveAimType == CharacterAiming.AimType.Throw)
			{
				animationClip = m_throwingClip;
			}
		}
		else if (m_inventory.Inventory.EquippedPrimaryItem != null && m_inventory.Inventory.EquippedPrimaryItem is SecondaryWeaponItemInstance)
		{
			animationClip = m_throwingClip;
		}
		if (animationClip != m_throwingClip && m_inventory.Inventory.EquippedPrimaryItem != null && m_inventory.Inventory.EquippedPrimaryItem is ProjectileWeaponItemInstance projectileWeaponItemInstance)
		{
			switch (m_aimAnimType)
			{
			case AimAnimOverrideType.AimStart:
				if ((bool)projectileWeaponItemInstance.WeaponSettings.AimStartAnimOverride)
				{
					animationClip = projectileWeaponItemInstance.WeaponSettings.AimStartAnimOverride;
				}
				break;
			case AimAnimOverrideType.AimStartArm:
				if ((bool)projectileWeaponItemInstance.WeaponSettings.AimStartArmAnimOverride)
				{
					animationClip = projectileWeaponItemInstance.WeaponSettings.AimStartArmAnimOverride;
				}
				break;
			case AimAnimOverrideType.AimEnd:
				if ((bool)projectileWeaponItemInstance.WeaponSettings.AimEndAnimOverride)
				{
					animationClip = projectileWeaponItemInstance.WeaponSettings.AimEndAnimOverride;
				}
				break;
			case AimAnimOverrideType.AimEndArm:
				if ((bool)projectileWeaponItemInstance.WeaponSettings.AimEndArmAnimOverride)
				{
					animationClip = projectileWeaponItemInstance.WeaponSettings.AimEndArmAnimOverride;
				}
				break;
			case AimAnimOverrideType.AimBlocked:
				if ((bool)projectileWeaponItemInstance.WeaponSettings.AimBlockedAnimOverride)
				{
					animationClip = projectileWeaponItemInstance.WeaponSettings.AimBlockedAnimOverride;
				}
				break;
			}
		}
		m_animationVariable.Value = animationClip;
		Finish();
	}
}
