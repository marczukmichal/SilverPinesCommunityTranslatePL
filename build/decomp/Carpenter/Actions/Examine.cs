using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.Character)]
public class Examine : FsmStateAction
{
	public VoidGameEventChannel m_showExaminingEventChannel;

	public VoidGameEventChannel m_cancelExaminingEventChannel;

	private CharacterInteractor m_interactor;

	private CharacterInputPlayer m_playerInput;

	[Tooltip("Event to send when examine is done.")]
	public FsmEvent m_examinedDoneEvent;

	[Tooltip("Event to send when examine is done but a follow up interact is queued.")]
	public FsmEvent m_examinedDoneFollowUpEvent;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_interactor = base.Owner.GetComponent<CharacterInteractor>();
			m_playerInput = base.Owner.GetComponent<CharacterInputPlayer>();
		}
	}

	public override void OnEnter()
	{
		m_showExaminingEventChannel.Raise();
		m_playerInput.SetInputDisabled(disabled: true);
	}

	public override void OnExit()
	{
		m_cancelExaminingEventChannel.Raise();
		m_playerInput.SetInputDisabled(disabled: false);
	}

	public override void OnUpdate()
	{
	}
}
