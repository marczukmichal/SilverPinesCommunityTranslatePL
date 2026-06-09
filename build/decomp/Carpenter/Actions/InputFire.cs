using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.Input)]
public class InputFire : FsmStateAction
{
	[Tooltip("Event to trigger on Fire start")]
	public FsmEvent m_onFireEvent;

	[Tooltip("Event to trigger on Fire end")]
	public FsmEvent m_onNotFireEvent;

	private BaseCharacterInput m_characterInput;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_characterInput = base.Owner.GetCharacterInputComponent();
		}
	}

	public override void OnEnter()
	{
		Check();
	}

	public override void OnUpdate()
	{
		Check();
	}

	private void Check()
	{
		if (m_onFireEvent != null && m_characterInput.IsFiring)
		{
			base.Fsm.Event(m_onFireEvent);
		}
		if (m_onNotFireEvent != null && !m_characterInput.IsFiring)
		{
			base.Fsm.Event(m_onNotFireEvent);
		}
	}
}
