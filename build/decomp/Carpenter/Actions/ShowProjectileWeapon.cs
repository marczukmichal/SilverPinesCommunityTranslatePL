using System;
using HutongGames.PlayMaker;
using UnityEngine.Events;

namespace Actions;

[ActionCategory("Combat")]
public class ShowProjectileWeapon : FsmStateAction
{
	public float m_zOffset;

	public bool m_allowSwap = true;

	public bool m_allowFiring;

	private CharacterAiming m_characterAiming;

	private BaseCharacterInput m_characterInput;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_characterAiming = base.Owner.GetComponent<CharacterAiming>();
			m_characterInput = base.Owner.GetCharacterInputComponent();
		}
	}

	public override void OnEnter()
	{
		m_characterAiming.ShowWeaponOnWeaponAnimNode(enable: true, m_zOffset, m_allowSwap);
		if (m_allowFiring)
		{
			BaseCharacterInput characterInput = m_characterInput;
			characterInput.OnFireAction = (UnityAction<bool>)Delegate.Combine(characterInput.OnFireAction, new UnityAction<bool>(FireInput));
		}
		Finish();
	}

	public override void OnExit()
	{
		m_characterAiming.ShowWeaponOnWeaponAnimNode(enable: false, m_zOffset, m_allowSwap);
		if (m_allowFiring)
		{
			BaseCharacterInput characterInput = m_characterInput;
			characterInput.OnFireAction = (UnityAction<bool>)Delegate.Remove(characterInput.OnFireAction, new UnityAction<bool>(FireInput));
		}
	}

	private void FireInput(bool firing)
	{
		if (!m_characterAiming.IsSecondaryWeaponType())
		{
			m_characterAiming.SetWeaponFiring(firing);
		}
	}
}
