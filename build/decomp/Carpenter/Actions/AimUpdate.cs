using System;
using HutongGames.PlayMaker;
using UnityEngine;
using UnityEngine.Events;

namespace Actions;

[ActionCategory("Combat")]
public class AimUpdate : FsmStateAction
{
	public FsmEvent m_weaponMismatchEvent;

	public FsmEvent m_aimBlockedEvent;

	public FsmEvent m_aimUnblockedEvent;

	public FsmEvent m_aimUseSecondaryEvent;

	public bool m_useAnimationAim;

	public bool m_allowPumpAnimation;

	public bool m_cameraOnly;

	public bool m_checkForBlock;

	private CharacterAiming m_characterAiming;

	private BaseCharacterInput m_characterInput;

	private CharacterCameraFollow m_cameraFollow;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_characterAiming = base.Owner.GetComponent<CharacterAiming>();
			m_characterInput = base.Owner.GetCharacterInputComponent();
			m_cameraFollow = base.Owner.GetComponent<CharacterCameraFollow>();
		}
	}

	public override void OnEnter()
	{
		if (!m_cameraOnly)
		{
			CharacterAiming characterAiming = m_characterAiming;
			characterAiming.OnAimBlockedChanged = (UnityAction<bool>)Delegate.Combine(characterAiming.OnAimBlockedChanged, new UnityAction<bool>(OnAimBlockedChanged));
			m_characterAiming.AimInput = m_characterInput.AimDirectionInput;
			m_characterAiming.UseAimAnimationTime = m_useAnimationAim;
			m_characterAiming.AllowPumpAnimation = m_allowPumpAnimation;
			m_characterAiming.CheckForBlock = m_checkForBlock;
			m_characterAiming.AimingActive = true;
			BaseCharacterInput characterInput = m_characterInput;
			characterInput.OnFireAction = (UnityAction<bool>)Delegate.Combine(characterInput.OnFireAction, new UnityAction<bool>(FireInput));
			if (!m_characterAiming.IsSecondaryWeaponType())
			{
				m_characterAiming.SetWeaponFiring(m_characterInput.IsFiring);
			}
			OnAimBlockedChanged(m_characterAiming.AimBlocked);
		}
	}

	private void OnAimBlockedChanged(bool blocked)
	{
		if (blocked)
		{
			base.Fsm.Event(m_aimBlockedEvent);
		}
		else
		{
			base.Fsm.Event(m_aimUnblockedEvent);
		}
	}

	public override void OnUpdate()
	{
		m_cameraFollow.AimOffset = m_characterInput.AimDirectionInput;
		if (!m_cameraOnly)
		{
			m_characterAiming.AimInput = m_characterInput.AimDirectionInput;
			if (m_characterAiming.CheckForRangedWeaponMismatch())
			{
				base.Fsm.Event(m_weaponMismatchEvent);
			}
		}
	}

	public override void OnExit()
	{
		m_cameraFollow.AimOffset = Vector2.zero;
		if (!m_cameraOnly)
		{
			m_characterAiming.AimingActive = false;
			BaseCharacterInput characterInput = m_characterInput;
			characterInput.OnFireAction = (UnityAction<bool>)Delegate.Remove(characterInput.OnFireAction, new UnityAction<bool>(FireInput));
			CharacterAiming characterAiming = m_characterAiming;
			characterAiming.OnAimBlockedChanged = (UnityAction<bool>)Delegate.Remove(characterAiming.OnAimBlockedChanged, new UnityAction<bool>(OnAimBlockedChanged));
		}
	}

	private void FireInput(bool firing)
	{
		if (m_characterAiming.IsSecondaryWeaponType())
		{
			if (firing)
			{
				base.Fsm.Event(m_aimUseSecondaryEvent);
			}
		}
		else
		{
			m_characterAiming.SetWeaponFiring(firing);
		}
	}
}
