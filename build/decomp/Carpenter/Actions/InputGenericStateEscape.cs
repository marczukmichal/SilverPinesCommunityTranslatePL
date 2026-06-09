using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory(ActionCategory.Input)]
public class InputGenericStateEscape : FsmStateAction
{
	public FsmEvent m_inputEvent = new FsmEvent("Reset");

	public float m_minimumTimeInState;

	public bool m_includeAim = true;

	public bool m_includeJump = true;

	public bool m_includeCrouch = true;

	public bool m_includeFiring = true;

	public bool m_includeMovement = true;

	private BaseCharacterInput m_characterInput;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_characterInput = base.Owner.GetCharacterInputComponent();
		}
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (!(base.State.StateTime < m_minimumTimeInState))
		{
			bool flag = false;
			if (m_includeAim && m_characterInput.IsAiming)
			{
				flag = true;
			}
			if (m_includeMovement && Mathf.Abs(m_characterInput.MovementInput.x) > 0.5f)
			{
				flag = true;
			}
			if (m_includeFiring && (m_characterInput.IsFiring || m_characterInput.IsSecondaryFiring))
			{
				flag = true;
			}
			if (m_includeCrouch && m_characterInput.IsCrouching)
			{
				flag = true;
			}
			if (m_includeJump && m_characterInput.IsJumping)
			{
				flag = true;
			}
			if (flag)
			{
				base.Fsm.Event(m_inputEvent);
			}
		}
	}
}
