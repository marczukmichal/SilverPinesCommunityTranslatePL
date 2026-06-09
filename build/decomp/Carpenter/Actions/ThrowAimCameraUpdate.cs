using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("Combat")]
public class ThrowAimCameraUpdate : FsmStateAction
{
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

	public override void OnUpdate()
	{
		if (m_characterAiming.UseAimForSecondary)
		{
			m_cameraFollow.AimOffset = m_characterInput.AimDirectionInput;
		}
	}

	public override void OnExit()
	{
		m_cameraFollow.AimOffset = Vector2.zero;
	}
}
