using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.Input)]
public class InputDown : FsmStateAction
{
	[Tooltip("Event to trigger on the input")]
	public FsmEvent m_event;

	public float m_threshold;

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
		if (m_characterInput.MovementInput.y < 0f - m_threshold)
		{
			base.Fsm.Event(m_event);
		}
	}
}
