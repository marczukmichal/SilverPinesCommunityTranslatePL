using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.Input)]
public class InputAIAbility : FsmStateAction
{
	[Tooltip("Event to trigger on")]
	public FsmEvent m_onActiveEvent;

	public AIAbility m_aiAbility;

	private BaseCharacterInput m_characterInput;

	private AIFSMAttackStrategy m_strategy;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_characterInput = base.Owner.GetCharacterInputComponent();
			m_strategy = base.Owner.GetComponent<AIFSMAttackStrategy>();
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
		if (m_onActiveEvent != null && m_characterInput.IsDoingAIAbility(m_aiAbility))
		{
			if (m_strategy != null)
			{
				m_strategy.ConfirmAbility(m_aiAbility);
			}
			base.Fsm.Event(m_onActiveEvent);
		}
	}
}
