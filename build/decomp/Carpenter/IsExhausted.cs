using HutongGames.PlayMaker;

[ActionCategory(ActionCategory.Character)]
public class IsExhausted : FsmStateAction
{
	public FsmOwnerDefault m_gameObject;

	public FsmEvent m_exhausedEvent;

	public FsmEvent m_notExhaustedEvent;

	public bool m_sendLowStaminaUIEvent = true;

	public bool m_everyFrame;

	public override void OnEnter()
	{
		Check();
		if (!m_everyFrame)
		{
			Finish();
		}
	}

	public override void OnUpdate()
	{
		if (m_everyFrame)
		{
			Check();
		}
	}

	private void Check()
	{
		if (base.Fsm.GetOwnerDefaultTarget(m_gameObject).GetComponent<CharacterStamina>().IsExhausted)
		{
			if (m_sendLowStaminaUIEvent && GameUtils.IsPlayer(base.Owner))
			{
				GlobalReferences.Instance.EventChannels.Gameplay.InsufficientStamina.Raise();
			}
			base.Fsm.Event(m_exhausedEvent);
		}
		else
		{
			base.Fsm.Event(m_notExhaustedEvent);
		}
	}
}
