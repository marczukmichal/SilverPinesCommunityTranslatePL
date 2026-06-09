using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("Interactor")]
public class CheckForInteractCommandEvent : FsmStateAction
{
	private CharacterInteractor m_interactor;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_interactor = base.Owner.GetComponent<CharacterInteractor>();
		}
	}

	public override void OnUpdate()
	{
		if (m_interactor.InteractableCommandRequestFlag == CharacterInteractor.InteractableCommandFlag.RequestEvent)
		{
			string interactableCommandEventToSend = m_interactor.InteractableCommandEventToSend;
			base.Fsm.Event(interactableCommandEventToSend);
			m_interactor.InteractableCommandEventToSend = null;
		}
	}
}
