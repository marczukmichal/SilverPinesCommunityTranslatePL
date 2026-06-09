using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("Interactor")]
public class CompleteInteractCommand : FsmStateAction
{
	public bool m_onEnter;

	public bool m_onExit;

	private CharacterInteractor m_interactor;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_interactor = base.Owner.GetComponent<CharacterInteractor>();
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
		if (m_onEnter)
		{
			m_interactor.InteractableCommandRequestFlag = CharacterInteractor.InteractableCommandFlag.EventDone;
		}
		Finish();
	}

	public override void OnExit()
	{
		base.OnExit();
		if (m_onExit)
		{
			m_interactor.InteractableCommandRequestFlag = CharacterInteractor.InteractableCommandFlag.EventDone;
		}
	}
}
