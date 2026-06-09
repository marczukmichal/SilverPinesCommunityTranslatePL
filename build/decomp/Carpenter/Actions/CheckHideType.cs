using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.Character)]
public class CheckHideType : FsmStateAction
{
	public FsmEvent m_onBushEvent;

	public FsmEvent m_onLockerEvent;

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
		BaseInteractable activeInteractable = m_interactor.ActiveInteractable;
		if (!(activeInteractable != null))
		{
			return;
		}
		InteractHide interactHide = activeInteractable as InteractHide;
		if (interactHide != null)
		{
			switch (interactHide.HideType)
			{
			case InteractHide.IntereactHideType.Bush:
				base.Fsm.Event(m_onBushEvent);
				break;
			case InteractHide.IntereactHideType.Locker:
				base.Fsm.Event(m_onLockerEvent);
				break;
			}
		}
	}
}
