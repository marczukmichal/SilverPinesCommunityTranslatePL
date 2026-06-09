using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.Input)]
public class InputUp : FsmStateAction
{
	[Tooltip("Event to trigger on the input")]
	public FsmEvent m_event;

	public float m_threshold;

	public bool m_onEnterOnly;

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
		base.OnEnter();
		if (m_onEnterOnly)
		{
			Check();
			Finish();
		}
	}

	private void Check()
	{
		if (m_characterInput.MovementInput.y > m_threshold)
		{
			base.Fsm.Event(m_event);
		}
	}

	public override void OnUpdate()
	{
		Check();
	}
}
