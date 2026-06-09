using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory(ActionCategory.Input)]
public class InputSprint : FsmStateAction
{
	[HutongGames.PlayMaker.Tooltip("Event to trigger on sprint start")]
	public FsmEvent m_onSprintEvent;

	[HutongGames.PlayMaker.Tooltip("Event to trigger on sprint end")]
	public FsmEvent m_onNotSprintEvent;

	public bool m_onlyOnce;

	private BaseCharacterInput m_characterInput;

	private CharacterFSMUtilities m_fsmUtilities;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_characterInput = base.Owner.GetCharacterInputComponent();
			m_fsmUtilities = base.Owner.GetComponent<CharacterFSMUtilities>();
		}
	}

	public override void OnEnter()
	{
		Check();
		if (m_onlyOnce)
		{
			Finish();
		}
	}

	public override void OnUpdate()
	{
		Check();
	}

	private void Check()
	{
		bool flag = m_characterInput.IsSprinting;
		if (Mathf.Abs(m_characterInput.MovementInput.x) < GameUtils.Constants.s_inputMoveDeadzoneMinValue)
		{
			flag = false;
		}
		if (m_fsmUtilities != null && !m_fsmUtilities.CanSprint())
		{
			flag = false;
		}
		if (m_onSprintEvent != null && flag)
		{
			base.Fsm.Event(m_onSprintEvent);
		}
		if (m_onNotSprintEvent != null && !flag)
		{
			base.Fsm.Event(m_onNotSprintEvent);
		}
	}
}
