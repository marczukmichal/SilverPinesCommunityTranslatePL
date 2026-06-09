using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.Character)]
public class CheckForTargetedLeap : FsmStateAction
{
	protected CharacterTargetedLeap m_targetedLeap;

	protected BaseCharacterInput m_input;

	protected LegacyCharacterMovement m_movement;

	[Tooltip("Event to send if vertical leap is attempted to be performed.")]
	public FsmEvent m_verticalLeapAction;

	[Tooltip("Event to send if horizontal leap is attempted to be performed.")]
	public FsmEvent m_horizontalLeapAction;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_targetedLeap = base.Owner.GetComponent<CharacterTargetedLeap>();
			m_input = base.Owner.GetCharacterInputComponent();
			m_movement = base.Owner.GetComponent<LegacyCharacterMovement>();
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_targetedLeap.IsChecking = true;
	}

	public override void OnExit()
	{
		base.OnExit();
		m_targetedLeap.IsChecking = false;
	}

	public override void OnUpdate()
	{
		CharacterTargetedLeap.TargetedLeapType availableLeapType = m_targetedLeap.AvailableLeapType;
		if (availableLeapType != 0 && m_input.IsJumping)
		{
			m_targetedLeap.StoreCurrentLeapPosition();
			switch (availableLeapType)
			{
			case CharacterTargetedLeap.TargetedLeapType.VerticalUpwards:
				base.Fsm.Event(m_verticalLeapAction);
				break;
			case CharacterTargetedLeap.TargetedLeapType.Horizontal:
				base.Fsm.Event(m_horizontalLeapAction);
				break;
			}
			Finish();
		}
	}
}
