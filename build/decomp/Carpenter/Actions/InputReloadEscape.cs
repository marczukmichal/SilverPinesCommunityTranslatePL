using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory(ActionCategory.Input)]
public class InputReloadEscape : FsmStateAction
{
	public FsmEvent m_onExitReload;

	private BaseCharacterInput m_characterInput;

	private CharacterDirection m_direction;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_characterInput = base.Owner.GetCharacterInputComponent();
			m_direction = base.Owner.GetComponent<CharacterDirection>();
		}
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		bool flag = false;
		if (!m_characterInput.IsAiming && Mathf.Abs(m_characterInput.MovementInput.x) > 0.5f)
		{
			flag = true;
		}
		if (m_characterInput.IsFiring || m_characterInput.IsSecondaryFiring)
		{
			flag = true;
		}
		if (flag)
		{
			base.Fsm.Event(m_onExitReload);
		}
	}
}
